using System.Windows;
using System.Windows.Controls;

namespace LogDigger.Gui.Views.DataTemplates
{
    /// <summary>
    /// This template manage datatemplate associed with generic object
    /// </summary>
    public class GenericTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return base.SelectTemplate(item, container);
            }
            var type = item.GetType();
            while (type != typeof(object))
            {
                var element = container as FrameworkElement;
                var key = new DataTemplateKey(type);
                var foundTemplate = element.TryFindResource(key) as DataTemplate;
                if (foundTemplate != null)
                {
                    return foundTemplate;
                }
                // navigate through generic type definition first
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    type = type.GetGenericTypeDefinition();
                }
                // then through base type
                else
                {
                    type = type.BaseType;
                }
            }
            // in last resort, just fall back to default data template retrieval mechanism
            return base.SelectTemplate(item, container);
        }
    }
}
