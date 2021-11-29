using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T>// where T : class
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

        //public void Create(T item)
        //{
        //    var sqlQuery = sqlGenerator.GetInsertIntoSqlQuery(item);  //TODO: почему-то корабли вставляются 7 раз !!! Проблема в INSERT команде. Добавляется новый корабль с каждой ячейкой
        //    sqlQueries.Add(sqlQuery);
        //    //addedItems.Add(item);
        //}

        public void Create<TItem>(TItem item, SqlConnection connection)
        {
            throw new NotImplementedException();
            var type = item.GetType();
            var properties = type.GetProperties();
            var sqlQuery = sqlGenerator.GetInsertConcreteItemSqlQuery(item);  //TODO: почему-то корабли вставляются 7 раз !!! Проблема в INSERT команде. Добавляется новый корабль с каждой ячейкой

            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var itemPrimaryKeyValue = sqlCommand.ExecuteScalar();

            properties.First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).SetValue(item, itemPrimaryKeyValue);

            var childs = properties.Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childs.Count() > 0)
            {
                foreach (var child in childs)
                {
                    if (child.GetValue(item) == null)
                    {
                        continue;
                    }
                    //Dictionary<object, object> childInstance = (Dictionary<object, object>)child.GetValue(item); //получение значения свойства
                    dynamic childInstance = child.GetValue(item); //получение значения свойства
                    if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection) //если свойство - коллекция, то для каждого элемента нужно получить имя колонки и значение
                    {
                        if (childInstance.GetType().GetInterface("IDictionary") != null)
                        {
                            foreach (var key in childInstance.Keys)
                            {
                                Create(key, connection);
                                Create(childInstance[key], connection);
                                //result += GetInsertIntoSqlQuery(key) + Environment.NewLine;
                                //result += GetInsertIntoSqlQuery(childInstance[key]) + Environment.NewLine;
                            }
                        }
                        else
                        {
                            foreach (var element in childInstance)
                            {
                                Create(element, connection);
                                //result += GetInsertIntoSqlQuery(element) + Environment.NewLine;
                            }
                        }
                    }
                    else
                    {
                        Create(childInstance, connection);
                        //result += GetInsertIntoSqlQuery(childInstance) + Environment.NewLine; //TODO: тут креш когда прилетатет NULL
                    }
                }
            }
            sqlQueries.Add(sqlQuery);
            //addedItems.Add(item);
        }

        public void Create(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var sqlQuery = sqlGenerator.GetInsertIntoSqlQuery(item);
                sqlQueries.Add(sqlQuery);
            }
        }

        public T ReadItemById(int id)
        {
            var type = typeof(T);
            var sqlQuery = sqlGenerator.GetSelectJoinString(type, id);
            //var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            bool hasCollectionInside = IsCollectionsInsideType(properties);

            if (hasCollectionInside)
            {
                using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
                {
                    connection.Open();

                    var test = sqlGenerator.GetSelectJoinString(type, id);
                    SqlCommand command = new SqlCommand(sqlGenerator.GetSelectJoinString(type, id), connection);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                return (T)MatchDataItem(type, reader);
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
                        SqlCommand cmd = new SqlCommand(sqlQuery, connection);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                return (T)MatchDataItem(type, reader);
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
            //var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

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

        private bool IsCollectionsInsideType(IEnumerable<PropertyInfo> properties)
        {
            bool hasCollectionInside = false;
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                {
                    hasCollectionInside = true;
                    break;
                }
            }

            return hasCollectionInside;
        }

        private List<int> SelectPrimaryKeyValues(Type type, SqlConnection con)
        {
            string selectPrimaryKeyValues = sqlGenerator.SelectFromSingleTableSqlQuery(type);
            var primaryColumnName = type.GetProperties()
                                                       .First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                                       .GetCustomAttribute<ColumnAttribute>().ColumnName;
            primaryColumnName = $"{this.typeTableName}{primaryColumnName}";
            var primaryKeyValues = new List<int>();
            SqlCommand cmd = new SqlCommand(selectPrimaryKeyValues, con);
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

        public void Update(T item)
        {
            var type = item.GetType();
            var primaryKeyColumn = (int)type.GetProperties().FirstOrDefault(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
            if (primaryKeyColumn > 0)
            {
                var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item);
                sqlQueries.Add(sqlUpdateQuery);
            }
            throw new Exception("Primary key value was not set");

            //TODO: пройтись по всем вложенным сущностям и обновить их тоже.
            //TODO: при изменении размера поля боя должны ли быть добавлены новые ячейки ?
        }

        public void UpdateBy(T item, string columnName, object value)
        {
            var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item, columnName, value);
            sqlQueries.Add(sqlUpdateQuery);
            //TODO: пройтись по всем вложенным сущностям и обновить их тоже.
            //TODO: при изменении размера поля боя должны ли быть добавлены новые ячейки ?
        }

        public void Delete(int id)
        {
            //type -> attribute [Table] -> TableName
            //DELETE [TableName]
            //WHERE [TableName].Id = id
            var sqlQuery = sqlGenerator.GetDeleteSqlQuery(this.typeTableName, id);
            sqlQueries.Add(sqlQuery);
        }

        public void Delete(T item)
        {
            //item -> type -> attribute [Table] -> TableName
            //DELETE [TableName]
            //WHERE [TableName].Id = item.id
            var type = typeof(T);
            var primaryColumnProperty = type.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
            int id = (int)primaryColumnProperty.GetValue(item); // только для элементов, где есть ID
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

        private object MatchDataItem(Type type, SqlDataReader reader)
        {
            var item = GetItemInstance(type, reader);
            if (item == null)
            {
                return null;
            }
            item = FillProperties(item, type, reader);
            item = FillChilds(item, type, reader);

            return item;
        }

        private object GetItemInstance(Type type, SqlDataReader reader)
        {
            object item;
            if (type.IsAbstract)
            {
                item = GetDerivedClass(reader);
                if (item == null)
                {
                    return item;
                }
            }
            else
                item = Activator.CreateInstance(type);
            return item;
        }

        private dynamic FillChilds(object item, Type type, SqlDataReader reader)
        {
            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            if (childs.Count() == 0)
            {
                return item;
            }

            foreach (var child in childs)
            {
                var childType = child.GetCustomAttribute<RelatedEntityAttribute>().RelatedType;
                var isCollection = child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
                if (isCollection)
                {
                    //var test = FilledCollection(child, reader);
                    //var listType = child.PropertyType;
                    //var countTest = listType.GetProperty("Count").GetValue(test);

                    //var itemPrimaryKeyValue = (int)child.ReflectedType.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
                    child.SetValue(item, GetFilledCollection(child, /*itemPrimaryKeyValue, */reader));
                }
                else
                {
                    child.SetValue(item, MatchDataItem(childType, reader));
                }
            }
            return item;
        }

        private object? GetFilledCollection(PropertyInfo child, /*int id,*/ SqlDataReader reader)
        {
            var type = child.PropertyType;
            var collection = Activator.CreateInstance(type);
            var genericTypes = child.PropertyType.GenericTypeArguments;
            var itemsCollection = new List<object>();

            object item;
            if (reader.HasRows)
            {
                do
                {
                    foreach (var generic in genericTypes)
                    {
                        item = MatchDataItem(generic, reader);
                        if (item == null)
                        {
                            continue;
                        }
                        itemsCollection.Add(item);
                    }
                    if (itemsCollection.Count != 0)
                    {
                        var methodAdd = child.PropertyType.GetMethod("Add");
                        methodAdd.Invoke(collection, itemsCollection.ToArray());
                        itemsCollection.Clear();
                    }
                } while (reader.Read());
            }
            return collection;
        }

        private object GetDerivedClass(SqlDataReader reader)
        {
            var derivedTypes = assembly.GetTypes().Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                                                        && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false);
            var tableName = derivedTypes.First().GetCustomAttribute<TableAttribute>().TableName;
            var matchingColumnName = derivedTypes.FirstOrDefault().GetCustomAttribute<TypeAttribute>().ColumnMatching;
            if (matchingColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                matchingColumnName = $"{tableName}{matchingColumnName}";
            }

            if (reader[matchingColumnName].GetType() == typeof(DBNull))
            {
                return null;
            }

            var type = derivedTypes.FirstOrDefault(t => t.GetCustomAttribute<TypeAttribute>().TypeID == (int)reader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private object FillProperties(object item, Type type, SqlDataReader reader)
        {
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);

            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                if (columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    columnName = $"{prop.ReflectedType.GetCustomAttribute<TableAttribute>().TableName}{columnName}";
                }

                if (reader[columnName].GetType() != typeof(DBNull))
                    prop.SetValue(item, reader[columnName]);
                else
                    prop.SetValue(item, null);
            }
            return item;
        }

        #endregion Methods.Private

        #endregion Methods
    }
}