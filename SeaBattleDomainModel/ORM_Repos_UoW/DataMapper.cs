using ReflectionExtensions;
using SeaBattleDomainModel.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW
{
    public class DataMapper<T>
    {
        private const string tableAttribute = "TableAttribute";
        private const string columnAttribute = "ColumnAttribute";

        private Type currentType;
        //private CustomAttributeData? attribute;
        private Attribute attribute;
        private string? tableName;
        private IEnumerable<PropertyInfo>? properties;
        private DbContext dbContext;

        public List<T> Items { get;}
        //public Dictionary<int,T> ItemsDict { get; set; }

        private Dictionary<PropertyInfo, string>? _propertiesTables;
        public DataMapper(DbContext dbContext)
        {
            _propertiesTables = new Dictionary<PropertyInfo, string>();
            this.currentType = typeof(T);
            this.attribute = currentType.GetCustomAttribute<TableAttribute>();

            this.tableName = ((TableAttribute)attribute).TableName;
            //this.tableName = currentType.CustomAttributes.FirstOrDefault(atr => atr.AttributeType == attribute.GetType()) ?
            //                .ConstructorArguments.FirstOrDefault()
            //                .Value?.ToString();
            this.properties = currentType.GetProperties()
                                         .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);

            this.dbContext = dbContext;
            Items = new List<T>() {};
        }
        public DataMapper(DbContext dbContext, T item) : this(dbContext)
        {
            Items.Add(item);
        }

        public DataMapper(DbContext dbContext, List<T> items): this(dbContext)
        {
            Items.AddRange(items);
        }

        public void FillItems()
        {
            DataTable dt = dbContext.GetTableWithData(tableName);
            foreach (DataRow row in dt.Rows)
            {
                var arguments = new List<object>();
                foreach (var property in properties)
                {
                    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                    arguments.Add(row[tableColumnByPropertyAttribute]);
                }
                T item = (T)Activator.CreateInstance(currentType, arguments.ToArray());
                Items.Add(item);
            }
        }

        public void TransferItemsIntoDB()
        {
            DataTable dt = dbContext.GetTable(tableName);
            foreach (var item in Items)
            {
                DataRow row = dt.NewRow();
                foreach (var property in properties)
                {
                    var tableColumnByPropertyAttribute = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                    row[tableColumnByPropertyAttribute] = property.GetValue(item);
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
