using System;
using System.Collections.Generic;

namespace SalesForceSample
{
	public class ViewModel <T>
	{
		public event EventHandler ItemsChanged;
		public event EventHandler<EventArgs<T>> ItemSelectedEvent;

		protected List<T> items = new List<T> ();

		public List<T> Items {
			get {
				return items;
			}
			set {
				items = value;
				if(ItemsChanged != null)
					ItemsChanged(this,EventArgs.Empty);
			}
		}

		public ViewModel ()
		{

		}

		public int Rows ()
		{
			return GetRows ();
		}

		protected virtual int GetRows ()
		{
			return items.Count;
		}

		public void ItemSelected (T item)
		{
			DidSelectItem (item);
			
			if (ItemSelectedEvent != null)
				ItemSelectedEvent (this,new EventArgs<T>(item));

		}

		protected virtual void DidSelectItem (T item)
		{

		}

	}
}

