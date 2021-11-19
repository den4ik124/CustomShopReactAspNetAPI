﻿using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW
{
    public class DataMapper<T> : IDataMaper<T> where T : class
    {
        private Type currentType;
        private Attribute attribute;
        private string? tableName;
        private IEnumerable<PropertyInfo>? properties;
        private DbContext dbContext;
        private List<MappedItem<T>> mappedItems;

        public int ItemsCount { get => mappedItems.Count; }

        public DataMapper(DbContext dbContext)
        {
            this.currentType = typeof(T);
            this.attribute = currentType.GetCustomAttribute<TableAttribute>();
            this.tableName = ((TableAttribute)attribute).TableName;
            this.properties = currentType.GetProperties()
                                         .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
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
                DataRow row = dt.NewRow();
                foreach (var property in properties)
                {
                    if (property.GetCustomAttribute<ColumnAttribute>().ReadWriteOption == ReadWriteOption.Write)
                    {
                        continue;
                    }
                    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                    row[tableColumnByPropertyAttribute] = property.GetValue(mappedElement.Item);
                }
                dt.Rows.Add(row);
            }
        }

        #endregion Methods.Private

        public void FillItems()
        {
            DataTable dt = dbContext.GetTableWithData(tableName);
            foreach (DataRow row in dt.Rows)
            {
                T item = Activator.CreateInstance<T>();//TODO: проверить как создаются другие типы
                foreach (var property in properties)
                {
                    //TODO: втулить проверку на наличие дочерних элементов
                    //CreateChild();
                    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                    var propValue = row[tableColumnByPropertyAttribute];
                    if (propValue.GetType() != typeof(DBNull))
                    {
                        property.SetValue(item, propValue);
                    }
                    else
                    {
                        property.SetValue(item, null);
                    }
                }
                mappedItems.Add(new MappedItem<T>(item, State.Unchanched));
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

        public T ReadItem(int id)
        {
            return this.mappedItems.Select(i => i.Item)
                                   .FirstOrDefault(item => (int)item.GetType()
                                   .GetProperty("Id")
                                   .GetValue(item) == id);
        }

        public IEnumerable<T> ReadAllItems()
        {
            return this.mappedItems.Select(e => e.Item);
        }

        public void Delete(int id)
        {
            var item = this.mappedItems.First(e => (int)e.Item.GetType().GetProperty("Id").GetValue(e.Item) == id);
            item.State = State.Deleted;
        }

        #endregion Methods

        //public DataRow MatchColumns<T>(DataTable table, T item)
        //{
        //    var type = typeof(T);
        //    var props = type.GetProperties()
        //                        .Where(atr => atr.CustomAttributes
        //                                    .Any(i => i.AttributeType.Name == "ColumnAttribute"))
        //                        .ToArray();
        //    DataRow row = table.NewRow();
        //    for (int i = 0; i < props.Length; i++)
        //    {
        //        var propAttributeArgumentName = props[i].CustomAttributes
        //                                .FirstOrDefault()?
        //                                .ConstructorArguments
        //                                .FirstOrDefault()
        //                                .Value?.ToString();

        //        row[propAttributeArgumentName] = props[i]?.GetValue(item)?.ToString();
        //    }
        //    return row;
        //}

        //private static string? GetTableName(DbContext dbContext, System.Reflection.CustomAttributeData attributes) //TODO: исправить на private вне тестов
        //{
        //    List<string> tablesNames = new List<string>();
        //    for (int i = 0; i < dbContext.tablesWithData.Tables.Count; i++)
        //    {
        //        tablesNames.Add(dbContext.tablesWithData.Tables[i].TableName);
        //    }
        //    var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value.ToString() == arg);
        //    return tableName;
        //}
    }
}