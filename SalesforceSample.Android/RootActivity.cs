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
	[Activity (Label = "Xamarin.Salesforce", MainLauncher = true), IntentFilter(new String[]{"com.sample.salesforce"})]
	public class RootActivity : ListActivity
	{
		public static SalesforceClient Client { get; private set; }

		ISalesforceUser Account { get; set; }

		const string key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
		Uri redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback"); // TODO: Move oauth redirect to constant or config

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Client = new SalesforceClient (key, "5754078534436456018", redirectUrl);
			Client.AuthenticationComplete += (sender, e) => OnAuthenticationCompleted (e);

			var users = Client.LoadUsers ();

			if (!users.Any()) {
				var intent = Client.GetLoginInterface () as Intent;
				StartActivityForResult (intent, 42);
			} else {
				// OnResume
				LoadAccounts ();
			}

			ListView.ItemClick += (sender,e) => {
				var t = ((DataAdapter)ListAdapter).GetItem(e.Position);

				Debug.WriteLine("Clicked on " + t.ToString());

				var intent = new Intent();
				intent.SetClass(this, typeof(DetailActivity));
				intent.PutExtra("JsonItem", t.ToString());

				StartActivity(intent);
			};
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			//LoadAccounts ();
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
			var request = new ReadRequest () {
				// TODO : Add error handling for when this query asks for stuff that does not exist (mispell a field to reproduce)
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber, Phone, Website, Industry FROM Account" }
			};

			Response response = await Client.ProcessAsync (request);
			var result = response.GetResponseText ();

			var jsonValue = JsonValue.Parse(result);

			if (jsonValue == null) {
				throw new Exception("Could not parse Json data");
			}

			var results = jsonValue["records"];

			var resultRecords = results.OfType<JsonValue> ().ToList ();

			Debug.WriteLine ("records: {0}", resultRecords.Count);
			ListAdapter = new DataAdapter (this, resultRecords);

//			SetLoadingState (false);
		}



		/// <summary>shortcut back to the main screen</summary>
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.Add, menu);
			return true;
		}

		/// <summary>shortcut back to the main screen</summary>
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.add) {
				// HACK: populate blank fields with blank JSON
				var extra = @"{""type"": ""Account"", ""Id"": """", ""Name"": """", ""AccountNumber"": """", ""Phone"": """", ""Website"": """", ""Industry"": """"}";

				var intent = new Intent();
				intent.SetClass(this, typeof(DetailActivity));
				intent.PutExtra("JsonItem", extra);

				StartActivity(intent);
			}
			return true;
		}
	}
}
