using System.Collections.Generic;
using System.Json;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Salesforce;
using System;

namespace SalesforceSample.iOS
{
	public class DataSource : UITableViewSource
	{
		static readonly NSString CellIdentifier = new NSString ("DataSourceCell");
		List<JsonValue> objects = new List<JsonValue> ();
		readonly RootViewController controller;

		public DataSource (RootViewController controller)
		{
			this.controller = controller;
		}

		public List<JsonValue> Objects
		{
			get { return objects; }
			set
			{
				objects = value;
				controller.TableView.ReloadData ();
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

		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public async override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle,	NSIndexPath indexPath)
		{
			if (editingStyle == UITableViewCellEditingStyle.Delete) {
				var selected = controller.DataSource.Objects.ElementAtOrDefault (indexPath.Row) as JsonValue;
				var selectedObject = new SObject (selected as JsonObject);

				// Delete the row from the data source.
				var request = new DeleteRequest (selectedObject) {Resource = selectedObject};

				var rootController = controller as RootViewController;
				try 
				{
					rootController.SetLoadingState(true);
					var response = await controller.Client.ProcessAsync<DeleteRequest> (request);
					if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
						objects.Remove (selected);
				}
				catch (InsufficientRightsException) 
				{
					ShowInsuffientRightsMessage (tableView);
				}
				catch (DeleteFailedException ex) 
				{
					ShowDeleteFailedMessage (tableView, ex);
				}
				finally
				{
					rootController.SetLoadingState(false);
					tableView.ReloadData ();
				}
			} else if (editingStyle == UITableViewCellEditingStyle.Insert) {
				// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
			}
		}

		static void ShowDeleteFailedMessage (UITableView tableView, DeleteFailedException ex)
		{
			var message = string.Format ("Well, that didn't work for {0} reason{2}: {1}",
			                             ex.FailureReasons.Count (),
			                             string.Join ("; and ", ex.FailureReasons.Select (r => r.Message + ": " + string.Join (", ", r.RelatedIds))),
			                             ex.FailureReasons.Count () == 1 ? string.Empty : "s");
			var alertView = new UIAlertView ("Oops!", message, null, "Dismiss", null);
			alertView.Show ();
			tableView.ReloadData ();
			return;
		}

		static void ShowInsuffientRightsMessage (UITableView tableView)
		{
			var message = "Looks like either you don't have permission to delete that, or someone made it readonly.";
			var alertView = new UIAlertView ("Oops!", message, null, "Dismiss", null);
			alertView.Show ();
			tableView.ReloadData ();
			return;
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