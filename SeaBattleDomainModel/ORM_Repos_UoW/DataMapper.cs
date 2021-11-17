using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW
{
    public class DataMapper<T> : IDisposable
    {
        private const string tableAttribute = "TableAttribute";
        private const string columnAttribute = "ColumnAttribute";

        private Type currentType;
        private CustomAttributeData? attribute;
        private string? tableName;
        private PropertyInfo[]? properties;
        //= currentType.GetProperties()
        //                .Where(atr => atr.CustomAttributes
        //                            .Any(i => i.AttributeType.Name == columnAttribute))
        //                .ToArray();

        private DbContext dbContext;
        public List<T> Items { get; set; }
        public DataMapper(DbContext dbContext, bool isItemsShouldBeFilled = true)
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
            if(isItemsShouldBeFilled)   //TODO: Подумать как убрать эту проверку
                Items = FillItems();

        }
        public DataMapper(DbContext dbContext, T item) : this(dbContext, false)
        {
            Items = new List<T>(){item};
            TransferItemsIntoDB();
        }

        public DataMapper(DbContext dbContext, List<T> item): this(dbContext, false)
        {
            Items = item;
            TransferItemsIntoDB();
        }

        private List<T> FillItems()
        {
            DataTable dt = dbContext.GetTable(tableName);
            List<T> items = new List<T>();

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
                items.Add(item);
            }
            return items;
        }

        private void TransferItemsIntoDB()
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


        public DataRow MatchColumns<T>(DataTable table, T item)
        {
            var type = typeof(T);
            var props = type.GetProperties()
                                .Where(atr => atr.CustomAttributes
                                            .Any(i => i.AttributeType.Name == "ColumnAttribute"))
                                .ToArray();
            DataRow row = table.NewRow();
            for (int i = 0; i < props.Length; i++)
            {
                var propAttributeArgumentName = props[i].CustomAttributes
                                        .FirstOrDefault()?
                                        .ConstructorArguments
                                        .FirstOrDefault()
                                        .Value?.ToString();

                row[propAttributeArgumentName] = props[i]?.GetValue(item)?.ToString();
            }
            return row;
        }

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



        public void Add(DataTable table, IEnumerable<DataRow> rows)
        {
            foreach (DataRow row in rows)
            {
                table.Rows.Add(row);
            };
        }
        public void Update(DataTable table, DataRow[] rows)
        {
            foreach (DataRow row in rows)
            {
                if (table.Rows.Contains(row))
                {
                    table.Rows.Find(row)?.SetModified();
                }
            };
        }

        #region Dispose
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //Dispose(); //TODO: продумаь что тут высвобождать?
                }
            this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }


    //private static Dictionary<string, string> GetTableColumnsHeaders(DataTable table)
    //{
    //    var columns = new Dictionary<string, string>();
    //    //var columnsName = new List<string>();
    //    for (int i = 0; i < table.Columns.Count; i++)
    //    {
    //        columns.Add(table.Columns[i].ColumnName, "");
    //        //columnsName.Add(table.Columns[i].ColumnName);
    //    }
    //    //return columnsName;
    //    return columns;
    //}
    //private static string? GetTableName(System.Reflection.CustomAttributeData attributes, DbContext dbContext) //TODO: исправить на private вне тестов
    //{
    //    List<string> tablesNames = new List<string>();
    //    for (int i = 0; i < dbContext.TablesWithData.Tables.Count; i++)
    //    {
    //        tablesNames.Add(dbContext.TablesWithData.Tables[i].TableName);
    //    }
    //    var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value.ToString() == arg);
    //    return tableName;
    //}
    //private void AttributesOfGenericType()
    //{
    //var type = someSuperClass.GetType();
    //var genericsWithAttributes = type.GetGenericArguments()
    //    .Where(a => a.CustomAttributes
    //            .Any(item => item.ConstructorArguments
    //                    .Any(item => item.Value.ToString() == "ExampleTable"))).ToList();
    //var result = type.GetGenericArguments()
    //                 .Where(a => a.CustomAttributes.Any(item => item.AttributeType.Name == "TableAttribute")
    //                          && a.CustomAttributes.Any(item => item.ConstructorArguments
    //                                                                    .Any(item => item.Value.ToString() == "ExampleTable"))).ToList();
    //}
}
