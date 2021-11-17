using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class DataMapper<T>
    {
        private DbContext dbContext;
        public List<T> Items { get; set; }
        public DataMapper(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public DataMapper(DbContext dbContext, T item) : this(dbContext)
        {
            Items = new List<T>(){item};
        }
        public DataMapper(DbContext dbContext, List<T> item): this(dbContext)
        {
            Items = item;
        }

        private void FillItems()
        {
            var type = typeof(T);
            var tableName = type.CustomAttributes.FirstOrDefault()
                                .ConstructorArguments.FirstOrDefault()
                                .Value.ToString();
            var columnsName = type.GetProperties()
                                    .Select(p => p.CustomAttributes.FirstOrDefault()
                                                  .ConstructorArguments.FirstOrDefault()
                                                  .Value.ToString());
            foreach (var item in )
            {

            }

            Items = new List<T>();

                       //var attributes = type.GetCustomAttributesData().First(a => a.AttributeType.Name == "TableAttribute");//.ConstructorArguments;
            ;
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
                                        .FirstOrDefault()
                                        .ConstructorArguments
                                        .FirstOrDefault()
                                        .Value.ToString();

                row[propAttributeArgumentName] = props[i].GetValue(item).ToString();
            }
            return row;
            //row.Table.Rows.Add(row);
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
                    table.Rows.Find(row).SetModified();
                }
            };
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
}