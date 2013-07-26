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
			ViewModel.ItemSelectedEvent += (sender,args) => {
				this.NavigationController.PushViewController(new AccountDetailsViewController(args.Data),true);
			};
			TableView.Source = Source;
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, (sender,args) => {
				this.NavigationController.PushViewController(new AccountDetailsViewController(new Account()){
					Title = "Add Account",
				},true);
			});

		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			ViewModel.Refresh ();
		}
	}
}

