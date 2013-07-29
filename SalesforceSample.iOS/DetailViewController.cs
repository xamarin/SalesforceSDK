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
	public class AddViewController : DetailViewController
	{
		public AddViewController (SalesforceClient client) : base (client)
		{
			Title = NSBundle.MainBundle.LocalizedString ("New Account", "New Account");
		}
	}
	public class DetailViewController : UITableViewController
	{
		AccountObject detailItem;
		DetailSource source;
		SalesforceClient client;

		public event EventHandler<AccountObject> ItemUpdated;

		public DetailViewController (SalesforceClient client) : base (UITableViewStyle.Grouped)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Account Details", "Account Details");
			this.client = client;
		}

		public void SendUpdate ()
		{
			if (ItemUpdated != null)
				ItemUpdated (this, detailItem);
		}

		public void SetDetailItem (AccountObject newDetailItem)
		{
			if (detailItem != newDetailItem) {
				detailItem = newDetailItem;
				
				// Update the view
				ConfigureView (detailItem);
			}
		}

		void ConfigureView (AccountObject target)
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

