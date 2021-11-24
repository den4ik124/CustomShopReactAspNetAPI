using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T>// where T : class
    {
        //private List<T> addedItems = new List<T>();
        //private List<T> dirtyItems = new List<T>();
        //private List<T> deletedItems = new List<T>();

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

        #region Methods.Private

        /// <summary>
        /// Создает запрос SELECT ... JOIN на основании вложенных типов
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        //private string GetSqlQuery(Type type)
        //{
        //    //var tablePropetriesNames = new Dictionary<string, List<string>>();
        //    var tablePropetriesNames = new Dictionary<Type, List<string>>();

        //    string tableName;
        //    List<string> propertiesNames;
        //    DefineTableAndProperties(type, out tableName, out propertiesNames);

        //    //tablePropetriesNames.Add(tableName, propertiesNames);
        //    tablePropetriesNames.Add(type, propertiesNames);

        //    var childTables = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
        //    if (childTables.Count() == 0)
        //    {
        //        return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName);
        //    }

        //    DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);

        //    StringBuilder sb = new StringBuilder("SELECT\n");
        //    foreach (var table in tablePropetriesNames)
        //    {
        //        string currentTableName = table.Key.GetCustomAttribute<TableAttribute>().TableName;
        //        foreach (var property in tablePropetriesNames[table.Key])
        //        {
        //            if (property.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
        //            {
        //                sb.Append($"[{currentTableName}].[{property}] AS [{currentTableName}{property}],\n");
        //                continue;
        //            }
        //            sb.Append($"[{currentTableName}].[{property}],\n");
        //        }
        //    }
        //    Debug.WriteLine(sb.ToString());
        //    var res = sb.ToString();

        //    sb.Remove(sb.Length - 2, 1);

        //    Debug.WriteLine(sb.ToString());

        //    string relatedTableName = tablePropetriesNames.First(type => type.Key.GetCustomAttribute<TableAttribute>().IsRelatedTable).Key.GetCustomAttribute<TableAttribute>().TableName;
        //    sb.Append($" FROM [{relatedTableName}]\n");

        //    Debug.WriteLine(sb.ToString());

        //    //SELECT
        //    //  [table1].[id] AS [table1Id],
        //    //  [table1].[prop2],
        //    //  [table1].[...],

        //    //  [table2].[Id] AS [table2Id],
        //    //  [table2].[prop2],
        //    //  [table2].[...],

        //    //  [table3].[Id] AS [table3Id],
        //    //  [table3].[prop2],
        //    //  ... . ...
        //    //  [tableN].[Id] AS [tableNId],
        //    //  [tableN].[prop2]
        //    // FROM [table1]
        //    // LEFT JOIN [table2] ON [table2].[Id] = [table1].[FK_table2_Id]
        //    // LEFT JOIN [table3] ON [table3].[Id] = [table1].[FK_table3_Id]
        //    // ...
        //    // LEFT JOIN [tableN] ON [tableN].[Id] = [table1].[FK_tableN_Id]

        //    //var columns =
        //    return sb.ToString();
        //    throw new NotImplementedException();
        //}

        //private void DefineRelatedEntities(ref Dictionary<Type, List<string>> tablePropetriesNames, ref string tableName, ref List<string> propertiesNames, IEnumerable<PropertyInfo> childTables)
        //{
        //    childTables.OrderBy(i => i.PropertyType.GetCustomAttribute<TableAttribute>().IsRelatedTable); //TODO: выяснить нужна ли эта сортировка или можно придумать что получше?
        //    //if (childTables.Count() == 0)
        //    //{
        //    //    return;
        //    //}

        //    //DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);
        //    foreach (var childTable in childTables)
        //    {
        //        //DefineTableAndProperties(childTable.PropertyType, out tableName, out propertiesNames);
        //        var test = childTable.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
        //        if (childTable.PropertyType.IsGenericType)
        //        {
        //            var genericTypes = childTable.PropertyType.GetGenericArguments();
        //            foreach (var genericType in genericTypes)
        //            {
        //                DefineTableAndProperties(genericType, out tableName, out propertiesNames);
        //                tablePropetriesNames.Add(genericType, propertiesNames);
        //            }
        //        }
        //    }
        //}

        ////private string SelectFromSingleTableSqlQuery(Dictionary<string, List<string>> tablePropetriesNames, string tableName)
        //private string SelectFromSingleTableSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string tableName)
        //{
        //    StringBuilder sb = new StringBuilder("SELECT \n");

        //    var type = tablePropetriesNames.First(t => t.Key.GetCustomAttribute<TableAttribute>().TableName == tableName).Key;

        //    var properties = tablePropetriesNames[type];// type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
        //    foreach (var prop in properties)
        //    {
        //        if (prop.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        //        {
        //            string propAs = $"{prop}] AS [{tableName}{prop}";
        //            sb.Append($"[{tableName}].[{propAs}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
        //        }
        //        else
        //        {
        //            sb.Append($"[{tableName}].[{prop}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
        //        }
        //    }
        //    sb.Remove(sb.Length - 1, 1);
        //    sb.Append($" FROM [{tableName}];");
        //    Debug.WriteLine(sb.ToString());

        //    return sb.ToString();
        //}

        //private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        //{
        //    tableName = "";
        //    propertiesNames = new List<string>();

        //    var test = type.GetCustomAttribute<RelatedEntityAttribute>();
        //    if (type.GetCustomAttribute<RelatedEntityAttribute>() != null && type.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
        //    {
        //        if (type.IsGenericType)
        //        {
        //            var genericTypes = type.GetGenericArguments();

        //            foreach (var genericType in genericTypes)
        //            {
        //                GetSinglePropertyData(genericType, out tableName, out propertiesNames);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        GetSinglePropertyData(type, out tableName, out propertiesNames);
        //    }
        //}

        private void GetSinglePropertyData(Type type, out string tableName, out List<string> propertiesNames)
        {
            var attribute = type.GetCustomAttribute<TableAttribute>();
            tableName = attribute.TableName;
            if (attribute.IsRelatedTable)
            {
                tableName += $".rel";
            }
            propertiesNames = type.GetProperties()
                                    .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                    .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName).ToList();
            if (type.IsAbstract)
            {
                var columnMatching = assembly.GetTypes()
                                            .Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                                            && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false)
                                            .Select(a => a.GetCustomAttribute<InheritanceRelationAttribute>().ColumnMatching)
                                            .First();
                //string propertyName = $"{columnMatching} AS  {tableName}{columnMatching}";
                string propertyName = $"{columnMatching}";
                propertiesNames.Add(propertyName);
            }
        }

        private object MatchDataItem(Type type, SqlDataReader reader)
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
            var columns = type.GetProperties()
                                .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName);
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            FillProperties(reader, ref item, properties);

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            if (childs.Count() == 0)
            {
                return item;
            }

            FillChilds(reader, ref item, childs);

            return item;
        }

        private void FillChilds(SqlDataReader reader, ref object item, IEnumerable<PropertyInfo> childs)
        {
            foreach (var child in childs)
            {
                var childType = child.GetCustomAttribute<RelatedEntityAttribute>().RelatedType;
                var isCollection = child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
                if (isCollection)
                {
                    //var test = FilledCollection(child, reader);
                    //var listType = child.PropertyType;
                    //var countTest = listType.GetProperty("Count").GetValue(test);

                    child.SetValue(item, FilledCollection(child, reader));
                }
                else
                {
                    child.SetValue(item, MatchDataItem(childType, reader));
                }
            }
        }

        private object? FilledCollection(PropertyInfo child, SqlDataReader reader)
        {
            var type = child.PropertyType;
            var collection = Activator.CreateInstance(type);
            var genericType = child.PropertyType.GenericTypeArguments;
            foreach (var generic in genericType)
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = MatchDataItem(generic, reader);
                        //var methodContains = child.PropertyType.GetMethod("Contains");
                        //if (item == null)// || (bool)methodContains.Invoke(collection, new object[] { item }))

                        if (item == null)
                        {
                            continue;
                        }
                        var methodAdd = child.PropertyType.GetMethod("Add");
                        methodAdd.Invoke(collection, new object[] { item });
                    }
                }
            }
            return collection;
        }

        private object GetDerivedClass(SqlDataReader reader)
        {
            Type type = typeof(object);
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

            type = derivedTypes.FirstOrDefault(t => t.GetCustomAttribute<TypeAttribute>().TypeID == (int)reader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private void FillProperties(SqlDataReader reader, ref object? item, IEnumerable<PropertyInfo> properties)
        {
            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                if (columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    columnName = $"{prop.ReflectedType.GetCustomAttribute<TableAttribute>().TableName}Id";
                }

                if (reader[columnName].GetType() != typeof(DBNull))
                    prop.SetValue(item, reader[columnName]);
                else
                    prop.SetValue(item, null);
            }
        }

        private void AddedItemsSubmition(SqlConnection connection)
        {
            //foreach (var item in addedItems)
            //{
            //}
        }

        private void DirtyItemsSubmition(SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private void DeletedItemsSubmition(SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        #endregion Methods.Private

        #region Methods.Public

        public void Create(T item)
        {
            //addedItems.Add(item);
        }

        public void Create(IEnumerable<T> items)
        {
            //addedItems.AddRange(items);
        }

        public T ReadItemById(int id)
        {
            var type = typeof(T);
            var sqlQuery = sqlGenerator.GetSelectJoinString(type, id);

            using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        return (T)MatchDataItem(type, reader);
                    }
                }
                return default(T);
            }
        }

        public IEnumerable<T> ReadItems()
        {
            var result = new List<T>();
            var type = typeof(T);
            //var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            var sqlQuery = sqlGenerator.GetSelectJoinString(type);

            #region OLD SqlQueries

            //string sqlQuery = GetSqlQuery(type);

            //sqlQuery = $"SELECT * FROM {tableName}"; //TODO: генерировать SQL запрос здесь

            //sqlQuery = $@"  SELECT
            //             Cells.Id AS CellsId,
            //             Cells.ShipID,
            //             Cells.PointID,
            //             Cells.BattleFieldID,
            //             Ships.Id AS ShipsId,
            //             Ships.TypeId,
            //             Ships.[Range],
            //             Ships.Velocity,
            //             Ships.Size,
            //             Points.Id AS PointsId,
            //             Points.X,
            //             Points.Y
            //              FROM Cells
            //              LEFT JOIN BattleFields ON Cells.BattleFieldID = BattleFields.Id
            //              LEFT JOIN Ships ON Cells.ShipID = Ships.Id
            //              LEFT JOIN Points ON Cells.PointID = Points.Id
            //              WHERE BattleFields.Id = 1;";

            //sqlQuery = $@"  SELECT
            //                    BattleFields.Id AS BattleFieldsId,
            //                    BattleFields.SideLength,
            //                 Cells.Id AS CellsId,
            //                 Cells.ShipID,
            //                 Cells.PointID,
            //                 Cells.BattleFieldID,
            //                 Ships.Id AS ShipsId,
            //                 Ships.TypeId,
            //                 Ships.[Range],
            //                 Ships.Velocity,
            //                 Ships.Size,
            //                 Points.Id AS PointsId,
            //                 Points.X,
            //                 Points.Y
            //                  FROM Cells
            //                  LEFT JOIN BattleFields ON Cells.BattleFieldID = BattleFields.Id
            //                  LEFT JOIN Ships ON Cells.ShipID = Ships.Id
            //                  LEFT JOIN Points ON Cells.PointID = Points.Id;";

            #endregion OLD SqlQueries

            using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sqlQuery, connection); //вылетает исключение, т.к. SQL запрос еще не доделан
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader);
                        var resultItem = MatchDataItem(type, reader);
                        if (resultItem != null)
                        {
                            result.Add((T)resultItem); //TODO: проверить почему всего одно поле боя создается
                        }
                    }
                }
            }
            return result;
        }

        public void Update(T item)
        {
            var sqlUpdateQuery = sqlGenerator.GetUpdateSqlQuery(item);
            sqlQueries.Add(sqlUpdateQuery);
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

            var sqlQuery = sqlGenerator.GetDeleteSqlQuery(item);
            sqlQueries.Add(sqlQuery);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Submit(SqlConnection connection)
        {
            foreach (var command in sqlQueries)
            {
                SqlCommand sqlCommand = new SqlCommand(command, connection);
                sqlCommand.ExecuteNonQuery();
            }
            //AddedItemsSubmition(connection);
            //DirtyItemsSubmition(connection);
            //DeletedItemsSubmition(connection);
        }

        #endregion Methods.Public

        #endregion Methods
    }
}