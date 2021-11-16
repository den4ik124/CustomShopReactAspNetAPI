using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ORM_Repos_UoW
{
    public static class DataMapper
    {

        public static void MatchColumns<T>(DataTable table, T item)
        {
            var type = typeof(T);
            List<string> columnsName = GetTableColumnsHeaders(table);

            var props = type.GetProperties()
                                .Where(atr => atr.CustomAttributes
                                            .Any(i => i.AttributeType.Name == "ColumnAttribute"))
                                .ToArray();

            DataRow row = table.NewRow();
            for (int i = 0; i < columnsName.Count; i++)
            {
                var property = props.Where(atr => atr.CustomAttributes
                                                        .FirstOrDefault().ConstructorArguments
                                                        .FirstOrDefault()
                                                        .Value
                                                        .ToString() == columnsName[i]);
                if (property?.FirstOrDefault() == null) //TODO: подумать как избежать этой проверки
                {
                    continue;
                }
                var propertyValue = property?.FirstOrDefault().GetValue(item);
                row[i] = propertyValue;
            }
            row.Table.Rows.Add(row);
        }

        private static List<string> GetTableColumnsHeaders(DataTable table)
        {
            var columnsName = new List<string>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columnsName.Add(table.Columns[i].ColumnName);
            }

            return columnsName;
        }
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