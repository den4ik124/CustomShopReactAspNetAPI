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
    public class MappedItem<T> : IMappedItem<T> where T : class
    {
        private Assembly assembly;
        private T item;
        private DataRow row;
        private Type type = typeof(T);

        public Dictionary<string, object> PropertyValue { get; set; }

        public MappedItem()
        {
            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
            this.TableName = typeof(T).GetCustomAttribute<TableAttribute>().TableName;
        }

        public string TableName { get; }

        public MappedItem(T item, State state) : this()
        {
            this.Item = item;
            this.State = state;
        }

        public MappedItem(DataRow row, State state) : this()
        {
            this.Row = row;
            this.State = state;
        }

        public State State { get; set; }

        public DataRow Row
        {
            get => row;
            set
            {
                this.row = value;
                this.item = ConvertRowToObject(value);
                PropertyValue = GetPropertyValue(this.item);
            }
        }

        public T Item
        {
            get => this.item;
            set
            {
                this.item = value;
                PropertyValue = GetPropertyValue(this.item);
                this.row = ConvertObjectToRow(value);
            }
        }

        private Dictionary<string, object> GetPropertyValue(T item)
        {
            var dictionary = new Dictionary<string, object>();
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                dictionary.Add(prop.GetCustomAttribute<ColumnAttribute>().ColumnName, prop.GetValue(item));
            }
            return dictionary;
        }

        private T ConvertRowToObject(DataRow row) //TODO: обработать вложенные сущности здесь !!!
        {
            var item = CreateItemInstance(row, typeof(T));

            return (T)item;
        }

        private object CreateItemInstance(DataRow row, Type type)
        {
            object item;
            var subEntities = type.GetProperties().Where(prop => prop.GetCustomAttributes<ChildAttribute>().Count() > 0);
            Dictionary<int, Type> idType = new Dictionary<int, Type>();
            if (type.IsAbstract)
            {
                var derivedTypes = assembly.ExportedTypes.Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0)
                                                         .Where(t => t.GetCustomAttribute<InheritanceRelationAttribute>().IsBase == false);

                foreach (var derivedType in derivedTypes)
                {
                    idType.Add(derivedType.GetCustomAttribute<ShipTypeAttribute>().ShipTypeID, derivedType);
                }
                if (row["TypeId"].GetType() == typeof(DBNull))
                {
                    return null;
                }
                type = idType[(int)row["TypeId"]];
            }

            item = Activator.CreateInstance(type);
            //T item = Activator.CreateInstance<T>();
            var properties = item.GetType().GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var value = row[columnName];
                if (value.GetType() != typeof(DBNull))
                {
                    prop.SetValue(item, value);
                }
                else
                {
                    prop.SetValue(item, null);
                }
            }

            if (subEntities.Count() == 0)
            {
                return item;
            }

            //var types = assemblies.Where(t => t.DefinedTypes.Contains(typeof(Ship).GetTypeInfo()));//.Select(type => type.GetTypes().Where(prop => prop.GetCustomAttributes<ShipTypeAttribute>().Count() > 0));
            foreach (var subEntity in subEntities)
            {
                var childType = subEntity.GetCustomAttribute<ChildAttribute>().RelatedType;
                object value;
                if (row["TypeId"].GetType() == typeof(DBNull) && childType.GetCustomAttribute<TableAttribute>().TableName == "Ships")
                {
                    subEntity.SetValue(item, null);
                    continue;
                }

                if (childType.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                    && childType.GetCustomAttribute<InheritanceRelationAttribute>().IsBase) // TODO: подумать как сделать так, что условие выполнялось только если создаем корабль
                {
                    if (idType.ContainsKey((int)row["TypeId"]))
                    {
                        value = CreateItemInstance(row, idType[(int)row["TypeId"]]);
                    }
                    else
                    {
                        value = CreateItemInstance(row, subEntity.PropertyType);
                    }
                }
                else
                {
                    value = CreateItemInstance(row, subEntity.PropertyType);
                }
                subEntity.SetValue(item, value);
            }
            return item;
        }

        private DataRow ConvertObjectToRow(T item) //TODO: доделать корректное создание/заполнение строки
        {
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            var columns = properties.Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName)
                                    .Select(column => new DataColumn(column));
            DataTable dt = new DataTable("Items");
            dt.Columns.AddRange(columns.ToArray());
            DataRow row = dt.NewRow();
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<ColumnAttribute>().ReadWriteOption == ReadWriteOption.Write)
                {
                    continue;
                }
                var columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                row[columnName] = prop.GetValue(item);
            }
            return row;
        }
    }
}