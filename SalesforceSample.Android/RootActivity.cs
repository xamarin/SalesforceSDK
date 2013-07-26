using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Salesforce;
using Xamarin.Auth;
using System.Json;
using System.Linq;

namespace SalesforceSample.Droid
{
	[Activity (Label = "Xamarin.Salesforce", MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		public SalesforceClient Client { get; private set; }

		ISalesforceUser Account { get; set; }

		const string key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
		Uri redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback"); // TODO: Move oauth redirect to constant or config


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Client = new SalesforceClient (key, redirectUrl);
			Client.AuthRequestCompleted += (sender, e) => OnAuthenticationCompleted (e);

			var users = Client.LoadUsers ();

			LoadAccounts ();
			if (!users.Any()) {
				var intent = Client.GetLoginInterface () as Intent;
			//	StartActivityForResult (intent, 42);
			} else {
				LoadAccounts ();
			}


			ListView.ItemClick += (sender,e) => {
				var t = ((DataAdapter)ListAdapter).GetItem(e.Position);

				Console.WriteLine("Clicked on " + t.ToString());

				var intent = new Intent();
				intent.SetClass(this, typeof(DetailActivity));
				intent.PutExtra("JsonItem", t.ToString());

				StartActivity(intent);
			};
		}

		void OnAuthenticationCompleted (AuthenticatorCompletedEventArgs e)
		{
			if (!e.IsAuthenticated) {
				// TODO: Handle failed login scenario by re-presenting login form with error
				throw new Exception ("Login failed and we don't handle that.");
			}

			LoadAccounts ();

			Account = e.Account;
			Client.Save (Account);
		}

		async void LoadAccounts ()
		{
//			SetLoadingState (true);
			var request = new ReadRequest () {
				// TODO : Add error handling for when this query asks for stuff that does not exist (mispell a field to reproduce)
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber, Phone, Website, Industry FROM Account" }
			};

			//Response response = await Client.ProcessAsync (request);
			//var result = response.GetResponseText ();

			var result = @"{""totalSize"":12,""done"":true,""records"":[{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000Js3UjAAJ""},""Id"":""001i000000Js3UjAAJ"",""Name"":""Tasty Food"",""AccountNumber"":""WEJH38787"",""Phone"":null,""Website"":null,""Industry"":null},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000Js3KqAAJ""},""Id"":""001i000000Js3KqAAJ"",""Name"":""Advanced Mechanics"",""AccountNumber"":""23KJ32KJ9"",""Phone"":null,""Website"":null,""Industry"":null},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0VAAT""},""Id"":""001i000000JEw0VAAT"",""Name"":""GenePoint"",""AccountNumber"":""CC978213"",""Phone"":""(650) 867-3450"",""Website"":""www.genepoint.com"",""Industry"":""Biotechnology""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0WAAT""},""Id"":""001i000000JEw0WAAT"",""Name"":""United Oil & Gas, UK"",""AccountNumber"":""CD355119-A"",""Phone"":""+44 191 4956203"",""Website"":""http://www.uos.com"",""Industry"":""Energy""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0XAAT""},""Id"":""001i000000JEw0XAAT"",""Name"":""United Oil & Gas, Singapore"",""AccountNumber"":""CD355120-B"",""Phone"":""(650) 450-8810"",""Website"":""http://www.uos.com"",""Industry"":""Energy""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0YAAT""},""Id"":""001i000000JEw0YAAT"",""Name"":""Edge Communications"",""AccountNumber"":""CD451796"",""Phone"":""(512) 757-6000"",""Website"":""http://edgecomm.com"",""Industry"":""Electronics""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0ZAAT""},""Id"":""001i000000JEw0ZAAT"",""Name"":""Burlington Textiles Corp of America"",""AccountNumber"":""CD656092"",""Phone"":""(336) 222-7000"",""Website"":""www.burlington.com"",""Industry"":""Apparel""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0cAAD""},""Id"":""001i000000JEw0cAAD"",""Name"":""Grand Hotels & Resorts Ltd"",""AccountNumber"":""CD439877"",""Phone"":""(312) 596-1000"",""Website"":""www.grandhotels.com"",""Industry"":""Hospitality""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0dAAD""},""Id"":""001i000000JEw0dAAD"",""Name"":""Express Logistics and Transport"",""AccountNumber"":""CC947211"",""Phone"":""(503) 421-7800"",""Website"":""www.expressl&t.net"",""Industry"":""Transportation""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0eAAD""},""Id"":""001i000000JEw0eAAD"",""Name"":""University of Arizona"",""AccountNumber"":""CD736025"",""Phone"":""(520) 773-9050"",""Website"":""www.universityofarizona.com"",""Industry"":""Education""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0fAAD""},""Id"":""001i000000JEw0fAAD"",""Name"":""United Oil & Gas Corp."",""AccountNumber"":""CD355118"",""Phone"":""(212) 842-5500"",""Website"":""http://www.uos.com"",""Industry"":""Energy""},{""attributes"":{""type"":""Account"",""url"":""/services/data/v28.0/sobjects/Account/001i000000JEw0gAAD""},""Id"":""001i000000JEw0gAAD"",""Name"":""sForce"",""AccountNumber"":null,""Phone"":""(415) 901-7000"",""Website"":""www.sforce.com"",""Industry"":null}]}";
			var jsonValue = JsonValue.Parse(result);

			if (jsonValue == null) {
				throw new Exception("Could not parse Json data");
			}

			var results = jsonValue["records"];
			
			ListAdapter = new DataAdapter (this, results.OfType<JsonValue> ().ToList ());

//			SetLoadingState (false);
		}
	}
}
