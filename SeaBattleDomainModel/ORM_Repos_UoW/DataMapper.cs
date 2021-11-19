using ORM_Repos_UoW.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW
{
    public class DataMapper<T> where T : class
    {
        private Type currentType;
        private Attribute attribute;
        private string? tableName;
        private IEnumerable<PropertyInfo>? properties;
        private DbContext dbContext;
        public List<MappedItem<T>> MappedItems { get; set; }

        public DataMapper(DbContext dbContext)
        {
            this.currentType = typeof(T);
            this.attribute = currentType.GetCustomAttribute<TableAttribute>();
            this.tableName = ((TableAttribute)attribute).TableName;
            this.properties = currentType.GetProperties()
                                         .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);

            this.dbContext = dbContext;
            MappedItems = new List<MappedItem<T>>() { };
        }

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
                MappedItems.Add(new MappedItem<T>(item, State.Unchanched));
            }
        }

        public void TransferItemsIntoDbTable()
        {
            DataTable dt = dbContext.GetTable(tableName);
            foreach (var mappedElement in MappedItems)
            {
                DataRow row = dt.NewRow();
                foreach (var property in properties)
                {
                    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                    row[tableColumnByPropertyAttribute] = property.GetValue(mappedElement.Item);
                }

                switch (mappedElement.State)
                {
                    case State.Added:
                        row.SetAdded();
                        break;

                    case State.Modified:
                        row.SetModified();
                        break;
                }
                dt.Rows.Add(row);
            }
        }

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