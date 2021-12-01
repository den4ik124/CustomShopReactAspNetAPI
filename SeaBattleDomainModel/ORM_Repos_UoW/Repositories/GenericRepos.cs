using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T>
    {
        private List<string> sqlQueries = new List<string>();

        private readonly string typeTableName;
        private Assembly assembly;
        private IUnitOfWork unitOfWork;
        private SqlGenerator sqlGenerator;

        public GenericRepos(IUnitOfWork uow)
        {
            unitOfWork = uow;
            sqlGenerator = new SqlGenerator();
            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
            typeTableName = typeof(T).GetCustomAttribute<TableAttribute>().TableName;
        }

        #region Methods

        #region Methods.Public

        public void Create<TItem>(ref TItem item, SqlConnection connection)
        {
            InsertAlgorithm(ref item, connection);

            var baseType = item.GetType();
            int itemPrimaryKeyValue = (int)item.GetType()
                                                .GetProperties()
                                                .First(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                                .GetValue(item);

            InsertRelatedDataOnly(ref item, connection, itemPrimaryKeyValue, baseType);
        }

        public void Create(IEnumerable<T> items, SqlConnection connection)
        {
            foreach (var item in items)
            {
                var refitem = item;
                Create(ref refitem, connection);
                //var sqlQuery = sqlGenerator.GetInsertIntoSqlQuery(item);
                //sqlQueries.Add(sqlQuery);
            }
        }

        public T ReadItemById(int id)
        {
            var type = typeof(T);
            var sqlQuery = sqlGenerator.GetSelectJoinString(type, id);
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            bool hasCollectionInside = IsCollectionsInsideType(properties);
            if (hasCollectionInside)
            {
                using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlGenerator.GetSelectJoinString(type, id), connection);
                    using (SqlDataReader sqlReader = command.ExecuteReader())
                    {
                        if (sqlReader.HasRows)
                        {
                            while (sqlReader.Read())
                            {
                                return (T)MatchDataItem(type, sqlReader);
                            }
                        }
                    }
                }
            }
            else
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        SqlDataReader sqlReader = command.ExecuteReader();
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
                    Console.WriteLine(ex.Message);
                }
            }

            return default(T);
        }

        public IEnumerable<T> ReadItems()
        {
            var result = new List<T>();
            var type = typeof(T);
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            bool hasCollectionInside = IsCollectionsInsideType(properties);
            if (hasCollectionInside)
            {
                using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
                {
                    connection.Open();
                    var primaryKeys = SelectPrimaryKeyValues(type, connection);

                    foreach (var primaryKey in primaryKeys)
                    {
                        var testSql = sqlGenerator.GetSelectJoinString(type, primaryKey);
                        SqlCommand command = new SqlCommand(sqlGenerator.GetSelectJoinString(type, primaryKey), connection);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var resultItem = MatchDataItem(type, reader);
                                    if (resultItem != null)
                                    {
                                        result.Add((T)resultItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
                {
                    connection.Open();
                    var testSql = sqlGenerator.GetSelectJoinString(type);
                    SqlCommand command = new SqlCommand(sqlGenerator.GetSelectJoinString(type), connection);
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var resultItem = MatchDataItem(type, reader);
                            if (resultItem != null)
                            {
                                result.Add((T)resultItem);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void Update(T item)
        {
            var type = item.GetType();
            var primaryKeyColumn = (int)type.GetProperties().FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
            if (primaryKeyColumn > 0)
            {
                var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item);
                sqlQueries.Add(sqlUpdateQuery);
            }
            else
            {
                throw new Exception("Primary key value was not set");
            }
        }

        public void UpdateBy(T item, string columnName, object value)
        {
            var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item, columnName, value);
            sqlQueries.Add(sqlUpdateQuery);
        }

        public void DeleteById(int id)
        {
            var sqlQuery = sqlGenerator.GetDeleteSqlQuery(this.typeTableName, id);
            sqlQueries.Add(sqlQuery);
        }

        public void Delete(T item)
        {
            var type = typeof(T);
            var primaryColumnProperty = type.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
            int id = (int)primaryColumnProperty.GetValue(item); // fot items with ID>0 only
            if (id > 0)
            {
                var sqlQuery = sqlGenerator.GetDeleteSqlQuery(item);
                sqlQueries.Add(sqlQuery);
            }
        }

        public void Delete(string columnName, dynamic value)
        {
            var sqlQuery = sqlGenerator.GetDeleteSqlQuery(columnName, value);
            sqlQueries.Add(sqlQuery);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Submit(SqlConnection connection, SqlTransaction transaction)
        {
            foreach (var command in sqlQueries)
            {
                SqlCommand sqlCommand = new SqlCommand(command, connection);
                sqlCommand.Transaction = transaction;
                sqlCommand.ExecuteNonQuery();
            }
        }

        #endregion Methods.Public

        #region Methods.Private

        private bool IsCollectionsInsideType(IEnumerable<PropertyInfo> properties)
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
            string selectAllDataForSpecificType = sqlGenerator.SelectFromSingleTableSqlQuery(type); //select all rows from a specific table to get the all IDs
            var primaryColumnName = type.GetProperties()
                                                       .First(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                                       .GetCustomAttribute<ColumnAttribute>().ColumnName;
            primaryColumnName = $"{this.typeTableName}{primaryColumnName}";
            var primaryKeyValues = new List<int>();
            SqlCommand cmd = new SqlCommand(selectAllDataForSpecificType, connection);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        primaryKeyValues.Add((int)reader[primaryColumnName]);
                    }
                }
            }
            return primaryKeyValues;
        }

        private void InsertRelatedDataOnly<TItem>(ref TItem? item, SqlConnection connection, int baseTypeId, Type baseType)
        {
            var type = item.GetType();

            if (type.GetCustomAttribute<TableAttribute>().IsRelatedTable)
            {
                if (type == baseType)
                {
                    type.GetProperties()
                        .Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                        .First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                        .SetValue(item, baseTypeId);
                }
                else
                {
                    type.GetProperties()
                        .Where(p => p.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                        .First(p => p.GetCustomAttribute<ColumnAttribute>().BaseType == baseType)
                        .SetValue(item, baseTypeId);
                }
                var sqlInsert = sqlGenerator.GetInsertConcreteItemSqlQuery(item);
                var sqlCommand = new SqlCommand(sqlInsert, connection);

                //get itemID from DB
                int itemId = (int)sqlCommand.ExecuteScalar();
            }

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childs.Count() > 0)
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
                            //If collection  IS DICTIONARY
                            var keys = childInstance.Keys;
                            foreach (var key in keys)
                            {
                                var keyInstance = key;
                                var valueInstance = childInstance[key];
                                InsertRelatedDataOnly(ref keyInstance, connection, baseTypeId, baseType);
                                InsertRelatedDataOnly(ref valueInstance, connection, baseTypeId, baseType);
                            }
                        }
                        else
                        {
                            //If collection IS NOT DICTIONARY
                            foreach (var element in childInstance)
                            {
                                var elementInstance = element;
                                InsertRelatedDataOnly(ref elementInstance, connection, baseTypeId, baseType);
                            }
                        }
                    }
                    else
                    {
                        //IF NOT A COLLECTION
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

                //get itemID from DB
                int itemId = (int)sqlCommand.ExecuteScalar();

                //set item ID to ENTITY
                var primaryKeyProperty = type.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
                if (type.IsValueType)
                {
                    object boxedItem = item;
                    primaryKeyProperty.SetValue(boxedItem, itemId);
                    item = (TItem)boxedItem;
                }
                else
                {
                    primaryKeyProperty.SetValue(item, itemId);
                }
            }
            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childs.Count() > 0)
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
                            //If collection  IS DICTIONARY
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
                            //If collection IS NOT DICTIONARY
                            for (int i = 0; i < childInstance.Count; i++)
                            {
                                InsertNonRelatedData(childInstance[i], connection);
                            }
                        }
                    }
                    else
                    {
                        //IF NOT A COLLECTION
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

                //get itemID from DB
                int itemId = (int)sqlCommand.ExecuteScalar();

                //set item ID to ENTITY
                var primaryKeyProperty = type.GetProperties().First(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
                SetValueIntoProperty(ref item, itemId, primaryKeyProperty);
            }
            WorkingWithRelatedEntities(ref item, connection, type);
        }

        private void WorkingWithRelatedEntities<TItem>(ref TItem? item, SqlConnection connection, Type type)
        {
            var realtedEntities = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (realtedEntities.Count() > 0)
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
                        //ЕСЛИ НЕ КОЛЕКЦИЯ
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
                //If collection IS DICTIONARY
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
                //If collection IS NOT DICTIONARY
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
            var relatedEntities = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            if (relatedEntities.Count() == 0)
            {
                return item;
            }

            foreach (var relatedEintity in relatedEntities)
            {
                var relatedEintityType = relatedEintity.GetCustomAttribute<RelatedEntityAttribute>()?.RelatedType;
                var isCollection = relatedEintity.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
                if (isCollection)
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
            var derivedTypes = assembly.GetTypes().Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                                                        && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false);
            var tableName = derivedTypes.First().GetCustomAttribute<TableAttribute>().TableName;
            var matchingColumnName = derivedTypes.FirstOrDefault().GetCustomAttribute<TypeAttribute>().ColumnMatching;
            if (matchingColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                matchingColumnName = $"{tableName}{matchingColumnName}";
            }

            if (sqlReader[matchingColumnName].GetType() == typeof(DBNull))
            {
                return null;
            }

            var type = derivedTypes.FirstOrDefault(derivaedType => derivaedType.GetCustomAttribute<TypeAttribute>().TypeID == (int)sqlReader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private object FillProperties(object item, Type type, SqlDataReader sqlReader)
        {
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0);

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

        #endregion Methods.Private

        #endregion Methods
    }
}