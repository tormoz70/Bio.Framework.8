using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Specialized;
using Bio.Helpers.Common;


namespace Bio.Framework.Client.SL {
  /// <summary>
  /// Implements ICollectionView to provide hook to plugin custom sorting
  /// </summary>
  /// <typeparam name="T">Type of Item in the collection</typeparam>
  public class SortableCollectionView<T> : ObservableCollection<T>, ICollectionView {

    /// <summary>
    /// Initializes a new instance of the <see cref="SortableCollectionView&lt;T&gt;"/> class.
    /// </summary>
    public SortableCollectionView(EventHandler<RefreshEventArgs> onRefreshEveHandler) {
      this.CurrentItem = null;
      this.CurrentPosition = -1;
      if (onRefreshEveHandler != null)
        this.OnRefresh += onRefreshEveHandler;
    }
    public SortableCollectionView() : this(null) { }

    /// <summary>
    /// Inserts an item into the collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    protected override void InsertItem(int index, T item) {
      //if (null != this.Filter && !this.Filter(item)) {
      //    return;
      //}
      base.InsertItem(index, item);
      if (0 == index || null == this.CurrentItem) {
        CurrentItem = item;
        CurrentPosition = index;
      }
    }

    /// <summary>
    /// Gets the item at.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>item if found; otherwise, null</returns>
    public virtual object GetItemAt(int index) {
      if ((index >= 0) && (index < this.Count)) {
        return this[index];
      }
      return null;
    }

    #region ICollectionView Members

    /// <summary>
    /// Gets a value that indicates whether this view supports filtering by way of the <see cref="P:System.ComponentModel.ICollectionView.Filter"/> property.
    /// </summary>
    /// <value></value>
    /// <returns>true if this view supports filtering; otherwise, false.
    /// </returns>
    public bool CanFilter {
      get {
        //return true;
        return false;
      }
    }

    /// <summary>
    /// Gets a value that indicates whether this view supports grouping by way of the <see cref="P:System.ComponentModel.ICollectionView.GroupDescriptions"/> property.
    /// </summary>
    /// <value></value>
    /// <returns>true if this view supports grouping; otherwise, false.
    /// </returns>
    public bool CanGroup {
      get { return false; }
    }

    /// <summary>
    /// Gets a value that indicates whether this view supports sorting by way of the <see cref="P:System.ComponentModel.ICollectionView.SortDescriptions"/> property.
    /// </summary>
    /// <value></value>
    /// <returns>true if this view supports sorting; otherwise, false.
    /// </returns>
    public bool CanSort {
      get { return true; }
    }

    /// <summary>
    /// Indicates whether the specified item belongs to this collection view.
    /// </summary>
    /// <param name="item">The object to check.</param>
    /// <returns>
    /// true if the item belongs to this collection view; otherwise, false.
    /// </returns>
    public bool Contains(object item) {
      if (!IsValidType(item)) {
        return false;
      }
      return this.Contains((T)item);
    }

    /// <summary>
    /// Determines whether the specified item is of valid type
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// 	<c>true</c> if the specified item is of valid type; otherwise, <c>false</c>.
    /// </returns>
    private bool IsValidType(object item) {
      return item is T;
    }

    private CultureInfo _culture;

    /// <summary>
    /// Gets or sets the cultural information for any operations of the view that may differ by culture, such as sorting.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The culture information to use during culture-sensitive operations.
    /// </returns>
    public CultureInfo Culture {
      get {
        return this._culture;
      }
      set {
        if (Equals(this._culture, value)) {
          this._culture = value;
          this.OnPropertyChanged(new PropertyChangedEventArgs("Culture"));
        }

      }
    }

    /// <summary>
    /// Occurs after the current item has been changed.
    /// </summary>
    public event EventHandler CurrentChanged;

    /// <summary>
    /// Occurs before the current item changes.
    /// </summary>
    public event CurrentChangingEventHandler CurrentChanging;

    /// <summary>
    /// Gets the current item in the view.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The current item in the view or null if there is no current item.
    /// </returns>
    public object CurrentItem { get; private set; }

    /// <summary>
    /// Gets the ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.
    /// </returns>
    public int CurrentPosition { get; private set; }

