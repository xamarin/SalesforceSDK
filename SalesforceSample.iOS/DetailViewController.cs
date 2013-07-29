using System;
using System.Drawing;
using System.Collections.Generic;
using System.Json;
using System.Net;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Salesforce;

namespace SalesforceSample.iOS
{
	public class DetailViewController : UITableViewController
	{
		JsonValue detailItem;
		DetailSource source;
		SalesforceClient client;

		public event EventHandler ItemUpdated;

		public DetailViewController (SalesforceClient client) : base (UITableViewStyle.Grouped)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Account Details", "Account Details");
			this.client = client;
		}

		public async Task<bool> SendUpdate ()
		{
			var account = new SObject { Id = detailItem["Id"], ResourceName = "Account" };
			account.Options.Add("Name", detailItem["Name"]);
			account.Options.Add("Industry", detailItem["Industry"]);
			account.Options.Add("Phone", detailItem["Phone"]);
			account.Options.Add("Website", detailItem["Website"]);
			account.Options.Add("AccountNumber", detailItem["AccountNumber"]);
			var request = new UpdateRequest {
				Resource = account
			};

			var response = await client.ProcessAsync (request);

			if (ItemUpdated != null)
				ItemUpdated (this, EventArgs.Empty);
			NavigationController.PopViewControllerAnimated (true);
			return response.StatusCode == HttpStatusCode.NoContent;
		}

		public void SetDetailItem (JsonValue newDetailItem)
		{
			if (detailItem != newDetailItem) {
				detailItem = newDetailItem;
				
				// Update the view
				ConfigureView (detailItem);
			}
		}

		void ConfigureView (JsonValue target)
		{
			if (TableView == null)
				return;

			if (TableView.Source == null)
				TableView.Source = source = new DetailSource (this);

			source.Data = target;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			ConfigureView (detailItem);
		}
	}
}

