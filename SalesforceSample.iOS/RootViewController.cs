using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using MonoTouch.CoreAnimation;

namespace SalesforceSample.iOS
{
	public partial class RootViewController : UITableViewController
	{
		DataSource dataSource;

		UIBarButtonItem ActivityItem {
			get;
			set;
		}

		public RootViewController () : base ("RootViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Master", "Master");

			var item = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White);
			item.Hidden = false;
			item.StartAnimating();

			ActivityItem = new UIBarButtonItem (item);

			// Custom initialization
		}

		public DetailViewController DetailViewController {
			get;
			set;
		}

		public UIViewController LoginController { get; protected set; }

		void AddNewItem (object sender, EventArgs args)
		{
			try {
				LoginController = Client.GetLoginInterface () as UIViewController;
				PresentViewController(LoginController, true, null);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		SalesforceClient Client;
		ISalesforceUser Account { get; set; }

		LoadingViewController LoadingController {
			get;
			set;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.
			NavigationItem.RightBarButtonItem = EditButtonItem;

			var addButton = new UIBarButtonItem (UIBarButtonSystemItem.Add, AddNewItem);
			NavigationItem.LeftBarButtonItem = addButton;

			TableView.Source = dataSource = new DataSource (this);

			var key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
			var callback = new Uri ("https://login.salesforce.com/services/oauth2/success/"); // TODO: Move oauth redirect to constant or config

			//LoadingController = new LoadingViewController ();

			Client = new SalesforceClient (key, callback);

			Client.AuthRequestCompleted += (sender, e) => {
				if (e.IsAuthenticated){
					// Invoke completion handler.
					Console.WriteLine("Auth success: " + e.Account.Username);
				}

				DismissViewController(true, new NSAction(
					()=>
					{
						NavigationItem.RightBarButtonItem = null;
						//PresentViewController(LoadingController, false, null); 
					}));

				Account = e.Account;
				Client.Save(Account);
			};

			var accounts = Client.LoadUsers ();
			if (accounts == null || accounts.Count () == 0)
			{
				var loginController = Client.GetLoginInterface () as UIViewController;
				PresentViewController (loginController, true, null);
			}
			else
			{
				ShowLoadingState (accounts, LoadingController);
				LoadAccounts (accounts.FirstOrDefault());
			}
		}

		void LoadAccounts (ISalesforceUser account)
		{
			Console.WriteLine (account);

			var request = new RestRequest {
				Resource = new SObject()
			};

			var response = Client.Process<RestRequest> (request);
			var result = response.GetResponseText ();

			Console.WriteLine(result);

			var versions = System.Json.JsonValue.Parse (result);
			foreach(var v in versions)
			{
				Console.WriteLine (v);
			}
		}

		public void ShowLoadingState(IEnumerable<ISalesforceUser> accounts, UIViewController controller)
		{
			// TODO: Show account picker, or save selected index to settings.
			Client.CurrentUser = accounts.FirstOrDefault();
			//this.View.InsertSubviewAbove (controller.View, View.Subviews.Last ());
			NavigationItem.RightBarButtonItem = ActivityItem;
		}

		class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString ("DataSourceCell");
			List<object> objects = new List<object> ();
			RootViewController controller;

			public DataSource (RootViewController controller)
			{
				this.controller = controller;
			}

			public IList<object> Objects {
				get { return objects; }
			}
			// Customize the number of sections in the table view.
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return objects.Count;
			}
			// Customize the appearance of table view cells.
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (CellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, CellIdentifier);
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				}

				cell.TextLabel.Text = objects [indexPath.Row].ToString ();

				return cell;
			}

			public override bool CanEditRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				// Return false if you do not want the specified item to be editable.
				return true;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete) {
					// Delete the row from the data source.
					objects.RemoveAt (indexPath.Row);
					controller.TableView.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
				} else if (editingStyle == UITableViewCellEditingStyle.Insert) {
					// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
				}
			}
			/*
			// Override to support rearranging the table view.
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
			}
			*/
			/*
			// Override to support conditional rearranging of the table view.
			public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
			{
				// Return false if you do not want the item to be re-orderable.
				return true;
			}
			*/
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (controller.DetailViewController == null)
					controller.DetailViewController = new DetailViewController ();

				controller.DetailViewController.SetDetailItem (objects [indexPath.Row]);

				// Pass the selected object to the new view controller.
				controller.NavigationController.PushViewController (controller.DetailViewController, true);
			}
		}
	}
}
