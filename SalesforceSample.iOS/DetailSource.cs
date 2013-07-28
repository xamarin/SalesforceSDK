using System;
using System.Collections.Generic;
using System.Drawing;
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
			return 2;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return section == 0 ? 5 : 1;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);
			if (indexPath.Section == 1) {
				controller.SendUpdate ();
			}
		}

		bool OnShouldReturn (UITextField textField)
		{
			if (textField.IsFirstResponder)
				textField.ResignFirstResponder ();
			return true;
		}

		List<UIView> cellsList = new List<UIView>();
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Section == 1) {
				var buttonCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				buttonCell.TextLabel.TextAlignment = UITextAlignment.Center;
				buttonCell.TextLabel.Text = "Submit Changes";
				return buttonCell;
			}
			var cell = new UITableViewCell (UITableViewCellStyle.Value1, null) {
				SelectionStyle = UITableViewCellSelectionStyle.None
			};

			var textField = new UITextField (new RectangleF(110, 10, 185, 30)) {
				AdjustsFontSizeToFitWidth = true,
				TextColor = cell.DetailTextLabel.TextColor,
				TextAlignment = UITextAlignment.Left,
				Tag = 0,
				ClearButtonMode = UITextFieldViewMode.Never,
				Enabled = true,
				BackgroundColor = UIColor.Clear,
				ShouldReturn = OnShouldReturn
			};

			switch (indexPath.Row) {
			case 0:
				cell.TextLabel.Text = "Name";
				textField.Text = Data["Name"];
				break;
			case 1:
				cell.TextLabel.Text = "Industry";
				textField.Text = Data["Industry"];
				break;
			case 2:
				cell.TextLabel.Text = "Phone";
				textField.Text = Data["Phone"];
				break;
			case 3:
				cell.TextLabel.Text = "Website";
				textField.Text = Data["Website"];
				break;
			case 4:
				cell.TextLabel.Text = "Account";
				textField.Text = Data["AccountNumber"];
				break;
			}

			cellsList.Add (cell);
			cellsList.Add (textField);
			cell.ContentView.Add (textField);
			return cell;
		}
	}
}