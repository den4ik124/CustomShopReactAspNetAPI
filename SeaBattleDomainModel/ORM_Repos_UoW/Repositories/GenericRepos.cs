using OrmRepositoryUnitOfWork.Attributes;
using OrmRepositoryUnitOfWork.Enums;
using OrmRepositoryUnitOfWork.Interfaces;
using ReflectionExtensions;
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

        private readonly string? typeTableName;
        private Assembly assembly;

        private SqlGenerator sqlGenerator;
        private ILogger logger;
        private SqlTransaction transaction;
        private bool disposed = false;

        public GenericRepos(ILogger logger)
        {
            this.sqlGenerator = new SqlGenerator();
            this.assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Any());
            this.typeTableName = typeof(T).GetCustomAttribute<TableAttribute>()?.TableName;
            this.logger = logger;
        }

        #region Methods

        #region Methods.Public

        public void Create<TItem>(ref TItem item, SqlConnection connection)
        {
            try
            {
                using (this.transaction = connection.BeginTransaction())
                {
                    try
                    {
                        InsertAlgorithm(ref item, connection);

                        var baseType = item.GetType();
                        int? itemPrimaryKeyValue = (int)item.GetType()
                                                            .GetProperties()
                                                            .FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                                            .GetValue(item);

                        InsertRelatedDataOnly(ref item, connection, itemPrimaryKeyValue, baseType);
                        this.transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(ex.Message);
                        try
                        {
                            this.transaction.Rollback();
                        }
                        catch (Exception innerEx)
                        {
                            this.logger.Log(innerEx.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void Create(IEnumerable<T> items, SqlConnection connection)
        {
            foreach (var item in items)
            {
                var refitem = item;
                Create(ref refitem, connection);
            }
        }

        public T ReadItemById(int id, SqlConnection connection)
        {
            var type = typeof(T);
            try
            {
                var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(type, id), connection);
                using (var sqlReader = command.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            return (T)MatchDataItem(type, sqlReader);
                        }
                    }
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
            return default(T);
        }

        public IEnumerable<T> ReadItems(SqlConnection connection)
        {
            var readedItems = new List<T>();
            var type = typeof(T);
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());

            var isTypeHasCollectionInside = IsTypeHasCollectionsInside(properties);
            try
            {
                if (isTypeHasCollectionInside)
                {
                    var primaryKeysValues = SelectPrimaryKeyValues(type, connection);

                    foreach (var primaryKey in primaryKeysValues)
                    {
                        var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(type, primaryKey), connection);
                        readedItems = GetReadedItems(readedItems, type, command);
                    }
                }
                else
                {
                    var command = new SqlCommand(this.sqlGenerator.GetSelectJoinString(type), connection);
                    readedItems = GetReadedItems(readedItems, type, command);
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
            return readedItems;
        }

        public void Update(T item)
        {
            var type = item.GetType();
            var primaryKeyColumn = (int)type.GetProperties().FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
            if (primaryKeyColumn > 0)
            {
                var sqlUpdateQuery = this.sqlGenerator.GetUpdateSqlQuery(item);
                this.sqlQueries.Add(sqlUpdateQuery);
            }
            else
            {
                throw new Exception("Primary key value was not set");
            }
        }

        public void UpdateBy(T item, string columnName, object value)
        {
            var sqlUpdateQuery = this.sqlGenerator.GetUpdateSqlQuery(item, columnName, value);
            this.sqlQueries.Add(sqlUpdateQuery);
        }

        public void DeleteById(int id)
        {
            var sqlQuery = this.sqlGenerator.GetDeleteSqlQuery(this.typeTableName, id);
            this.sqlQueries.Add(sqlQuery);
        }

        public void Delete(T item)
        {
            var type = typeof(T);
            var primaryColumnProperty = type.GetProperties().FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
            var id = (int)primaryColumnProperty.GetValue(item);
            if (id > 0)
            {
                var sqlQuery = this.sqlGenerator.GetDeleteSqlQuery(item);
                this.sqlQueries.Add(sqlQuery);
            }
        }

        public void Delete(string columnName, dynamic value, SqlConnection connection)
        {
            var sqlPrimaryKeyValues = this.sqlGenerator.GetSelectIdForItem(typeof(T), columnName, value);
            var command = new SqlCommand(sqlPrimaryKeyValues, connection);
            var primaryKeys = new List<int>();
            var primaryKeyColumnName = typeof(T).GetProperties().FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                                && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                            .GetCustomAttribute<ColumnAttribute>().ColumnName;
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

        public void Submit(SqlConnection connection, SqlTransaction transaction)
        {
            foreach (var command in this.sqlQueries)
            {
                var sqlCommand = new SqlCommand(command, connection);
                sqlCommand.Transaction = transaction;
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
                if (property.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                {
                    hasCollectionInside = true;
                    break;
                }
            }

            return hasCollectionInside;
        }

        private List<int> SelectPrimaryKeyValues(Type type, SqlConnection connection)
        {
            var selectAllDataForSpecificType = sqlGenerator.SelectFromSingleTableSqlQuery(type);
            var primaryColumnName = type.GetProperties()
                                        .FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                        .GetCustomAttribute<ColumnAttribute>().ColumnName;
            primaryColumnName = $"{this.typeTableName}{primaryColumnName}";
            var primaryKeyValues = new List<int>();
            var command = new SqlCommand(selectAllDataForSpecificType, connection);
            command.Transaction = this.transaction;
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

        private void InsertRelatedDataOnly<TItem>(ref TItem? item, SqlConnection connection, int? baseTypeId, Type baseType)
        {
            var type = item.GetType();

            if (type.GetCustomAttribute<TableAttribute>().IsRelatedTable)
            {
                if (type == baseType)
                {
                    type.GetProperties()
                        .FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                        && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)?
                        .SetValue(item, baseTypeId);
                }
                else
                {
                    type.GetProperties()
                        .FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == baseType)?
                        .SetValue(item, baseTypeId);
                }
                try
                {
                    var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                    var sqlCommand = new SqlCommand(sqlInsert, connection);
                    sqlCommand.Transaction = this.transaction;
                    sqlCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    this.logger.Log(ex.Message);
                }
            }

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (childs.Any())
            {
                foreach (var child in childs)
                {
                    if (child.GetValue(item) == null)
                        continue;

                    dynamic? childInstance = child.GetValue(item);
                    if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                    {
                        if (child.PropertyType.GetInterface("IDictionary") != null)
                        {
                            var keys = childInstance?.Keys;
                            foreach (var key in keys)
                            {
                                var keyInstance = key;
                                var valueInstance = childInstance?[key];
                                InsertRelatedDataOnly(ref keyInstance, connection, baseTypeId, baseType);
                                InsertRelatedDataOnly(ref valueInstance, connection, baseTypeId, baseType);
                            }
                        }
                        else
                        {
                            foreach (var element in childInstance)
                            {
                                var elementInstance = element;
                                InsertRelatedDataOnly(ref elementInstance, connection, baseTypeId, baseType);
                            }
                        }
                    }
                    else
                    {
                        InsertRelatedDataOnly(ref childInstance, connection, baseTypeId, baseType);
                    }
                    SetValueIntoProperty(ref item, childInstance, child);
                }
            }
        }

        private void InsertNonRelatedData<TItem>(ref TItem? item, SqlConnection connection)
        {
            var type = item.GetType();
            if (!type.GetCustomAttribute<TableAttribute>().IsRelatedTable)
            {
                var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                var sqlCommand = new SqlCommand(sqlInsert, connection);
                sqlCommand.Transaction = this.transaction;
                int itemId = (int)sqlCommand.ExecuteScalar();

                var primaryKeyProperty = type.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);

                SetValueIntoProperty(ref item, itemId, primaryKeyProperty);
            }
            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (childs.Any())
            {
                foreach (var child in childs)
                {
                    if (child.GetValue(item) == null)
                        continue;
                    dynamic childInstance = child.GetValue(item);
                    if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                    {
                        if (child.PropertyType.GetInterface("IDictionary") != null)
                        {
                            var keys = childInstance.Keys;
                            foreach (var key in keys)
                            {
                                var keyInstance = key;
                                InsertNonRelatedData(ref keyInstance, connection);
                                InsertNonRelatedData(ref childInstance[key], connection);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < childInstance.Count; i++)
                            {
                                InsertNonRelatedData(childInstance[i], connection);
                            }
                        }
                    }
                    else
                    {
                        InsertNonRelatedData(ref childInstance, connection);
                    }
                }
            }
        }

        private void InsertAlgorithm<TItem>(ref TItem? item, SqlConnection connection)
        {
            var type = item.GetType();
            if (!type.GetCustomAttribute<TableAttribute>().IsRelatedTable)
            {
                var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                var sqlCommand = new SqlCommand(sqlInsert, connection);
                sqlCommand.Transaction = this.transaction;

                try
                {
                    int itemId = (int)sqlCommand.ExecuteScalar();
                    var primaryKeyProperty = type.GetProperties().FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
                    SetValueIntoProperty(ref item, itemId, primaryKeyProperty);
                }
                catch (Exception ex)
                {
                    this.logger.Log(ex.Message);
                }
            }
            WorkingWithRelatedEntities(ref item, connection, type);
        }

        private void WorkingWithRelatedEntities<TItem>(ref TItem? item, SqlConnection connection, Type type)
        {
            var realtedEntities = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (realtedEntities.Any())
            {
                foreach (var realtedEntity in realtedEntities)
                {
                    if (realtedEntity.GetValue(item) == null)
                        continue;

                    dynamic relatedEntityInstance = realtedEntity.GetValue(item);
                    if (realtedEntity.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                    {
                        WorkingWithRelatedEntityCollection(connection, realtedEntity, relatedEntityInstance);
                    }
                    else
                    {
                        InsertAlgorithm(ref relatedEntityInstance, connection);
                    }
                    SetValueIntoProperty(ref item, relatedEntityInstance, realtedEntity);
                }
            }
        }

        private void WorkingWithRelatedEntityCollection(SqlConnection connection, PropertyInfo child, dynamic childInstance)
        {
            if (child.PropertyType.GetInterface("IDictionary") != null)
            {
                var keys = childInstance.Keys;
                foreach (var key in keys)
                {
                    var keyInstance = key;
                    var valueInstance = childInstance[key];

                    InsertAlgorithm(ref keyInstance, connection);
                    InsertAlgorithm(ref valueInstance, connection);
                }
            }
            else
            {
                foreach (var element in childInstance)
                {
                    var elementInstance = element;
                    InsertAlgorithm(ref elementInstance, connection);
                }
            }
        }

        private void SetValueIntoProperty<TItem>(ref TItem? item, object value, PropertyInfo property)
        {
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
                return null;
            }
            item = FillProperties(item, type, sqlReader);
            item = FillChilds(item, type, sqlReader);

            return item;
        }

        private object GetItemInstance(Type type, SqlDataReader sqlReader)
        {
            object item;
            if (type.IsAbstract)
            {
                item = GetDerivedClass(sqlReader);
                if (item == null)
                {
                    return item;
                }
            }
            else
                item = Activator.CreateInstance(type);
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
                var relatedEintityType = relatedEintity.GetCustomAttribute<RelatedEntityAttribute>()?.RelatedType;
                var isCollection = relatedEintity.GetCustomAttribute<RelatedEntityAttribute>()?.IsCollection;
                if ((bool)isCollection)
                {
                    relatedEintity.SetValue(item, GetFilledCollection(relatedEintity, sqlReader));
                }
                else
                {
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
                        methodAdd.Invoke(collection, itemsCollection.ToArray());
                        itemsCollection.Clear();
                    }
                } while (sqlReader.Read());
            }
            return collection;
        }

        private object GetDerivedClass(SqlDataReader sqlReader)
        {
            var derivedTypes = this.assembly.GetTypes().Where(assemblyType => assemblyType.GetCustomAttributes<InheritanceRelationAttribute>().Any()
                                                        && !assemblyType.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass);
            var tableName = derivedTypes.FirstOrDefault()?.GetCustomAttribute<TableAttribute>()?.TableName;
            var matchingColumnName = derivedTypes.FirstOrDefault()?.GetCustomAttribute<TypeAttribute>()?.ColumnMatching;
            if (matchingColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                matchingColumnName = $"{tableName}{matchingColumnName}";
            }

            if (sqlReader[matchingColumnName].GetType() == typeof(DBNull))
            {
                return null;
            }

            var type = derivedTypes.FirstOrDefault(derivedType => derivedType.GetCustomAttribute<TypeAttribute>().TypeID == (int)sqlReader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private object FillProperties(object item, Type type, SqlDataReader sqlReader)
        {
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());

            foreach (var property in properties)
            {
                string columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                if (columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    columnName = $"{property.ReflectedType.GetCustomAttribute<TableAttribute>().TableName}{columnName}";
                }

                if (sqlReader[columnName].GetType() != typeof(DBNull))
                    property.SetValue(item, sqlReader[columnName]);
                else
                    property.SetValue(item, null);
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
                    this.transaction.Dispose();
                }
                disposed = true;
            }
        }

        #endregion Methods.Private

        #endregion Methods
    }
}