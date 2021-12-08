using OrmRepositoryUnitOfWork.Attributes;
using OrmRepositoryUnitOfWork.Enums;
using OrmRepositoryUnitOfWork.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace OrmRepositoryUnitOfWork.Repositories
{
    public class GenericRepos<T> : IRepository<T>
    {
        private List<string> sqlQueries = new List<string>();

        private readonly string typeTableName;
        private readonly Assembly assembly;

        private SqlGenerator sqlGenerator;
        private SqlTransaction? transaction;
        private bool disposed;
        private SqlConnection connection;
        private Type type;
        private AttributeChecker attributeChecker;

        public GenericRepos(SqlConnection sqlConnection)
        {
            this.attributeChecker = new AttributeChecker();
            this.connection = sqlConnection;
            this.sqlGenerator = new SqlGenerator();
            this.assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Any());
            if (this.assembly == null)
            {
                throw new ArgumentNullException($"The [{nameof(DomainModelAttribute)}] was not set to your assembly");
            }
            this.type = typeof(T);
            CheckAttributes(this.type);
            this.typeTableName = this.type.GetCustomAttribute<TableAttribute>().TableName;
        }

        #region Methods

        #region Methods.Public

        public void Create<TItem>(ref TItem item)
        {
            if (item == null)
            {
                return;
            }
            using (this.transaction = this.connection.BeginTransaction())
            {
                try
                {
                    InsertAlgorithm(ref item);
                    if (item == null)
                    {
                        return;
                    }
                    var baseType = item.GetType();

                    var primaryKeyProperty = GetPrimaryKeyProperty(baseType);
                    var itemPrimaryKeyValue = (int?)primaryKeyProperty.GetValue(item);

                    InsertRelatedDataOnly(ref item, itemPrimaryKeyValue, baseType);
                    this.transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        this.transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    throw;
                }
            }
        }

        public void CreateItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var refitem = item;
                Create(ref refitem);
            }
        }

        public T ReadItemById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Incorrect ID value. Please check.");
            }
            var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(type, id), this.connection);
            using (var sqlReader = command.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        return (T)MatchDataItem(type, sqlReader);
                    }
                }
                return default;
            }
        }

        public IEnumerable<T> ReadItems()
        {
            var readedItems = new List<T>();
            var properties = this.type.GetProperties()
                                        .Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());

            var isTypeHasCollectionInside = IsTypeHasCollectionsInside(properties);
            try
            {
                if (isTypeHasCollectionInside)
                {
                    var primaryKeysValues = SelectPrimaryKeyValues(this.type);

                    foreach (var primaryKey in primaryKeysValues)
                    {
                        var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(this.type, primaryKey), this.connection);
                        readedItems = GetReadedItems(readedItems, this.type, command);
                    }
                }
                else
                {
                    var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(this.type), this.connection);
                    readedItems = GetReadedItems(readedItems, this.type, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return readedItems;
        }

        public void Update(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"{nameof(item)} was null");
            }
            var primaryKeyProperty = GetPrimaryKeyProperty(this.type);
            if (primaryKeyProperty == null)
            {
                return;
            }
            var primaryKeyColumnValue = (int?)primaryKeyProperty.GetValue(item);
            if (primaryKeyColumnValue != null && primaryKeyColumnValue > default(int))
            {
                var sqlUpdateQuery = this.sqlGenerator.GetUpdateSqlQuery(item);
                this.sqlQueries.Add(sqlUpdateQuery);
            }
            else
            {
                throw new Exception("Primary key value was not set (or equals to 0)");
            }
        }

        public void UpdateBy(T item, string columnName, object value)
        {
            var sqlUpdateQuery = this.sqlGenerator.GetUpdateSqlQuery(item, columnName, value);
            this.sqlQueries.Add(sqlUpdateQuery);
        }

        public void DeleteById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Incorrect ID value. Please check.");
            }
            var sqlQuery = this.sqlGenerator.GetDeleteSqlQuery(this.typeTableName, id);
            this.sqlQueries.Add(sqlQuery);
        }

        public void Delete(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"{nameof(item)} was null");
            }
            var primaryColumnProperty = GetPrimaryKeyProperty(this.type);

            if (primaryColumnProperty != null && (int?)primaryColumnProperty.GetValue(item) > default(int))
            {
                var sqlQuery = this.sqlGenerator.GetDeleteSqlQuery(item);
                this.sqlQueries.Add(sqlQuery);
            }
        }

        public void Delete(string columnName, dynamic value)
        {
            var sqlPrimaryKeyValues = this.sqlGenerator.GetSelectIdForItem(typeof(T), columnName, value);
            var command = new SqlCommand(sqlPrimaryKeyValues, this.connection);
            var primaryKeys = new List<int>();

            var primaryKeyProperty = GetPrimaryKeyProperty(typeof(T));
            if (primaryKeyProperty == null)
            {
                return;
            }

            var primaryKeyColumnName = primaryKeyProperty.GetCustomAttribute<ColumnAttribute>()?.ColumnName;

            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        primaryKeys.Add((int)reader[primaryKeyColumnName]);
                    }
                }
            }
            var sqlQuery = this.sqlGenerator.GetDeleteSqlQuery<T>(columnName, value, primaryKeys);
            this.sqlQueries.Add(sqlQuery);
        }

        public void Submit(SqlTransaction transaction)
        {
            foreach (var command in this.sqlQueries)
            {
                var sqlCommand = new SqlCommand(command, this.connection)
                {
                    Transaction = transaction
                };
                sqlCommand.ExecuteNonQuery();
            }
        }

        #endregion Methods.Public

        #region Methods.Private

        private bool IsTypeHasCollectionsInside(IEnumerable<PropertyInfo> properties)
        {
            bool hasCollectionInside = false;
            foreach (var property in properties)
            {
                var propertyAttribute = property.GetCustomAttribute<RelatedEntityAttribute>();
                if (propertyAttribute == null)
                {
                    continue;
                }
                if (propertyAttribute.IsCollection)
                {
                    hasCollectionInside = true;
                    break;
                }
            }
            return hasCollectionInside;
        }

        private List<int> SelectPrimaryKeyValues(Type type)
        {
            var primaryKeyValues = new List<int>();
            var selectAllDataForSpecificType = sqlGenerator.SelectFromSingleTableSqlQuery(type);
            var primaryKeyProperty = GetPrimaryKeyProperty(type);

            if (primaryKeyProperty == null)
            {
                return primaryKeyValues;
            }

            var primaryColumnName = primaryKeyProperty.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
            primaryColumnName = $"{this.typeTableName}{primaryColumnName}";
            var command = new SqlCommand(selectAllDataForSpecificType, this.connection)
            {
                Transaction = this.transaction
            };

            using (var sqlReader = command.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        primaryKeyValues.Add((int)sqlReader[primaryColumnName]);
                    }
                }
            }
            return primaryKeyValues;
        }

        private void InsertRelatedDataOnly<TItem>(ref TItem? item, int? baseTypeId, Type baseType)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"{nameof(item)} was null");
            }
            var type = item.GetType();

            CheckAttributes(type);

            var typeAttribute = type.GetCustomAttribute<TableAttribute>();
            if (typeAttribute != null && typeAttribute.IsRelatedTable)
            {
                var properties = type.GetProperties();

                if (type == baseType)
                {
                    properties.FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                        && property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Primary)?
                                .SetValue(item, baseTypeId);
                }
                else
                {
                    properties.FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                        && property.GetCustomAttribute<ColumnAttribute>()?.BaseType == baseType)?
                                .SetValue(item, baseTypeId);
                }

                var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                var sqlCommand = new SqlCommand(sqlInsert, this.connection)
                {
                    Transaction = this.transaction
                };
                sqlCommand.ExecuteScalar();
            }

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (childs.Any())
            {
                foreach (var child in childs)
                {
                    if (child.GetValue(item) == null)
                    {
                        continue;
                    }

                    dynamic? childInstance = child.GetValue(item);
                    var childAttribute = child.GetCustomAttribute<RelatedEntityAttribute>();
                    if (childAttribute != null && childAttribute.IsCollection)
                    {
                        if (child.PropertyType.GetInterface(nameof(IDictionary<object, object>)) != null)
                        {
                            var keys = childInstance?.Keys;
                            if (keys == null)
                            {
                                continue;
                            }
                            foreach (var key in keys)
                            {
                                var keyInstance = key;
                                var valueInstance = childInstance?[key];
                                InsertRelatedDataOnly(ref keyInstance, baseTypeId, baseType);
                                InsertRelatedDataOnly(ref valueInstance, baseTypeId, baseType);
                            }
                        }
                        else
                        {
                            foreach (var element in childInstance)
                            {
                                var elementInstance = element;
                                InsertRelatedDataOnly(ref elementInstance, baseTypeId, baseType);
                            }
                        }
                    }
                    else
                    {
                        InsertRelatedDataOnly(ref childInstance, baseTypeId, baseType);
                    }
                    SetValueIntoProperty(ref item, childInstance, child);
                }
            }
        }

        private void CheckAttributes(Type type)
        {
            this.attributeChecker.CheckTableAttribute(type);
            this.attributeChecker.CheckColumnAttribute(type);
        }

        private void InsertAlgorithm<TItem>(ref TItem? item)
        {
            if (item == null)
            {
                return;
            }
            var type = item.GetType();
            var typeAttribute = type.GetCustomAttribute<TableAttribute>();

            if (typeAttribute != null && !typeAttribute.IsRelatedTable)
            {
                var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                var sqlCommand = new SqlCommand(sqlInsert, this.connection)
                {
                    Transaction = this.transaction
                };

                try
                {
                    int itemId = (int)sqlCommand.ExecuteScalar();

                    var primaryKeyProperty = GetPrimaryKeyProperty(type);
                    if (primaryKeyProperty != null)
                    {
                        SetValueIntoProperty(ref item, itemId, primaryKeyProperty);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            WorkingWithRelatedEntities(ref item, type);
        }

        private void WorkingWithRelatedEntities<TItem>(ref TItem? item, Type type)
        {
            if (item == null)
            {
                return;
            }
            var realtedEntities = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (realtedEntities.Any())
            {
                foreach (var realtedEntity in realtedEntities)
                {
                    if (realtedEntity.GetValue(item) == null)
                    {
                        continue;
                    }

                    dynamic? relatedEntityInstance = realtedEntity.GetValue(item);
                    var relatedEntityAttribute = realtedEntity.GetCustomAttribute<RelatedEntityAttribute>();

                    if (relatedEntityAttribute != null && relatedEntityAttribute.IsCollection)
                    {
                        WorkingWithRelatedEntityCollection(realtedEntity, relatedEntityInstance);
                    }
                    else
                    {
                        InsertAlgorithm(ref relatedEntityInstance);
                    }

                    SetValueIntoProperty(ref item, relatedEntityInstance, realtedEntity);
                }
            }
        }

        private void WorkingWithRelatedEntityCollection(PropertyInfo child, dynamic childInstance)
        {
            if (child.PropertyType.GetInterface(nameof(IDictionary<object, object>)) != null)
            {
                var keys = childInstance.Keys;
                foreach (var key in keys)
                {
                    var keyInstance = key;
                    var valueInstance = childInstance[key];

                    InsertAlgorithm(ref keyInstance);
                    InsertAlgorithm(ref valueInstance);
                }
            }
            else
            {
                foreach (var element in childInstance)
                {
                    var elementInstance = element;
                    InsertAlgorithm(ref elementInstance);
                }
            }
        }

        private void SetValueIntoProperty<TItem>(ref TItem? item, object value, PropertyInfo property)
        {
            if (item == null)
            {
                return;
            }
            var type = item.GetType();
            if (type.IsValueType)
            {
                object boxedItem = item;
                property.SetValue(boxedItem, value);
                item = (TItem)boxedItem;
            }
            else
            {
                property.SetValue(item, value);
            }
        }

        private object MatchDataItem(Type type, SqlDataReader sqlReader)
        {
            var item = GetItemInstance(type, sqlReader);
            if (item == null)
            {
                return item;
            }
            item = FillProperties(item, type, sqlReader);
            item = FillChilds(item, type, sqlReader);

            return item;
        }

        private object GetItemInstance(Type type, SqlDataReader sqlReader)
        {
            object? item;
            if (type.IsAbstract)
            {
                item = GetDerivedClass(sqlReader, type);
            }
            else
            {
                item = Activator.CreateInstance(type);
            }
            return item;
        }

        private dynamic FillChilds(object item, Type type, SqlDataReader sqlReader)
        {
            var relatedEntities = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());

            if (!relatedEntities.Any())
            {
                return item;
            }

            foreach (var relatedEintity in relatedEntities)
            {
                var relatedEntityAttribute = relatedEintity.GetCustomAttribute<RelatedEntityAttribute>();
                if (relatedEntityAttribute == null)
                {
                    continue;
                }
                if (relatedEntityAttribute.IsCollection)
                {
                    relatedEintity.SetValue(item, GetFilledCollection(relatedEintity, sqlReader));
                }
                else
                {
                    var relatedEintityType = relatedEntityAttribute.RelatedType;
                    if (relatedEintityType == null)
                    {
                        throw new Exception($"[{nameof(RelatedEntityAttribute)}].RelatedType was not set, or was set as null");
                    }
                    relatedEintity.SetValue(item, MatchDataItem(relatedEintityType, sqlReader));
                }
            }
            return item;
        }

        private object? GetFilledCollection(PropertyInfo relatedEntity, SqlDataReader sqlReader)
        {
            var type = relatedEntity.PropertyType;
            var collection = Activator.CreateInstance(type);
            var genericTypes = relatedEntity.PropertyType.GenericTypeArguments;
            var itemsCollection = new List<object>();

            object item;
            if (sqlReader.HasRows)
            {
                do
                {
                    foreach (var generic in genericTypes)
                    {
                        item = MatchDataItem(generic, sqlReader);
                        if (item == null)
                        {
                            continue;
                        }
                        itemsCollection.Add(item);
                    }
                    if (itemsCollection.Count != 0)
                    {
                        var methodAdd = relatedEntity.PropertyType.GetMethod("Add");
                        if (methodAdd != null)
                        {
                            methodAdd.Invoke(collection, itemsCollection.ToArray());
                        }
                        itemsCollection.Clear();
                    }
                } while (sqlReader.Read());
            }
            return collection;
        }

        private object GetDerivedClass(SqlDataReader sqlReader, Type baseType)
        {
            var assemblyInheritedTypes = this.assembly.GetTypes().Where(assemblyType => assemblyType.GetCustomAttributes<InheritanceRelationAttribute>().Any());

            IsAnyItemsWereFound(assemblyInheritedTypes, $"There are no types that marked by [{ nameof(InheritanceRelationAttribute)}] attribute.");

            var derivedClasses = assemblyInheritedTypes.Where(assemblyType => assemblyType.BaseType == baseType);

            IsAnyItemsWereFound(derivedClasses, $"There are no derived classes from \"{baseType.Name}\" type.");

            var derivedClassesWithTableAttribute = derivedClasses.Where(derivedType => derivedType.GetCustomAttributes<TableAttribute>().Any());

            IsAnyItemsWereFound(derivedClassesWithTableAttribute, $"There are no derived classes that marked by [{nameof(TableAttribute)}] attribute.");

            var derivedClassesWithTypeAttribute = derivedClassesWithTableAttribute.Where(derivedType => derivedType.GetCustomAttributes<TypeAttribute>().Any());

            IsAnyItemsWereFound(derivedClassesWithTypeAttribute, $"There are no derived classes that marked by [{nameof(TypeAttribute)}] attribute.");

            var tableName = derivedClassesWithTypeAttribute.First().GetCustomAttribute<TableAttribute>()?.TableName;
            var matchingColumnName = derivedClassesWithTypeAttribute.First().GetCustomAttribute<TypeAttribute>()?.ColumnMatching;

            if (tableName != null && matchingColumnName != null && matchingColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                matchingColumnName = $"{tableName}{matchingColumnName}";
            }

            if (sqlReader[matchingColumnName].GetType() == typeof(DBNull))
            {
                return null;
            }

            var type = derivedClassesWithTypeAttribute.First(derivedType => derivedType.GetCustomAttribute<TypeAttribute>()?.TypeID == (int)sqlReader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private static void IsAnyItemsWereFound(IEnumerable<Type> items, string exceptionMessage)
        {
            if (!items.Any())
            {
                throw new Exception(exceptionMessage);
            }
        }

        private object FillProperties(object item, Type type, SqlDataReader sqlReader)
        {
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());

            foreach (var property in properties)
            {
                var propertyAttribute = property.GetCustomAttribute<ColumnAttribute>();
                var propertyReflectedTypeAttribute = property?.ReflectedType?.GetCustomAttribute<TableAttribute>();
                if (property == null || propertyAttribute == null || propertyReflectedTypeAttribute == null)
                {
                    continue;
                }
                string columnName = propertyAttribute.ColumnName;

                if (columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    columnName = $"{propertyReflectedTypeAttribute.TableName}{columnName}";
                }

                if (sqlReader[columnName].GetType() != typeof(DBNull))
                {
                    property.SetValue(item, sqlReader[columnName]);
                }
                else
                {
                    property.SetValue(item, null);
                }
            }
            return item;
        }

        private List<T> GetReadedItems(List<T> readedItems, Type type, SqlCommand command)
        {
            using (var sqlReader = command.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        var resultItem = MatchDataItem(type, sqlReader);
                        if (resultItem != null)
                        {
                            readedItems.Add((T)resultItem);
                        }
                    }
                }
            }
            return readedItems;
        }

        private PropertyInfo GetPrimaryKeyProperty(Type type)
        {
            var properties = type.GetProperties();
            var primaryKeyProperty = properties.FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Primary);

            if (primaryKeyProperty == null)
            {
                throw new Exception($"[{nameof(ColumnAttribute)}].{nameof(KeyType)} was not set to the primary key property");
            }
            return primaryKeyProperty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        #endregion Methods.Private

        #endregion Methods
    }
}