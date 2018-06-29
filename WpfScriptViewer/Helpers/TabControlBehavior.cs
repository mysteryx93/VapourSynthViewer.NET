using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Wraps tab item contents in UserControl to prevent TabControl from re-using its content
    /// </summary>
    public class TabControlBehavior {
        private static readonly HashSet<TabControl> _tabControls = new HashSet<TabControl>();
        private static readonly Dictionary<ItemCollection, TabControl> _tabControlItemCollections = new Dictionary<ItemCollection, TabControl>();

        public static bool GetDoNotCacheControls(TabControl tabControl) {
            return (bool)tabControl.GetValue(DoNotCacheControlsProperty);
        }

        public static void SetDoNotCacheControls(TabControl tabControl, bool value) {
            tabControl.SetValue(DoNotCacheControlsProperty, value);
        }

        public static readonly DependencyProperty DoNotCacheControlsProperty = DependencyProperty.RegisterAttached(
            "DoNotCacheControls",
            typeof(bool),
            typeof(TabControlBehavior),
            new UIPropertyMetadata(false, OnDoNotCacheControlsChanged));

        private static void OnDoNotCacheControlsChanged(
            DependencyObject depObj,
            DependencyPropertyChangedEventArgs e) {
            var tabControl = depObj as TabControl;
            if (null == tabControl)
                return;
            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                Attach(tabControl);
            else
                Detach(tabControl);
        }

        private static void Attach(TabControl tabControl) {
            if (!_tabControls.Add(tabControl))
                return;
            _tabControlItemCollections.Add(tabControl.Items, tabControl);
            ((INotifyCollectionChanged)tabControl.Items).CollectionChanged += TabControlUcWrapperBehavior_CollectionChanged;
        }

        private static void Detach(TabControl tabControl) {
            if (!_tabControls.Remove(tabControl))
                return;
            _tabControlItemCollections.Remove(tabControl.Items);
            ((INotifyCollectionChanged)tabControl.Items).CollectionChanged -= TabControlUcWrapperBehavior_CollectionChanged;
        }

        //private static Dictionary<TabItem, Panel> CacheList = new Dictionary<TabItem, Panel>();

        private static void TabControlUcWrapperBehavior_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            var itemCollection = (ItemCollection)sender;
            var tabControl = _tabControlItemCollections[itemCollection];
            IList items;
            if (e.Action == NotifyCollectionChangedAction.Reset)   /* our ObservableArray<T> swops out the whole collection */
                items = (ItemCollection)sender;
            else if (e.Action == NotifyCollectionChangedAction.Add)
                items = e.NewItems;
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                //foreach (object item in e.OldItems) {
                //    var ti = CacheList[item];
                //    if (ti != null && ti.Content is UserControl tu) {
                //        ti.Content = tu.Content;
                //    }
                //}
                return;
            } else
                return;

            foreach (var newItem in items) {
                var ti = tabControl.ItemContainerGenerator.ContainerFromItem(newItem) as TabItem;
                if (ti != null) {
                    var userControl = ti.Content as UserControl;
                    if (null == userControl)
                        ti.Content = new UserControl { Content = ti.Content };
                    //ti.Unloaded += (s2, e2) => {
                    //    Panel P = CacheList[s2 as TabItem];
                    //    P.Children.Add(s2 as TabItem);

                    //};
                    //Panel Parent = (VisualTreeHelper.GetParent(ti) as Panel)?.Parent as Panel;
                    //CacheList.Add(ti, Parent);
                }
            }
        }

        public class CacheItem {
            public object Data { get; set; }
            public TabItem Tab { get; set; }
            public Panel Parent { get; set; }

            public CacheItem() { }
            public CacheItem(object data, TabItem tab, Panel parent) {
                this.Data = data;
                this.Tab = tab;
                this.Parent = parent;
            }
        }
    }
}