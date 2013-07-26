using System;
using System.Drawing;
using System.Collections.Generic;
using System.Json;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SalesforceSample.iOS
{
	public class DetailViewController : UITableViewController
	{
		JsonValue detailItem;
		DetailSource source;

		public DetailViewController () : base (UITableViewStyle.Grouped)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Account Details", "Account Details");
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

