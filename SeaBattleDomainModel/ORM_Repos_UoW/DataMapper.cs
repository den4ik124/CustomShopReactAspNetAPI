using System.Linq;

namespace ORM_Repos_UoW
{
    public class DataMapper
    {
        private void AttributesOfGenericType()
        {
            var type = someSuperClass.GetType();
            var genericsWithAttributes = type.GetGenericArguments()
                .Where(a => a.CustomAttributes
                        .Any(item => item.ConstructorArguments
                                .Any(item => item.Value.ToString() == "ExampleTable"))).ToList();
            var result = type.GetGenericArguments()
                             .Where(a => a.CustomAttributes.Any(item => item.AttributeType.Name == "TableAttribute")
                                      && a.CustomAttributes.Any(item => item.ConstructorArguments
                                                                                .Any(item => item.Value.ToString() == "ExampleTable"))).ToList();
        }
    }
}