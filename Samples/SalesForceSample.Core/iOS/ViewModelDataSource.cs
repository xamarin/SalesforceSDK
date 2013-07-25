using System;
using MonoTouch.UIKit;
namespace SalesForceSample
{
	public class ViewModelDataSource<T> : UITableViewSource
	{
		UITableView tableView;

		public ViewModelDataSource (UITableView tableView)
		{
			this.tableView = tableView;
		}

		
		public delegate UITableViewCell GetCellEventHandler (UITableView tableView, T item);
		// the event
		public event GetCellEventHandler CellForItem;

		ViewModel<T> model;
		
		public ViewModel<T> Model {
			get {
				return model;
			}
			set {
				if (model != null)
					removeModelEvents ();
				model = value;
				addModelEvents ();
			}
		}
		
		void removeModelEvents ()
		{
			model.ItemsChanged -= HandleItemsChanged;
		}
		
		void addModelEvents ()
		{
			model.ItemsChanged += HandleItemsChanged;
		}
		
		void HandleItemsChanged (object sender, EventArgs e)
		{
			tableView.ReloadData ();
		}


		#region implemented abstract members of UITableViewSource

		public override int RowsInSection (UITableView tableview, int section)
		{
			if (model == null)
				return 0;
			return model.Rows ();
		}

		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			if (CellForItem != null)
				return CellForItem (tableView, model.Items [indexPath.Row]);

			throw new Exception("Event CellForItem is required for ViewModelDatasource");
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			model.ItemSelected (model.Items [indexPath.Row]);
		}
		#endregion
	}
}

