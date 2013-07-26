using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using MonoTouch.CoreAnimation;
using System.Json;

namespace SalesforceSample.iOS
{
	public sealed partial class RootViewController : UITableViewController
	{
		DataSource dataSource;
		SalesforceClient client;

		ISalesforceUser Account { get; set; }

		UIBarButtonItem ActivityItem { get; set; }

		DetailViewController DetailViewController { get; set; }

		UIViewController LoginController { get; set; }

		public RootViewController () : base ("RootViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Master", "Master");

			var item = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White) {Hidden = false};
			item.StartAnimating();

			ActivityItem = new UIBarButtonItem (item);
		}

		void AddNewItem (object sender, EventArgs args)
		{
			try {
				LoginController = client.GetLoginInterface () as UIViewController;
				PresentViewController(LoginController, true, null);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			NavigationItem.RightBarButtonItem = EditButtonItem;
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, AddNewItem);

			TableView.Source = dataSource = new DataSource (this);

			const string key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
			var redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback"); // TODO: Move oauth redirect to constant or config

			client = new SalesforceClient (key, redirectUrl);
			client.AuthRequestCompleted += (sender, e) => {
				if (e.IsAuthenticated){
					// TODO: Transition to regular application UI.
					Console.WriteLine("Auth success: " + e.Account.Username);
				}

				DismissViewController(true, () => {
					NavigationItem.RightBarButtonItem = null;
					SetLoadingState (true);
					LoadAccounts ();
				});

				Account = e.Account;
				client.Save(Account);
			};

			var users = client.LoadUsers ();
			
			if (!users.Any()) {
				var loginController = client.GetLoginInterface () as UIViewController;
				PresentViewController (loginController, true, null);
			} else {
				SetLoadingState (true);
				LoadAccounts ();
			}
		}

		async void LoadAccounts ()
		{
			var request = new ReadRequest {
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber FROM Account" }
			};

			Response response = await client.ProcessAsync (request);
			var result = response.GetResponseText ();
			var jsonValue = JsonValue.Parse(result);

			if (jsonValue == null) {
				throw new Exception("Could not parse Json data");
			}

			var results = jsonValue["records"];

			dataSource.Objects = results.OfType<object>().ToList();
			SetLoadingState (false);
		}

		static void SetLoadingState(bool loading)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = loading;
		}

		class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString ("DataSourceCell");
			List<object> objects = new List<object> ();
			readonly RootViewController controller;

			public DataSource (RootViewController controller)
			{
				this.controller = controller;
			}

			public List<object> Objects {
				get { return objects; }
				set { objects = value; this.controller.TableView.ReloadData ();}
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
					cell = new UITableViewCell (UITableViewCellStyle.Subtitle, CellIdentifier);
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				}

				var o = (JsonObject)objects [indexPath.Row];
				cell.TextLabel.Text = o["Name"];
				cell.DetailTextLabel.Text = o["AccountNumber"];
				return cell;
			}

			public override bool CanEditRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				return true;
			}

			public async override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete) {

					var selected = controller.dataSource.Objects.ElementAtOrDefault (indexPath.Row) as JsonValue;
					var selectedObject = new SObject (selected as JsonObject);
					// Delete the row from the data source.
					var request = new DeleteRequest (selectedObject) {Resource = selectedObject};

					await controller.client.ProcessAsync (request);
					((DataSource)tableView.Source).Objects.Remove (selectedObject);
					tableView.ReloadData ();

				} else if (editingStyle == UITableViewCellEditingStyle.Insert) {
					// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
				}
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (controller.DetailViewController == null)
					controller.DetailViewController = new DetailViewController ();

				controller.DetailViewController.SetDetailItem (objects [indexPath.Row]);

				controller.NavigationController.PushViewController (controller.DetailViewController, true);
			}
		}
	}
}
