using System;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// An attribute applied to a view to tell the view injector for which view model the view is injected
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class InjectViewAttribute : Attribute
    {
        private readonly Type _injectionKey;
        private readonly Type _viewModelType;
        private readonly int _overridingLevel;

        public InjectViewAttribute(Type viewModelType)
            : this(viewModelType, 0)
        {
        }

        public InjectViewAttribute(Type viewModelType, int overridingLevel)
            : this(viewModelType, null, overridingLevel)
        {
        }

        public InjectViewAttribute(Type viewModelType, Type injectionKey)
            : this(viewModelType, injectionKey, 0)
        {
        }

        public InjectViewAttribute(Type viewModelType, Type injectionKey, int overridingLevel)
        {
            _injectionKey = injectionKey;
            _overridingLevel = overridingLevel;
            _viewModelType = viewModelType;
        }

        /// <summary>
        /// The type to do the mapping from
        /// </summary>
        public Type ViewModelType
        {
            get { return _viewModelType; }
        }

        public int OverridingLevel
        {
            get { return _overridingLevel; }
        }

        public Type InjectionKey
        {
            get { return _injectionKey; }
        }
    }
}