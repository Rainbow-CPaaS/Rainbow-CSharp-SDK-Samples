using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class ObjectsGroupingModel<K, T> : ObservableCollection<T>
    {
        public K Key { get; private set; }

        public ObjectsGroupingModel(K key, IEnumerable<T> items)
        {
            Key = key;

            foreach(var item in items)
            {
                this.Items.Add(item);
            }
            
        }
    }

}
