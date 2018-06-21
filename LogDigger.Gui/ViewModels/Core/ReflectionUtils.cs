using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// Reflection utility methods
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Get Assemblies
        /// </summary>
        public static Assembly[] GetAssemblies(IEnumerable<Type> types)
        {
            HashSet<Assembly> result = new HashSet<Assembly>();
            foreach (var type in types)
            {
                result.Add(type.GetTypeInfo().Assembly);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Check if the given type inherits from the given generic type
        /// </summary>
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }

        /// <summary>
        /// Finds all the types derived from the type provided, in the assembly provided
        /// </summary>
        /// <param name="assembly">The assembly where to look for derived types</param>
        /// <param name="baseType">The base type of which the result types are derived from</param>
        /// <param name="classOnly">If true, result types will only be classes</param>
        /// <param name="ignoreAbstract">If true, abstract types will be ignored</param>
        public static IEnumerable<Type> FindDerivedTypesFromAssembly(Assembly assembly, Type baseType, bool classOnly, bool ignoreAbstract)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "Assembly must be defined ");
            }
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType), "Parent Type must be defined");
            }

            // get all the types
            var typeInfos = assembly.GetLoadableTypes();

            return FindDerivedTypes(baseType, classOnly, ignoreAbstract, typeInfos);
        }

        /// <summary>
        /// Finds all the types derived from the type provided, in the type list provided
        /// </summary>
        public static IEnumerable<Type> FindDerivedTypes(Type baseType, bool classOnly, bool ignoreAbstract, IEnumerable<TypeInfo> typeInfos)
        {
            // works out the derived types
            foreach (var typeInfo in typeInfos)
            {
                // if classOnly, it must be a class
                // useful when you want to create instance
                if ((classOnly && !typeInfo.IsClass) || (typeInfo.IsAbstract && ignoreAbstract))
                {
                    continue;
                }

                if (baseType.GetTypeInfo().IsInterface)
                {
                    var it = typeInfo.ImplementedInterfaces.FirstOrDefault(itf => itf.Name.Equals(baseType.Name));

                    if (it != null)
                    {
                        // add it to result list
                        yield return typeInfo.AsType();
                    }
                }
                else if (typeInfo.IsSubclassOf(baseType))
                {
                    // add it to result list
                    yield return typeInfo.AsType();
                }
            }
        }

        /// <summary>
        /// Finds all the types decorated with the given attribute in the assemblies provided
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to look for</typeparam>
        /// <param name="assemblies">The assemblies where to look for decorated types</param>
        public static IDictionary<Type, TAttribute> FindDecoratedTypesFromAssembly<TAttribute>(params Assembly[] assemblies)
        {
            return FindDecoratedTypesFromAssembly<TAttribute>(_ => true, assemblies);
        }

        /// <summary>
        /// Finds all the types decorated with the given attribute in the assemblies provided
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to look for</typeparam>
        /// <param name="filter">Filter for the attribute</param>
        /// <param name="assemblies">The assemblies where to look for decorated types</param>
        public static IDictionary<Type, TAttribute> FindDecoratedTypesFromAssembly<TAttribute>(Func<TAttribute, bool> filter, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentNullException(nameof(assemblies), "Assembly must be defined ");
            }
            Dictionary<Type, TAttribute> result = new Dictionary<Type, TAttribute>();

            // get all the types
            foreach (var a in assemblies)
            {
                var typeInfos = a.GetLoadableTypes();

                // works out the derived types
                foreach (var typeInfo in typeInfos)
                {
                    foreach (TAttribute attribute in typeInfo.GetCustomAttributes(false).OfType<TAttribute>().Where(filter))
                    {
                        result.Add(typeInfo.AsType(), attribute);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Finds all the types decorated with the given attribute in the assemblies provided
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to look for</typeparam>
        /// <param name="filter">Filter for the attribute</param>
        /// <param name="assemblies">The assemblies where to look for decorated types</param>
        public static IList<Tuple<Type, TAttribute>> MultipleFindDecoratedTypesFromAssembly<TAttribute>(Func<TAttribute, bool> filter, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentNullException(nameof(assemblies), "Assembly must be defined ");
            }
            IList<Tuple<Type, TAttribute>> result = new List<Tuple<Type, TAttribute>>();

            // get all the types
            foreach (var a in assemblies)
            {
                var typeInfos = a.GetLoadableTypes();

                // works out the derived types
                foreach (var typeInfo in typeInfos)
                {
                    var attributes = typeInfo.GetCustomAttributes(false).OfType<TAttribute>().Where(filter).ToList();
                    foreach (var attribute in attributes)
                    {
                        result.Add(new Tuple<Type, TAttribute>(typeInfo.AsType(), attribute));
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///  An System.Attribute array that contains the custom attributes of type type applied to element, or an empty array if no such custom attributes exist.
        /// </summary>
        public static Attribute[] GetCustomAttributes(MemberInfo element, Type type, bool inherit)
        {
            return element.GetCustomAttributes(type, inherit).OfType<Attribute>().ToArray();
        }

        /// <summary>
        /// Instanciate each derived types in Assemblies list
        /// </summary>
        /// <typeparam name="TBase">Base interface</typeparam>
        /// <param name="assemblies">assemblies where types will to searched</param>
        /// <returns>List of instance</returns>
        public static IEnumerable<TBase> BuildAllDerivedInstancesOf<TBase>(params Assembly[] assemblies) where TBase : class
        {
            var types = assemblies.SelectMany(assembly => FindDerivedTypesFromAssembly(assembly, typeof(TBase), false, true));

            var instances = from type in types
                let ctor = type.GetConstructor(new Type[0])
                where ctor != null && !type.GetTypeInfo().ContainsGenericParameters
                select ctor.Invoke(null) as TBase;

            return instances;
        }
    }
}