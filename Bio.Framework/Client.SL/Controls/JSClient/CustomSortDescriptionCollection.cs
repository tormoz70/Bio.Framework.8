using System.ComponentModel;
using System.Collections.Specialized;

namespace Bio.Framework.Client.SL {
    /// <summary>
    /// CustomSortDescriptionCollection to get access to CollectionChanged event
    /// </summary>
    public class CustomSortDescriptionCollection : SortDescriptionCollection {

        /// <summary>
        /// Occurs when collection is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler MyCollectionChanged {
            add {
                this.CollectionChanged += value;
            }
            remove {
                this.CollectionChanged -= value;
            }
        }
    }
}
