using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// A view injector implementation using attributes to define which view is injected for which view model.
    /// </summary>
    public class AttributeViewInjector : ACodeViewInjector
    {
        private readonly IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Construct the injector with a list of types which are contained by the assemblies to visit.
        /// The order of types is important.
        /// It defines the priority of injection: the last injection found is the one used.
        /// </summary>
        public AttributeViewInjector(params Type[] types)
            : this(types.Select(Assembly.GetAssembly).ToArray())
        {
        }

        /// <summary>
        /// Construct the injector with a list of assemblies to visit.
        /// The order of assemblies is important.
        /// It defines the priority of injection: the last injection found is the one used.
        /// </summary>
        public AttributeViewInjector(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public override void Init()
        {
            var viewFactoryAttributes = ReflectionUtils.MultipleFindDecoratedTypesFromAssembly<InjectViewAttribute>(t => t.InjectionKey == null, _assemblies.ToArray());

            // select the highest overriding level on the duplicated view model types
            var uniqueValues = InjectViewAttributeHelper.GetHighestLevelInjections(viewFactoryAttributes);

            foreach (var typeAttribute in uniqueValues)
            {
                Type viewModelType = typeAttribute.Item2.ViewModelType;
                RegisterInjection(viewModelType, typeAttribute.Item1);
            }
        }
    }
}