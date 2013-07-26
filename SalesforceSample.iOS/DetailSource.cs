using System.Json;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SalesforceSample.iOS
{
	public class DetailSource : UITableViewSource
	{
		JsonValue data;
		readonly DetailViewController controller;

		public JsonValue Data
		{
			get { return data; }
			set
			{
				if (data == value)
					return;
				data = value;
				controller.TableView.ReloadData ();
			}
		}

		public DetailSource (DetailViewController controller)
		{
			this.controller = controller;
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return 5;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = new UITableViewCell (UITableViewCellStyle.Value1, null);
			var items = Data.ToString ();

			switch (indexPath.Row) {
			case 0:
				cell.TextLabel.Text = "Name";
				cell.DetailTextLabel.Text = Data["Name"];
				break;
			case 1:
				cell.TextLabel.Text = "Industry";
				cell.DetailTextLabel.Text = Data["Industry"];
				break;
			case 2:
				cell.TextLabel.Text = "Phone";
				cell.DetailTextLabel.Text = Data["Phone"];
				break;
			case 3:
				cell.TextLabel.Text = "Website";
				cell.DetailTextLabel.Text = Data["Website"];
				break;
			case 4:
				cell.TextLabel.Text = "Account Number";
				cell.DetailTextLabel.Text = Data["AccountNumber"];
				break;
			}

			return cell;
		}
	}
}