    public IDisposable DeferRefresh() {
      //throw new NotImplementedException();
      return null;
    }

    public Predicate<object> Filter { get; set; }

    public ObservableCollection<GroupDescription> GroupDescriptions {
      get { throw new NotImplementedException(); }
    }

    public ReadOnlyObservableCollection<object> Groups {
      get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the end of the collection.
    /// </summary>
    /// <value></value>
    /// <returns>true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the end of the collection; otherwise, false.
    /// </returns>
    public bool IsCurrentAfterLast {
      get {
        if (!this.IsEmpty) {
          return (this.CurrentPosition >= this.Count);
        }
        return true;
      }
    }

    /// <summary>
    /// Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the start of the collection.
    /// </summary>
    /// <value></value>
    /// <returns>true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> of the view is beyond the start of the collection; otherwise, false.
    /// </returns>
    public bool IsCurrentBeforeFirst {
      get {
        if (!this.IsEmpty) {
          return (this.CurrentPosition < 0);
        }
        return true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is current in sync.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is current in sync; otherwise, <c>false</c>.
    /// </value>
    protected bool IsCurrentInSync {
      get {
        if (this.IsCurrentInView) {
          return (this.GetItemAt(this.CurrentPosition) == this.CurrentItem);
        }
        return (this.CurrentItem == null);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is current in view.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is current in view; otherwise, <c>false</c>.
    /// </value>
    private bool IsCurrentInView {
      get {
        return ((0 <= this.CurrentPosition) && (this.CurrentPosition < this.Count));
      }
    }

    /// <summary>
    /// Gets a value that indicates whether the view is empty.
    /// </summary>
    /// <value></value>
    /// <returns>true if the view is empty; otherwise, false.
    /// </returns>
    public bool IsEmpty {
      get {
        return (this.Count == 0);
      }
    }

    /// <summary>
    /// Sets the specified item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
    /// </summary>
    /// <param name="item">The item to set as the current item.</param>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentTo(object item) {
      if (!IsValidType(item)) {
        return false;
      }
      if (Equals(this.CurrentItem, item) && ((item != null) || this.IsCurrentInView)) {
        return this.IsCurrentInView;
      }
      int index = this.IndexOf((T)item);
      return this.MoveCurrentToPosition(index);
    }

    /// <summary>
    /// Sets the first item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
    /// </summary>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentToFirst() {
      return this.MoveCurrentToPosition(0);
    }

    /// <summary>
    /// Sets the last item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
    /// </summary>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentToLast() {
      return this.MoveCurrentToPosition(this.Count - 1);
    }

    /// <summary>
    /// Sets the item after the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
    /// </summary>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentToNext() {
      return ((this.CurrentPosition < this.Count) && this.MoveCurrentToPosition(this.CurrentPosition + 1));
    }

    /// <summary>
    /// Sets the item before the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view to the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/>.
    /// </summary>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentToPrevious() {
      return ((this.CurrentPosition >= 0) && this.MoveCurrentToPosition(this.CurrentPosition - 1));
    }

