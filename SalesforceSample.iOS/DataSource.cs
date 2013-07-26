using System.Collections.Generic;
using System.Json;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Salesforce;

namespace SalesforceSample.iOS
{
	public class DataSource : UITableViewSource
	{
		static readonly NSString CellIdentifier = new NSString ("DataSourceCell");
		List<object> objects = new List<object> ();
		readonly RootViewController controller;

		public DataSource (RootViewController controller)
		{
			this.controller = controller;
		}

		public List<object> Objects
		{
			get { return objects; }
			set
			{
				objects = value;
				this.controller.TableView.ReloadData ();
			}
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return objects.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (CellIdentifier);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, CellIdentifier) {
					Accessory = UITableViewCellAccessory.DisclosureIndicator
				};
			}

			var o = (JsonObject) objects[indexPath.Row];
			cell.TextLabel.Text = o["Name"];
			cell.DetailTextLabel.Text = o["AccountNumber"];
			return cell;
		}

		public override bool CanEditRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			return true;
		}

		public override async void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle,
			NSIndexPath indexPath)
		{
			if (editingStyle == UITableViewCellEditingStyle.Delete) {

				var selected = controller.DataSource.Objects.ElementAtOrDefault (indexPath.Row) as JsonValue;
				var selectedObject = new SObject (selected as JsonObject);
				// Delete the row from the data source.
				var request = new DeleteRequest (selectedObject) {Resource = selectedObject};

				await controller.Client.ProcessAsync (request);
				((DataSource) tableView.Source).Objects.Remove (selectedObject);
				tableView.ReloadData ();

			} else if (editingStyle == UITableViewCellEditingStyle.Insert) {
				// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
			}
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			if (controller.DetailViewController == null)
				controller.DetailViewController = new DetailViewController ();

			controller.DetailViewController.SetDetailItem (objects[indexPath.Row]);

			controller.NavigationController.PushViewController (controller.DetailViewController, true);
		}
	}
}