using ORM_Repos_UoW.Attributes;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW.DataMapper
{
    public class MappedItem<T> where T : class
    {
        private T item;
        private DataRow row;
        private Type type = typeof(T);

        public MappedItem(T item, State state)
        {
            Item = item;
            State = state;
        }

        public MappedItem(DataRow row, State state)
        {
            Row = row;
            State = state;
        }

        public State State { get; set; }

        public DataRow Row
        {
            get => row;
            set
            {
                row = value;
                item = ConvertRowToObject(value);
            }
        }

        public T Item
        {
            get => this.item;
            set
            {
                item = value;
                row = ConvertObjectToRow(value);
            }
        }

        private T ConvertRowToObject(DataRow row)
        {
            T item = Activator.CreateInstance<T>();
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var value = row[columnName];
                prop.SetValue(item, value);
            }
            return item;
        }

        private DataRow ConvertObjectToRow(T item)
        {
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            var columns = properties.Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName)
                                    .Select(column => new DataColumn(column));
            DataTable dt = new DataTable("Items");
            dt.Columns.AddRange(columns.ToArray());
            DataRow row = dt.NewRow();
            foreach (var prop in properties)
            {
                var columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                row[columnName] = prop.GetValue(item);
            }
            return row;
        }
    }
}