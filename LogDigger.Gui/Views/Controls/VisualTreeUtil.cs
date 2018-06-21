using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Controls
{
    /// <summary>
    /// Helper class for operation on the visual tree
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class VisualTreeUtil
    {
        /// <summary>
        /// Check if a field has errors in the visual tree
        /// </summary>
        /// <param name="obj">Root element</param>
        /// <returns>Is valid</returns>
        public static bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors,  
            // and all of its children (that are dependency objects) are error-free. 
            return !System.Windows.Controls.Validation.GetHasError(obj) &&
                   LogicalTreeHelper.GetChildren(obj)
                       .OfType<DependencyObject>()
                       .All(IsValid);
        }

        public static T GetFirstVisibleItems<T>(ItemsControl itemsControl) 
            where T : class
        {
            HitTestResult hitTest = VisualTreeHelper.HitTest(itemsControl, new Point(10, 10));
            if (hitTest != null)
            {
                return GetParentUntil<T>(itemsControl, hitTest.VisualHit);
            }
            return null;
        }

        private static T GetParentUntil<T>(object sender, object originalSource) 
            where T : class
        {
            DependencyObject depObj = originalSource as DependencyObject;
            if (depObj != null)
            {
                DependencyObject current = depObj;
                while (current != null && current != sender)
                {
                    var listViewItem = current as T;
                    if (listViewItem != null)
                    {
                        return listViewItem;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
            }

            return null;
        }

        public static T FindVisualAncestorByType<T>(DependencyObject dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return default(T);
            }

            if (dependencyObject is T)
            {
                return (T)dependencyObject;
            }

            var parent = FindVisualAncestorByType<T>(VisualTreeHelper.GetParent(dependencyObject));
            return parent;
        }

        /// <summary>
        /// Find an ancestor in the visual tree, even in a Ms Framework popup element (Do not use with FxPopup)
        /// </summary>
        /// <typeparam name="T">The type to find</typeparam>
        /// <param name="frameworkElement">The element which is looked up</param>
        /// <returns>The ancestor</returns>
        public static T FindVisualAncestorByTypeThroughMsPopup<T>(FrameworkElement frameworkElement)
            where T : FrameworkElement
        {
            if (frameworkElement == null)
            {
                return default(T);
            }

            if (frameworkElement is T)
            {
                return (T)frameworkElement;
            }

            var parent = FindVisualAncestorByTypeThroughMsPopup<T>(VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement);
            if (parent == null)
            {
                parent = FindVisualAncestorByTypeThroughMsPopup<T>(VisualTreeHelper.GetParent(frameworkElement.Parent) as FrameworkElement);
            }
            return parent;
        }

         //[SuppressMessage("Microsoft.Usage", "CA1800", Justification = "Cast")]
        public static T FindVisualAncestorByName<T>(DependencyObject dependencyObject, string name)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return default(T);
            }
            var frameworkElement = dependencyObject as FrameworkElement;
            if (frameworkElement != null && frameworkElement.Name == name)
            {
                return (T)dependencyObject;
            }

            var parent = FindVisualAncestorByName<T>(VisualTreeHelper.GetParent(dependencyObject), name);
            return parent;
        }

        public static T FindVisualChildByType<T>(DependencyObject dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return default(T);
            }

            if (dependencyObject is T)
            {
                return (T)dependencyObject;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = FindVisualChildByType<T>(VisualTreeHelper.GetChild(dependencyObject, i));
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the top level parent in the visual tree for the specified element
        /// </summary>
        /// <param name="element">A FrameworkElement in the visual tree</param>
        /// <returns>The top level parent</returns>
        public static FrameworkElement FindTopLevelElement(FrameworkElement element)
        {
            FrameworkElement iterator, nextUp = element;
            do
            {
                iterator = nextUp;
                nextUp = VisualTreeHelper.GetParent(iterator) as FrameworkElement;
            }
            while (nextUp != null);
            return iterator;
        }

        /// <summary>
        /// Find all visual children of type T in the visual tree
        /// </summary>
        /// <typeparam name="T">Type to find</typeparam>
        /// <param name="depObj">Root element</param>
        /// <returns>Visual children</returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : class
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return child as T;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T GetFirstElementInItemGeneratorOfType<T>(ItemContainerGenerator itemGenerator, int index) where T : class
        {
            DependencyObject containerFromIndex = itemGenerator.ContainerFromIndex(index);
            IEnumerable<T> enumeration = FindVisualChildren<T>(containerFromIndex).ToList();
            T elem = null;
            if (enumeration.Count() != 0)
            {
                elem = enumeration.First();
            }
            return elem;
        }

        public static T GetFirstElementInItemGeneratorOfType<T>(ItemContainerGenerator itemGenerator, object item) where T : class
        {
            DependencyObject containerFromItem = itemGenerator.ContainerFromItem(item);
            IEnumerable<T> enumeration = FindVisualChildren<T>(containerFromItem).ToList();
            T elem = null;
            if (enumeration.Count() != 0)
            {
                elem = enumeration.First();
            }
            return elem;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found,
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static T GetChild<T>(DependencyObject root) where T : class
        {
            var asT = root as T;
            if (asT != null)
            {
                return asT;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i) as FrameworkElement;

                if (child != null)
                {
                    var found = GetChild<T>(child);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public static void GetAllChild<T>(DependencyObject root, IList<T> l) where T : class
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                if (child != null)
                {
                    var found = GetChild<T>(child);
                    if (found != null)
                    {
                        l.Add(found);
                    }
                    GetAllChild(child, l);
                }
            }
        }

        public static T GetParent<T>(FrameworkElement root, int maxDeep) where T : FrameworkElement
        {
            if (maxDeep <= 0)
            {
                return null;
            }

            if (root == null)
            {
                return null;
            }

            if (root is T)
            {
                return (T)root;
            }

            var parent = VisualTreeHelper.GetParent(root);

            if (parent == null)
            {
                return null;
            }

            return GetParent<T>(parent as FrameworkElement, maxDeep - 1);
        }

        public static bool IsParentOfTypeOrPopup(FrameworkElement root, FrameworkElement parentElement, int maxDeep)
        {
            if (maxDeep <= 0)
            {
                return false;
            }

            if (root == parentElement)
            {
                return true;
            }

            if (root == null)
            {
                return false;
            }

            if (root.GetType().Name.EndsWith("PopupRoot"))
            {
                return true;
            }

            var parent = VisualTreeHelper.GetParent(root);

            if (parent == null)
            {
                return false;
            }

            return IsParentOfTypeOrPopup(parent as FrameworkElement, parentElement, maxDeep - 1);
        }

        public static bool IsParentOfTypeOrPopup<T>(FrameworkElement root, int maxDeep) where T : FrameworkElement
        {
            if (maxDeep <= 0)
            {
                return false;
            }

            if (root is T)
            {
                return true;
            }

            if (root == null)
            {
                return false;
            }

            if (root.GetType().Name.EndsWith("PopupRoot"))
            {
                return true;
            }

            var parent = VisualTreeHelper.GetParent(root);

            if (parent == null)
            {
                return false;
            }

            return IsParentOfTypeOrPopup<T>(parent as FrameworkElement, maxDeep - 1);
        }

        public static bool IsParentOf(FrameworkElement child, FrameworkElement parent)
        {
            if (child == null)
            {
                return false;
            }

            if (child == parent)
            {
                return true;
            }

            child = VisualTreeHelper.GetParent(child) as FrameworkElement;

            if (child == null)
            {
                return false;
            }

            return IsParentOf(child, parent);
        }

        public static bool IsChildUnder<T>(DependencyObject initalChild, DependencyObject finalParent, bool testSubClass) where T : DependencyObject
        {
            // 'GetParent' of the VisualTreeHelper crash if the argument is a Visual
            if (!(initalChild is Visual))
            {
                return false;
            }
            var parent = VisualTreeHelper.GetParent(initalChild);
            var paramType = typeof(T);

            while (parent != null && (finalParent == null || !Equals(finalParent, parent)))
            {
                var parentType = parent.GetType();

                if (parentType == paramType || (testSubClass && parentType.IsSubclassOf(paramType)))
                {
                    return true;
                }

                // handle the popup case
                if (parentType.Name == "PopupRoot")
                {
                    parent = ((FrameworkElement)parent).Parent;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }

            return false;
        }

        public static bool IsChildOf(DependencyObject child, DependencyObject potentialParent)
        {
            // 'GetParent' of the VisualTreeHelper crash if the argument is a Visual
            if (!(child is Visual))
            {
                return false;
            }
            var parent = VisualTreeHelper.GetParent(child);

            while (parent != null)
            {
                if (parent.Equals(potentialParent))
                {
                    return true;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return false;
        }

        public static bool IntersectsWith(this FrameworkElement first, FrameworkElement second)
        {
            var commonAncestor = first.FindCommonVisualAncestor(second) as Visual;
            if (commonAncestor == null)
            {
                return false;
            }
            var firstCoordToAncestor = first.TransformToAncestor(commonAncestor).Transform(new Point(0, 0));
            var firstRect = new Rect(firstCoordToAncestor.X, firstCoordToAncestor.Y, first.ActualWidth, first.ActualHeight);

            var secondCoordToAncestor = second.TransformToAncestor(commonAncestor).Transform(new Point(0, 0));
            var secondRect = new Rect(secondCoordToAncestor.X, secondCoordToAncestor.Y, second.ActualWidth, second.ActualHeight);
            if (firstRect.IntersectsWith(secondRect))
            {
                return true;
            }
            return false;
        }
    }
}