    /// <summary>
    /// Sets the item at the specified index to be the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> in the view.
    /// </summary>
    /// <param name="position">The index to set the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> to.</param>
    /// <returns>
    /// true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem"/> is an item in the view; otherwise, false.
    /// </returns>
    public bool MoveCurrentToPosition(int position) {
      if ((position < -1) || (position > this.Count)) {
        throw new ArgumentOutOfRangeException("position");
      }
      if (((position != this.CurrentPosition) || !this.IsCurrentInSync) && this.isOkToChangeCurrent()) {
        bool isCurrentAfterLast = this.IsCurrentAfterLast;
        bool isCurrentBeforeFirst = this.IsCurrentBeforeFirst;
        _changeCurrentToPosition(position);
        OnCurrentChanged();
        if (this.IsCurrentAfterLast != isCurrentAfterLast) {
          this._doOnPropertyChanged("IsCurrentAfterLast");
        }
        if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst) {
          this._doOnPropertyChanged("IsCurrentBeforeFirst");
        }
        this._doOnPropertyChanged("CurrentPosition");
        this._doOnPropertyChanged("CurrentItem");
      }
      return this.IsCurrentInView;
    }

    /// <summary>
    /// Changes the current to position.
    /// </summary>
    /// <param name="position">The position.</param>
    private void _changeCurrentToPosition(int position) {
      if (position < 0) {
        this.CurrentItem = null;
        this.CurrentPosition = -1;
      } else if (position >= this.Count) {
        this.CurrentItem = null;
        this.CurrentPosition = this.Count;
      } else {
        this.CurrentItem = this[position];
        this.CurrentPosition = position;
      }
    }

    /// <summary>
    /// Determines whether it is OK to change current item.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if is OK to change current item; otherwise, <c>false</c>.
    /// </returns>
    protected bool isOkToChangeCurrent() {
      var args = new CurrentChangingEventArgs();
      this.OnCurrentChanging(args);
      return !args.Cancel;
    }

    /// <summary>
    /// Called when current item has changed.
    /// </summary>
    protected virtual void OnCurrentChanged() {
      if (this.CurrentChanged != null) {
        this.CurrentChanged(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Raises the <see><cref>E:CurrentChanging</cref></see> event.
    /// </summary>
    /// <param name="args">The <see cref="System.ComponentModel.CurrentChangingEventArgs"/> instance containing the event data.</param>
    protected virtual void OnCurrentChanging(CurrentChangingEventArgs args) {
      if (args == null) {
        throw new ArgumentNullException("args");
      }
      if (this.CurrentChanging != null) {
        this.CurrentChanging(this, args);
      }
    }

    /// <summary>
    /// Called when the current item is changing.
    /// </summary>
    protected void OnCurrentChanging() {
      this.CurrentPosition = -1;
      this.OnCurrentChanging(new CurrentChangingEventArgs(false));
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    protected override void ClearItems() {
      OnCurrentChanging();
      base.ClearItems();
    }

    /// <summary>
    /// Called when a property has changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    private void _doOnPropertyChanged(string propertyName) {
      this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Occurs when data needs to be refreshed.
    /// </summary>
    public event EventHandler<RefreshEventArgs> OnRefresh;

    /// <summary>
    /// Recreates the view, by firing OnRefresh event.
    /// </summary>
    public void Refresh() {
      // sort and refersh
      if (null != OnRefresh) {
        delayedStarter.Do(1000, () => this.OnRefresh(this, new RefreshEventArgs { SortDescriptions = SortDescriptions }));
      }
      //this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private CustomSortDescriptionCollection _sort;

    /// <summary>
    /// Gets a collection of <see cref="T:System.ComponentModel.SortDescription"/> instances that describe how the items in the collection are sorted in the view.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// A collection of values that describe how the items in the collection are sorted in the view.
    /// </returns>
    public SortDescriptionCollection SortDescriptions {
      get {
        if (this._sort == null) {
          this._setSortDescriptions(new CustomSortDescriptionCollection());
        }
        return this._sort;
      }
    }

    /// <summary>
    /// Sets the sort descriptions.
    /// </summary>
    /// <param name="descriptions">The descriptions.</param>
    private void _setSortDescriptions(CustomSortDescriptionCollection descriptions) {
      if (this._sort != null) {
        this._sort.MyCollectionChanged -= this._sortDescriptionsChanged;
      }
      this._sort = descriptions;
      if (this._sort != null) {
        this._sort.MyCollectionChanged += this._sortDescriptionsChanged;
      }
    }

    /// <summary>
    /// Sorts the descriptions changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void _sortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e) {
      if (e.Action == NotifyCollectionChangedAction.Remove && e.NewStartingIndex == -1 && SortDescriptions.Count > 0) {
        return;
      }
      if (
          ((e.Action != NotifyCollectionChangedAction.Reset) || (e.NewItems != null))
          || (((e.NewStartingIndex != -1) || (e.OldItems != null)) || (e.OldStartingIndex != -1))
          ) {
        this.Refresh();
        //this.Items.
      }
    }

    /// <summary>
    /// Gets the underlying collection.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The underlying collection.
    /// </returns>
    public System.Collections.IEnumerable SourceCollection {
      get {
        return this;
      }
    }

    #endregion


  }
}
