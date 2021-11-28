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
            //uow.Register(this);

            sqlGenerator = new SqlGenerator();
            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
            typeTableName = typeof(T).GetCustomAttribute<TableAttribute>().TableName;
        }

        #region Methods

        #region Methods.Public

        public void Create(T item)
        {
            var sqlQuery = sqlGenerator.GetInsertIntoSqlQuery(item);
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
            return default(T);
        }

        public IEnumerable<T> ReadItems()
        {
            var result = new List<T>();
            var type = typeof(T);
            //var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

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
            return result;
        }

        //public IEnumerable<T> ReadItems()
        //{
        //    var result = new List<T>();
        //    var type = typeof(T);
        //    //var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

        //    var sqlQuery = sqlGenerator.GetSelectJoinString(type);
        //    Dictionary<int, DataTable> dataTables;
        //    using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
        //    {
        //        connection.Open();
        //        List<int> primaryKeyValues = SelectPrimaryKeyValues(type, connection);

        //        dataTables = GetDataTables(type, connection, primaryKeyValues);
        //    }

        //    foreach (var dataTable in dataTables)
        //    {
        //        var test = dataTable.Value.Rows;
        //        result.Add(GetItemFromDataTable(dataTable.Value.Rows, type));
        //    }

        //    return null;
        //    //using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
        //    //{
        //    //    connection.Open();
        //    //    SqlCommand cmd = new SqlCommand(sqlQuery, connection);
        //    //    SqlDataReader reader = cmd.ExecuteReader();
        //    //    if (reader.HasRows)
        //    //    {
        //    //        while (reader.Read())
        //    //        {
        //    //            var resultItem = MatchDataItem(type, reader);
        //    //            if (resultItem != null)
        //    //            {
        //    //                result.Add((T)resultItem);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //return result;
        //}

        //private T GetItemFromDataTable(DataRowCollection dataRows, Type parentType)
        //{
        //    var item = GetItemInstance(parentType, dataRows[0]); //TODO: подумать какую строку сюда нужно передавать
        //    if (item == null)
        //    {
        //        return default(T);
        //    }
        //    item = FillProperties(item, parentType, dataRows[0]);
        //    item = FillChilds(item, parentType, dataRows);

        //    return item;
        //}

        private Dictionary<int, DataTable> GetDataTables(Type type, SqlConnection con, List<int> primaryKeyValues)
        {
            var dataTables = new Dictionary<int, DataTable>();

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();

            foreach (var primaryKey in primaryKeyValues)
            {
                var sqlSelectQuery = sqlGenerator.GetSelectJoinString(type, primaryKey);
                adapter.SelectCommand = new SqlCommand(sqlSelectQuery, con);
                var dt = new DataTable();
                adapter.Fill(dt);
                dataTables.Add(primaryKey, dt);
            }
            return dataTables;
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
            var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item);
            sqlQueries.Add(sqlUpdateQuery);
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

        //private void GetSinglePropertyData(Type type, out string tableName, out List<string> propertiesNames)
        //{
        //    var attribute = type.GetCustomAttribute<TableAttribute>();
        //    tableName = attribute.TableName;
        //    if (attribute.IsRelatedTable)
        //    {
        //        tableName += $".rel";
        //    }
        //    propertiesNames = type.GetProperties()
        //                            .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
        //                            .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName).ToList();
        //    if (type.IsAbstract)
        //    {
        //        var columnMatching = assembly.GetTypes()
        //                                    .Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
        //                                    && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false)
        //                                    .Select(a => a.GetCustomAttribute<InheritanceRelationAttribute>().ColumnMatching)
        //                                    .First();
        //        //string propertyName = $"{columnMatching} AS  {tableName}{columnMatching}";
        //        string propertyName = $"{columnMatching}";
        //        propertiesNames.Add(propertyName);
        //    }
        //}

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

        //private void FillChilds(SqlDataReader reader, ref object item, IEnumerable<PropertyInfo> childs)
        //{
        //    foreach (var child in childs)
        //    {
        //        var childType = child.GetCustomAttribute<RelatedEntityAttribute>().RelatedType;
        //        var isCollection = child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
        //        if (isCollection)
        //        {
        //            //var test = FilledCollection(child, reader);
        //            //var listType = child.PropertyType;
        //            //var countTest = listType.GetProperty("Count").GetValue(test);
        //            var itemId = (int)child.ReflectedType.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
        //            child.SetValue(item, GetFilledCollection(child, itemId, reader));
        //        }
        //        else
        //        {
        //            child.SetValue(item, MatchDataItem(childType, reader));
        //        }
        //    }
        //}

        //private object FillChilds(SqlDataReader reader, object item, IEnumerable<PropertyInfo> childs)
        //{
        //    foreach (var child in childs)
        //    {
        //        var childType = child.GetCustomAttribute<RelatedEntityAttribute>().RelatedType;
        //        var isCollection = child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
        //        if (isCollection)
        //        {
        //            //var test = FilledCollection(child, reader);
        //            //var listType = child.PropertyType;
        //            //var countTest = listType.GetProperty("Count").GetValue(test);
        //            var itemId = (int)child.ReflectedType.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
        //            child.SetValue(item, GetFilledCollection(child, itemId, reader));
        //        }
        //        else
        //        {
        //            child.SetValue(item, MatchDataItem(childType, reader));
        //        }
        //    }
        //    return item;
        //}
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
                    var itemPrimaryKeyValue = (int)child.ReflectedType.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
                    child.SetValue(item, GetFilledCollection(child, itemPrimaryKeyValue, reader));
                }
                else
                {
                    child.SetValue(item, MatchDataItem(childType, reader));
                }
            }
            return item;
        }

        private object? GetFilledCollection(PropertyInfo child, int id, SqlDataReader reader)
        {
            var baseType = child.ReflectedType;
            var baseTypePrimaryColumnName = $"{baseType.GetCustomAttribute<TableAttribute>().TableName}" +
                                            $"{baseType.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).Name}";

            //var baseTypeId = baseType.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
            var baseTypeID = id;
            var type = child.PropertyType;
            var collection = Activator.CreateInstance(type);
            var genericTypes = child.PropertyType.GenericTypeArguments;
            var itemsCollection = new List<object>();

            object item;
            if (reader.HasRows)
            {
                do
                {
                    if ((int)reader[baseTypePrimaryColumnName] != baseTypeID)
                    {
                        //reader.Close();
                        break;
                    }
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
                } while (reader.Read()); //TODO: сжирает строку следующего BF. Подумать как исправить (Только при ReadItems)
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

        //private void FillProperties(SqlDataReader reader, ref object? item, IEnumerable<PropertyInfo> properties)
        //{
        //    foreach (var prop in properties)
        //    {
        //        string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
        //        if (columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        //        {
        //            columnName = $"{prop.ReflectedType.GetCustomAttribute<TableAttribute>().TableName}Id";
        //        }

        //        if (reader[columnName].GetType() != typeof(DBNull))
        //            prop.SetValue(item, reader[columnName]);
        //        else
        //            prop.SetValue(item, null);
        //    }
        //}
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