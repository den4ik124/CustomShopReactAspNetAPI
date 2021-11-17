using ReflectionExtensions;
using System;
using System.Collections.Generic;
using System.Data;
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
        private CustomAttributeData? attribute;
        private string? tableName;
        private PropertyInfo[]? properties;
        private DbContext dbContext;

        public List<T> Items { get;}
        public Dictionary<int,T> ItemsDict { get; set; }

        public DataMapper(DbContext dbContext)
        {
            this.currentType = typeof(T);
            this.attribute = currentType.GetCustomAttributesData()
                            .First(a => a.AttributeType.Name == tableAttribute);
            this.tableName = currentType.CustomAttributes.FirstOrDefault(atr => atr.AttributeType.Name == attribute.AttributeType.Name) ?
                            .ConstructorArguments.FirstOrDefault()
                            .Value?.ToString();
            this.properties = currentType.GetProperties()
                        .Where(atr => atr.CustomAttributes
                                    .Any(i => i.AttributeType.Name == columnAttribute))
                        .ToArray();
            this.dbContext = dbContext;
            Items = new List<T>() {};
        }
        public DataMapper(DbContext dbContext, T item) : this(dbContext)
        {
            Items = new List<T>(){item};
        }

        public DataMapper(DbContext dbContext, List<T> items): this(dbContext)
        {
            Items = items;
        }

        public void FillItems()
        {
            DataTable dt = dbContext.GetTable(tableName);
            foreach (DataRow row in dt.Rows)
            {
                var arguments = new List<object>();
                for (int i = 0; i < properties.Length /*row.ItemArray.Length*/; i++)
                {
                    var tableColumnByPropertyAttribute = properties[i].CustomAttributes //TODO: вылетаем за пределы массива props , т.к. у корабля нет поля TypeID, у Cell нет BattleFieldID
                                            .FirstOrDefault()?
                                            .ConstructorArguments
                                            .FirstOrDefault()
                                            .Value?.ToString();

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
                for (int i = 0; i < this.properties.Length; i++)
                {
                    var tableColumnByPropertyAttribute = properties[i].CustomAttributes //TODO: вылетаем за пределы массива props , т.к. у корабля нет поля TypeID, у Cell нет BattleFieldID
                        .FirstOrDefault()?
                        .ConstructorArguments
                        .FirstOrDefault()
                        .Value?.ToString();
                    row[tableColumnByPropertyAttribute] = properties[i].GetValue(item);
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
