using System;
using System.Windows;
using System.Windows.Markup;
using SimpleLogger;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// A "by code" implementation of a view injector.
    /// DataTemplates are built in C# and added to the ResourceDictionary when using the RegisterInjection method.
    /// </summary>
    public abstract class ACodeViewInjector
    {
        protected ACodeViewInjector()
        {
            ResourceDictionary = new ResourceDictionary();
        }

        protected void RegisterInjection(Type viewModelType, Type viewType)
        {
            // The only way to build DataTemplate from code is to parse XAML
            // The FrameworkElementFactory way is now deprecated and does not support some XAML elements
            var parserContext = new ParserContext();
            parserContext.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            parserContext.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            parserContext.XmlnsDictionary.Add("v", "clr-namespace:" + viewType.Namespace + ";assembly=" + viewType.Assembly.GetName().Name);
            string xamlDataTemplate =
                "<DataTemplate>" +
                "<v:" + viewType.Name + "/>" +
                "</DataTemplate>";
            var dataTemplate = (DataTemplate)XamlReader.Parse(xamlDataTemplate, parserContext);
            dataTemplate.DataType = viewModelType;
            var dataTemplateKey = new DataTemplateKey(dataTemplate.DataType);
            foreach (var key in ResourceDictionary.Keys)
            {
                if (Equals(key, dataTemplateKey))
                {
                    var oldDataTemplate = (DataTemplate)ResourceDictionary[key];
                    var oldType = oldDataTemplate.Template.GetType();
                    Logger.Log(Logger.Level.Info, $"The view model {viewModelType.Name} previously associated with the view {oldType.Name} has been redefined in the view injection by the view {viewType.Name}");
                    ResourceDictionary.Remove(key);
                }
            }
            ResourceDictionary.Add(dataTemplateKey, dataTemplate);
        }

        protected void RegisterInjection<TViewModel, TView>()
            where TViewModel : AViewModel
            where TView : FrameworkElement
        {
            RegisterInjection(typeof(TViewModel), typeof(TView));
        }

        public abstract void Init();

        public ResourceDictionary ResourceDictionary { get; }
    }
}