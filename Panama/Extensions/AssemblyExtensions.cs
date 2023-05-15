using System.Reflection;

namespace Panama.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Assembly> GetServiceAssemblies(this IEnumerable<Assembly>? assemblies)
        {
            var result = assemblies ?? new List<Assembly>();


            // domain built like so to overcome .net core .dll discovery issue 
            // within container:
            
            result = result.Concat(AppDomain.CurrentDomain.GetAssemblies());
            result = result.Concat(Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(x => Assembly.Load(x))
                .ToList());

            return result;
        }
    }
}
