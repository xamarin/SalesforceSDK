using System;
using System.Threading;
using System.Threading.Tasks;
using Salesforce;
using Xamarin.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Json;
using Xamarin.Auth;

namespace SalesForceSample
{
	public class SalesForceService
	{
		static SalesForceService shared;
		public static SalesForceService Shared
		{
			get{ return shared ?? (shared = new SalesForceSample.SalesForceService ());}
		}
		public event EventHandler<EventArgs<SalesforceClient>> ShowLoginScreen;
		public event EventHandler<EventArgs<ISalesforceUser>> LoggedIn;
		
		const string key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
		static Uri redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback");
		SalesforceClient Client;
		public ISalesforceUser CurrentUser {
			get{
				if (Client == null)
					return null;
				return Client.CurrentUser;
			}
		}

		public SalesForceService()
		{			
			Client = new SalesforceClient (key, redirectUrl);

			Client.AuthRequestCompleted += (sender, e) => {
				if (e.IsAuthenticated){
					Console.WriteLine("Auth success: " + e.Account.Username);
					Client.Save( e.Account);
					if(LoggedIn != null)
						LoggedIn(this, new EventArgs<ISalesforceUser>(e.Account));
				}
			};
		}


		public void Loaded()
		{
			if (CurrentUser == null && ShowLoginScreen != null)
				ShowLoginScreen (this, new EventArgs<SalesforceClient> (Client));
		}

		public Task<List<Account>> GetAccounts()
		{
			return Task.Factory.StartNew (() => {
				return getAccounts();
			});
		}

		List<Account> getAccounts()
		{
			if (Client.CurrentUser == null)
				return new List<Account> ();
			var request = new RestRequest {
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber,Industry, Phone FROM Account" }
			};

			var response = Client.Process<RestRequest> (request);
			var result = response.GetResponseText ();

			var results = System.Json.JsonValue.Parse(result);

			List<Account> accounts = results ["records"].OfType<JsonValue>().Select (x => new Account {
				Id = x["Id"],
				Name = x["Name"],
				AccountNumber = x["AccountNumber"],
				Industry = x["Industry"],
				//Phone = x["Phone"],
				//LastModifiedBy = x["LastModifiedBy"],
			}).ToList();

			return accounts;
		}

		public Task<bool> SaveAccount(Account account)
		{
			return Task.Factory.StartNew (() => {
				//TODO: save
				return true;
			});
		}

		public Task<bool> DeleteAccount(Account account)
		{
			return Task.Factory.StartNew (() => {
				//TODO: save
				return true;
			});
		}

	}
}

