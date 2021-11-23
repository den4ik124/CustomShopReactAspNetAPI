using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

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

            var sqlQuery = $"SELECT * FROM {tableName}"; //TODO: генерировать SQL запрос здесь
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

        public IEnumerable<T?> ReadItems<T>()
        {
            var result = new List<T?>();
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            //string sqlQuery = GetSqlQuery(type);

            var sqlQuery = $"SELECT * FROM {tableName}"; //TODO: генерировать SQL запрос здесь

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
                        result.Add((T)MatchDataItem(type, reader));
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
            //List<string> tables = new List<string>();
            //tables.Add(type.GetCustomAttribute<TableAttribute>().TableName);
            //tables.AddRange(type.GetProperties().Select(prop => prop.GetCustomAttribute<ChildAttribute>().Table).ToList());

            //var columns =
            throw new NotImplementedException();
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

            foreach (var child in childs)
            {
                var childType = child.GetCustomAttribute<ChildAttribute>().RelatedType;
                var isCollection = child.GetCustomAttribute<ChildAttribute>().IsCollection;
                if (isCollection)
                {
                    var test = FilledCollection(child, reader);
                    child.SetValue(item, FilledCollection(child, reader));
                }
                else
                {
                    child.SetValue(item, MatchDataItem(childType, reader));
                }
            }

            return item;
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
                        var methodContains = child.PropertyType.GetMethod("Contains");

                        if (item == null || (bool)methodContains.Invoke(collection, new object[] { item }))
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