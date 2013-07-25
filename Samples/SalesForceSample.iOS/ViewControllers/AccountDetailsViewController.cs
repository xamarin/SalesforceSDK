using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using BigTed;

namespace SalesForceSample.iOS
{
	public class AccountDetailsViewController : DialogViewController
	{
		EntryElement name;
		EntryElement industry;
		EntryElement phoneNumber;
		StringElement owner;
		StringElement lastModified;
		StringElement save;
		public AccountDetailsViewController (Account account) : base (UITableViewStyle.Grouped, null,true)
		{
			Title = string.IsNullOrEmpty(account.Id)  ? "Add Account" : "Edit Account";

			if (!string.IsNullOrEmpty (account.Id))
				this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Trash, (sender,args) => {
					Delete(account);
				});
			name = new EntryElement ("Name", "", account.Name);
			name.Changed += (object sender, EventArgs e) => {
				account.Name = name.Value;
			};

			industry = new EntryElement ("Industry", "", account.Industry);
			industry.Changed += (object sender, EventArgs e) =>{
				account.Industry = industry.Value;
			};

			phoneNumber = new EntryElement ("Phone", "", account.Phone);
			phoneNumber.Changed += (object sender, EventArgs e) => {
				account.Phone = phoneNumber.Value;
			};

			owner = new StringElement ("Owner", account.AccountOwner);
			lastModified = new StringElement ("Last Modified", account.LastModifiedBy);

			save = new StringElement (string.IsNullOrEmpty(account.Id) ? "Add" : "Save", () => {
				Save(account);
			});

			Root = new RootElement (Title) {
				new Section(){
					name,
					industry,
					phoneNumber,
					owner,
					lastModified,
				},
				new Section(){
					save
				}
			};
		}

		public async void Save(Account account)
		{	
			BTProgressHUD.Show(); 	
			bool success = false;
			try{
				success = await SalesForceService.Shared.SaveAccount(account);
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
				new UIAlertView ("Error saving", ex.Message, null, "Ok").Show ();
			}
			BTProgressHUD.Dismiss ();
			if (success)
				this.NavigationController.PopViewControllerAnimated (true);

		}

		public async void Delete(Account account)
		{
			BTProgressHUD.Show(); 	
			bool success = false;
			try{
				success = await SalesForceService.Shared.DeleteAccount(account);
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
				new UIAlertView ("Error deleting", ex.Message, null, "Ok").Show ();
			}
			BTProgressHUD.Dismiss ();
			if (success)
				this.NavigationController.PopViewControllerAnimated (true);
		}
	}
}

