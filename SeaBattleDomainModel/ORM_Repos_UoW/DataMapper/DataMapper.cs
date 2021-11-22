using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW.DataMapper
{
    public class DataMapper<T> : IDataMapper<T> where T : class
    {
        private Type currentType;
        private Attribute attribute;
        private string? tableName;
        private IEnumerable<PropertyInfo>? properties;
        private IEnumerable<PropertyInfo>? propertiesChilds;
        private DbContext dbContext;

        //private List<MappedItem<T>> mappedItems;
        public List<MappedItem<T>> mappedItems { get; set; }

        public int ItemsCount { get => mappedItems.Count; }

        public DataMapper(DbContext dbContext)
        {
            this.currentType = typeof(T);
            this.attribute = currentType.GetCustomAttribute<TableAttribute>();
            this.tableName = ((TableAttribute)attribute).TableName;
            this.properties = currentType.GetProperties()
                                         .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            this.propertiesChilds = currentType.GetProperties()
                                         .Where(prop => prop.GetCustomAttributes<ChildAttribute>().Count() > 0);
            this.dbContext = dbContext;
            this.mappedItems = new List<MappedItem<T>>() { };
        }

        #region Methods

        #region Methods.Private

        private void TransferItemsIntoDbTable()
        {
            DataTable dt = dbContext.GetTable(tableName);
            AddDataIntoTable(dt);
            DeleteDataFromTable(dt);
            mappedItems.Clear();
        }

        private void DeleteDataFromTable(DataTable dt)
        {
            var itemsID = mappedItems.Where(item => item.State == State.Deleted)
                                    .Select(e => e.Item.GetType().
                                                        GetProperty("Id").
                                                        GetValue(e));
            //var itemsID = mappedItems.Where(item => item.State == State.Deleted)
            //    .Select(e => e.Item)
            //    .Select(el => currentType.GetProperty("Id").GetValue(el));
            foreach (DataRow row in dt.Rows)
            {
                if (itemsID.Contains(row["Id"]))
                {
                    row.Delete();
                }
            }
        }

        private void AddDataIntoTable(DataTable dt)
        {
            foreach (var mappedElement in mappedItems.Where(item => item.State == State.Added))
            {
                #region Old code items adding

                //DataRow row = dt.NewRow();
                //foreach (var property in properties)
                //{
                //    if (property.GetCustomAttribute<ColumnAttribute>().ReadWriteOption == ReadWriteOption.Write)
                //    {
                //        continue;
                //    }
                //    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                //    row[tableColumnByPropertyAttribute] = property.GetValue(mappedElement.Item);
                //}
                //dt.Rows.Add(row);

                #endregion Old code items adding

                dt.Rows.Add(mappedElement.Row);
            }
        }

        #endregion Methods.Private

        public void FillItems()
        {
            //string sqlQuery = GetSqlQuery();
            DataTable dt = dbContext.GetTableWithData(tableName); //TODO: получить JOIN таблицу
            foreach (DataRow row in dt.Rows)
            {
                var test = new MappedItem<T>(row, State.Added);
                this.mappedItems.Add(test);
                //this.mappedItems.Add(new MappedItem<T>(row, State.Added));

                #region old mapper logic

                //T item = Activator.CreateInstance<T>();//TODO: проверить как создаются другие типы
                //foreach (var property in properties)
                //{
                //    //TODO: втулить проверку на наличие дочерних элементов
                //    //CreateChild();
                //    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                //    var propValue = row[tableColumnByPropertyAttribute];
                //    if (propValue.GetType() != typeof(DBNull))
                //    {
                //        property.SetValue(item, propValue);
                //    }
                //    else
                //    {
                //        property.SetValue(item, null);
                //    }
                //}
                //mappedItems.Add(new MappedItem<T>(item, State.Unchanched));

                #endregion old mapper logic
            }
        }

        public void Add(T item)
        {
            this.mappedItems.Add(new MappedItem<T>(item, State.Added));
            TransferItemsIntoDbTable();
        }

        public void Add(IEnumerable<T> items)
        {
            this.mappedItems.AddRange(items.Select(item => new MappedItem<T>(item, State.Added)));
            TransferItemsIntoDbTable();
        }

        public T ReadItemById(int id)
        {
            var test = this.mappedItems.Select(i => i.Item)
                                   .FirstOrDefault(item => (int)item.GetType()
                                                                    .GetProperty("Id") //TODO: а если поменяется имя свойства? Как убрать string из метода?
                                                                    .GetValue(item) == id);

            return this.mappedItems.Select(i => i.Item)
                                   .FirstOrDefault(item => (int)item.GetType()
                                   .GetProperty("Id") //TODO: а если поменяется имя свойства? Как убрать string из метода?
                                   .GetValue(item) == id);
        }

        public IEnumerable<T> ReadAllItems()
        {
            return this.mappedItems.Select(e => e.Item);
        }

        public void Update(T item)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var item = this.mappedItems.First(e => (int)e.Item.GetType()
                                                                .GetProperty("Id")
                                                                .GetValue(e.Item) == id);
            item.State = State.Deleted;
        }

        public void Delete(T item)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}