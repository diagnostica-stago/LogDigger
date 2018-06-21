using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// Extensions methods for Assembly
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get the loadable types from the assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        /// <returns>The types</returns>
        public static IEnumerable<TypeInfo> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
            }
        }
    }
}