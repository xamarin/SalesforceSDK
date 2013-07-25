using System;
using MonoTouch.UIKit;

namespace SalesForceSample.iOS
{
	public class AccountsViewController : UITableViewController
	{
		AccountsViewModel ViewModel;
		ViewModelDataSource<Account> Source;
		const string cellKey = "accountCell";
		public AccountsViewController ()
		{
			Source = new ViewModelDataSource<Account>(TableView) {
				Model = ViewModel =  new AccountsViewModel ()
			};
			Source.CellForItem += (tableView, item) => {
				var cell = tableView.DequeueReusableCell(cellKey) ?? new UITableViewCell(UITableViewCellStyle.Subtitle, cellKey);
				cell.TextLabel.Text = item.Name;
				cell.DetailTextLabel.Text = item.AccountNumber;
				return cell;
			};
			TableView.Source = Source;

		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			ViewModel.Refresh ();
		}
	}
}

