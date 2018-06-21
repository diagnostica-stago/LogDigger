using System;
using System.Windows;
using System.Windows.Data;
using Dragablz;

namespace LogDigger.Gui.Views.Controls
{
    public class DefaultInterLayoutClientEx : IInterLayoutClient
    {
        public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
        {
            TabablzControl tabablzControl1 = new TabablzControl();
            object dataContext = source.DataContext;
            tabablzControl1.DataContext = dataContext;
            TabablzControl tabablzControl2 = tabablzControl1;
            DefaultInterLayoutClientEx.Clone((DependencyObject) source, (DependencyObject) tabablzControl2);
            InterTabController interTabController = new InterTabController()
            {
                Partition = source.InterTabController.Partition
            };
            DefaultInterLayoutClientEx.Clone((DependencyObject) source.InterTabController, (DependencyObject) interTabController);
            tabablzControl2.SetCurrentValue(TabablzControl.InterTabControllerProperty, (object) interTabController);
            return (INewTabHost<UIElement>) new NewTabHost<UIElement>((UIElement) tabablzControl2, tabablzControl2);
        }

        private static void Clone(DependencyObject from, DependencyObject to)
        {
            LocalValueEnumerator localValueEnumerator = from.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                LocalValueEntry current = localValueEnumerator.Current;
                if (!current.Property.ReadOnly)
                {
                    current = localValueEnumerator.Current;
                    if (!(current.Value is FrameworkElement))
                    {
                        current = localValueEnumerator.Current;
                        if (!(current.Value is BindingExpressionBase))
                        {
                            DependencyObject dependencyObject = to;
                            current = localValueEnumerator.Current;
                            DependencyProperty property = current.Property;
                            current = localValueEnumerator.Current;
                            object obj = current.Value;
                            try
                            {
                                dependencyObject.SetCurrentValue(property, obj);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }
    }
}