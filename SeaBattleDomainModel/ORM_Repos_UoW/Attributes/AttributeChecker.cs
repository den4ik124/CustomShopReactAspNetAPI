using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmRepositoryUnitOfWork.Attributes
{
    public class AttributeChecker
    {
        public AttributeChecker()
        {
            CheckAssembly();
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Any());
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                CheckTableAttribute(type);
                CheckColumnAttribute(type);
            }
        }

        private void CheckAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetCustomAttributes<DomainModelAttribute>().Any());
            if (!assemblies.Any())
            {
                throw new Exception($"There are no any assembly marked with [{nameof(DomainModelAttribute)}].");
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if type marked with [TableAttribute]
        /// </summary>
        /// <param name="type">Type for analysis</param>
        /// <exception cref="Exception">If type was not marked Exception will be thrown</exception>
        public void CheckTableAttribute(Type type)
        {
            if (!type.GetCustomAttributes<TableAttribute>().Any())
            {
                throw new Exception($"Type {type.Name} was not marked by [{nameof(TableAttribute)}]");
            }
        }

        /// <summary>
        /// Check if properties marked with [ColumnAttribute]
        /// </summary>
        /// <param name="type">Type with properties for analysis</param>
        /// <exception cref="Exception">If no one property was marked Exception will be thrown</exception>
        public void CheckColumnAttribute(Type type)
        {
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Any());

            if (!properties.Any())
            {
                throw new Exception($"None property in {type.Name} was not marked by [{nameof(ColumnAttribute)}]");
            }
        }
    }
}