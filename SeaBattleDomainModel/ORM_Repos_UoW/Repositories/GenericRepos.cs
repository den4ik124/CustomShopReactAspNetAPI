using ORM_Repos_UoW.Attributes;
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
        private List<T> addedItems = new List<T>();
        private List<T> dirtyItems = new List<T>();
        private List<T> deletedItems = new List<T>();
        private Assembly assembly;

        private IUnitOfWork unitOfWork;

        public GenericRepos(IUnitOfWork uow)
        {
            unitOfWork = uow;
            uow.Register(this);

            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
        }

        public void Create(T item)
        {
            addedItems.Add(item);
        }

        public void Create(IEnumerable<T> items)
        {
            addedItems.AddRange(items);
        }

        public T ReadItemById(int id)
        {
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            //string sqlQuery = GetSqlQuery(type);

            var sqlQuery = $"SELECT * FROM {tableName} WHERE {tableName}Id = {id}"; //TODO: генерировать SQL запрос здесь
            using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return (T)MatchDataItem(type, reader);
                }
                else
                {
                    return default(T);
                }
            }
        }

        public IEnumerable<T> ReadItems()
        {
            var result = new List<T?>();
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            string sqlQuery = GetSqlQuery(type);

            sqlQuery = $"SELECT * FROM {tableName}"; //TODO: генерировать SQL запрос здесь

            sqlQuery = $@"  SELECT
	                        Cells.Id AS CellsId,
	                        Cells.ShipID,
	                        Cells.PointID,
	                        Cells.BattleFieldID,
	                        Ships.Id AS ShipsId,
	                        Ships.TypeId,
	                        Ships.[Range],
	                        Ships.Velocity,
	                        Ships.Size,
	                        Points.Id AS PointsId,
	                        Points.X,
	                        Points.Y
                          FROM Cells
                          LEFT JOIN BattleFields ON Cells.BattleFieldID = BattleFields.Id
                          LEFT JOIN Ships ON Cells.ShipID = Ships.Id
                          LEFT JOIN Points ON Cells.PointID = Points.Id
                          WHERE BattleFields.Id = 1;";

            sqlQuery = $@"  SELECT
                                BattleFields.Id AS BattleFieldsId,
                                BattleFields.SideLength,
	                            Cells.Id AS CellsId,
	                            Cells.ShipID,
	                            Cells.PointID,
	                            Cells.BattleFieldID,
	                            Ships.Id AS ShipsId,
	                            Ships.TypeId,
	                            Ships.[Range],
	                            Ships.Velocity,
	                            Ships.Size,
	                            Points.Id AS PointsId,
	                            Points.X,
	                            Points.Y
                              FROM Cells
                              LEFT JOIN BattleFields ON Cells.BattleFieldID = BattleFields.Id
                              LEFT JOIN Ships ON Cells.ShipID = Ships.Id
                              LEFT JOIN Points ON Cells.PointID = Points.Id;";
            using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var test = MatchDataItem(type, reader);
                        if (test != null)
                        {
                            result.Add((T)test); //TODO: проверить почему всего одно поле боя создается
                        }
                        //result.Add((T)MatchDataItem(type, reader));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Создает запрос SELECT ... JOIN на основании вложенных типов
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetSqlQuery(Type type)
        {
            var tablePropetriesNames = new Dictionary<string, List<string>>();

            string tableName;
            List<string> propertiesNames;
            DefineTableAndProperties(type, out tableName, out propertiesNames);

            tablePropetriesNames.Add(tableName, propertiesNames);

            var childTables = type.GetProperties().Where(prop => prop.GetCustomAttributes<ChildAttribute>().Count() > 0);
            if (childTables.Count() == 0)
            {
                return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName);
            }

            childTables.OrderBy(i => i.PropertyType.GetCustomAttribute<TableAttribute>().IsRelatedTable); //TODO: выяснить нужна ли эта сортировка или можно придумать что получше?

            foreach (var childTable in childTables)
            {
                DefineTableAndProperties(childTable.PropertyType, out tableName, out propertiesNames);
                tablePropetriesNames.Add(tableName, propertiesNames);
            }

            StringBuilder sb = new StringBuilder("SELECT\n");
            foreach (var table in tablePropetriesNames.Keys)
            {
                foreach (var property in tablePropetriesNames[table])
                {
                    if (property.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append($"[{table}].[{property}] AS [{table}{property}],\n");
                        continue;
                    }
                    sb.Append($"[{table}].[{property}],\n");
                }
            }
            Debug.WriteLine(sb.ToString());
            var res = sb.ToString();

            sb.Remove(sb.Length - 2, 1);

            Debug.WriteLine(sb.ToString());

            sb.Append($" FROM {tablePropetriesNames.First().Key}\n");

            Debug.WriteLine(sb.ToString());
            //SELECT
            //  [table1].[id] AS [table1Id],
            //  [table1].[prop2],
            //  [table1].[...],

            //  [table2].[Id] AS [table2Id],
            //  [table2].[prop2],
            //  [table2].[...],

            //  [table3].[Id] AS [table3Id],
            //  [table3].[prop2],
            //  ... . ...
            //  [tableN].[Id] AS [tableNId],
            //  [tableN].[prop2]
            // FROM [table1]
            // LEFT JOIN [table2] ON [table2].[Id] = [table1].[FK_table2_Id]
            // LEFT JOIN [table3] ON [table3].[Id] = [table1].[FK_table3_Id]
            // ...
            // LEFT JOIN [tableN] ON [tableN].[Id] = [table1].[FK_tableN_Id]

            //var columns =
            throw new NotImplementedException();
        }

        private string SelectFromSingleTableSqlQuery(Dictionary<string, List<string>> tablePropetriesNames, string tableName)
        {
            StringBuilder sb = new StringBuilder("SELECT \n");

            var properties = tablePropetriesNames[tableName];// type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                if (prop.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{prop}] AS [{tableName}{prop}";
                    sb.Append($"[{tableName}].[{propAs}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
                else
                {
                    sb.Append($"[{tableName}].[{prop}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append($" FROM [{tableName}];");
            Debug.WriteLine(sb.ToString());

            return sb.ToString();
        }

        private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = "";
            propertiesNames = new List<string>();

            //TODO: подумать, как работать с типом List здесь!!! Вылетает исключение 

            var test = type.GetCustomAttribute<ChildAttribute>();
            if (type.GetCustomAttribute<ChildAttribute>() != null && type.GetCustomAttribute<ChildAttribute>().IsCollection)
            {
                if (type.IsGenericType)
                {
                    var genericTypes = type.GetGenericArguments();

                    foreach (var genericType in genericTypes)
                    {
                        GetSinglePropertyData(genericType, out tableName, out propertiesNames);
                    }
                }
            }
            else
            {
                GetSinglePropertyData(type, out tableName, out propertiesNames);
            }
        }

        private void GetSinglePropertyData(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = type.GetCustomAttribute<TableAttribute>().TableName;
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
                item = Activator.CreateInstance(type); //TODO: проверка для abstract Ship
            var columns = type.GetProperties()
                                .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName);
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            FillProperties(reader, ref item, properties);

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<ChildAttribute>().Count() > 0);

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
                var childType = child.GetCustomAttribute<ChildAttribute>().RelatedType;
                var isCollection = child.GetCustomAttribute<ChildAttribute>().IsCollection;
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
            var derivedTypes = assembly.GetTypes().Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                                                        && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false);

            var matchingColumnName = derivedTypes.FirstOrDefault().GetCustomAttribute<ShipTypeAttribute>().ColumnMatching;
            if (reader[matchingColumnName].GetType() == typeof(DBNull))
            {
                return null;
            }
            var type = derivedTypes.FirstOrDefault(t => t.GetCustomAttribute<ShipTypeAttribute>().ShipTypeID == (int)reader[matchingColumnName]);
            return Activator.CreateInstance(type);
        }

        private void FillProperties(SqlDataReader reader, ref object? item, IEnumerable<PropertyInfo> properties)
        {
            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                if (columnName == "Id")
                {
                    columnName = $"{prop.ReflectedType.GetCustomAttribute<TableAttribute>().TableName}Id";
                }
                if (reader[columnName].GetType() != typeof(DBNull))
                    prop.SetValue(item, reader[columnName]);
                else
                    prop.SetValue(item, null);
            }
        }

        public void Update(T item)
        {
            dirtyItems.Add(item);
        }

        public void Delete(int id)
        {
            T item = default(T); //продумать алгоритм поиска сущности по Id
            deletedItems.Add(item);
        }

        public void Delete(T item)
        {
            deletedItems.Add(item);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Submit(SqlConnection connection)
        {
            AddedItemsSubmition(connection);
            DirtyItemsSubmition(connection);
            DeletedItemsSubmition(connection);
        }

        private void AddedItemsSubmition(SqlConnection connection)
        {
            foreach (var item in addedItems)
            {
            }
        }

        private void DirtyItemsSubmition(SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private void DeletedItemsSubmition(SqlConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}