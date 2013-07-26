using System;

namespace SalesForceSample
{
	public class AccountsViewModel : ViewModel<Account>
	{
		public AccountsViewModel ()
		{
		}
		public async void Refresh()
		{
			Items = await SalesForceService.Shared.GetAccounts ();
		}
	}
}

