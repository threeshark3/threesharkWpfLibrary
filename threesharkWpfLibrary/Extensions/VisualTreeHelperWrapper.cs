using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace threesharkWpfLibrary.Extensions
{
    public static class VisualTreeHelperWrapper
    {
        /// <summary>
        /// VisualTreeを親側にたどって、
        /// 指定した型の要素を探す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static T FindAncestor<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            while (depObj != null)
            {
                if (depObj is T target)
                {
                    return target;
                }
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            return null;
        }

        public static IEnumerable<DependencyObject> FindAncestors(this DependencyObject depObj)
        {
            if (depObj == null) { yield break; }
            depObj = VisualTreeHelper.GetParent(depObj);
            while (depObj != null)
            {
                yield return depObj;
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            yield break;
        }

        public static T FindDescendant<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) { return null; }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindDescendant<T>(child);
                if (result != null) { return result; }
            }
            return null;
        }
    }
}
