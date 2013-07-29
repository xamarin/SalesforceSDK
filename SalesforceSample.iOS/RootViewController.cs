using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using System.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;

namespace SalesforceSample.iOS
{
	public sealed partial class RootViewController : UITableViewController
	{
		public DataSource DataSource { get; private set; }
		public SalesforceClient Client { get; private set; }
		public DetailViewController DetailViewController { get; set; }

		AddViewController AddAccountController { get; set; }

		public RootViewController () : base ("RootViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Accounts", "Accounts");
		}

		void AddNewItem ()
		{
			AddAccountController = new AddViewController ();
			AddAccountController.ItemUpdated += OnItemAdded;
			PresentViewController (AddAccountController, true, null);
		}

		async void OnItemAdded (object sender, AccountObject account)
		{
			// Create salesforce creation request from generated account object
			var createRequest = new CreateRequest (account);
			var result = await Client.ProcessAsync (createRequest).ConfigureAwait (true);
			var json = result.GetResponseText ();
			var jsonValue = JsonValue.Parse (json);
			if (jsonValue != null) {
				// Grab the newly created objects ID from the result and store on account object
				// We will need this set for any future operations we do on this object.
				account.Id = jsonValue["id"];
			}
			FinishAddAccount (account);
		}

		void FinishAddAccount (AccountObject account)
		{
			// Reset the form for the next use.
			AddAccountController.DismissViewController(true, ()=> {
				// Insert our newly created object into the table
				DataSource.Objects.Add(account);
				TableView.ReloadData();

				AddAccountController.Dispose();
				AddAccountController = null;
			});
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			NavigationItem.RightBarButtonItem = EditButtonItem;
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, (o, e) => AddNewItem ());

			TableView.Source = DataSource = new DataSource (this);

			InitializeSalesforce ();

			DetailViewController = new DetailViewController();
			DetailViewController.ItemUpdated += OnItemUpdated;
		}

		async void OnItemUpdated (object sender, AccountObject args)
		{
			var request = new UpdateRequest { Resource = args };
			await Client.ProcessAsync (request);
			LoadAccounts ();
			NavigationController.PopViewControllerAnimated (true);
		}

		void InitializeSalesforce ()
		{
			const string clientId = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
			const string clientSecret = "5754078534436456018";
			var redirectUrl = new Uri ("com.sample.salesforce:/oauth2Callback");

			// Creates our connection to salesforce.
			Client = new SalesforceClient (clientId, clientSecret, redirectUrl);
			Client.AuthenticationComplete += (sender, e) => OnAuthenticationCompleted (e);

			// Get authenticated users from the local keystore
			var users = Client.LoadUsers ();
			if (!users.Any ()) {
				// Begin OAuth journey
				StartAuthorization (); 
			} else {
				// Immediately go to accounts screen
				LoadAccounts (); 
			}
		}

		void StartAuthorization ()
		{
			var loginController = Client.GetLoginInterface () as UIViewController;
			PresentViewController (loginController, true, null);
		}

		void OnAuthenticationCompleted (AuthenticatorCompletedEventArgs e)
		{
			if (!e.IsAuthenticated) {
				// TODO: Handle failed login scenario by re-presenting login form with error
				throw new Exception ("Login failed and we don't handle that.");
			}

			DismissViewController (true, () => {
				NavigationItem.RightBarButtonItem = null;
				LoadAccounts ();
			});
		}

		async void LoadAccounts ()
		{
			SetLoadingState (true);
			var request = new ReadRequest {
				// TODO : Add error handling for when this query asks for stuff that does not exist (mispell a field to reproduce)
				// Query language is SOQL. Documentation can be found at http://www.salesforce.com/us/developer/docs/soql_sosl/
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber, Phone, Website, Industry FROM Account" }
			};

			var handledAlready = false;
			Response response = null;

			try {
				response = await Client.ProcessAsync (request);
			} catch (AggregateException ex) {

				// Since we're using process async, we're going to
				// get an aggregate exception that we need to unwrap
				// before we decide how to handle it.
				var e = ex.Flatten ().InnerException;
				Debug.WriteLine ("loadaccounts: process returned: " + e);

				if (e is InvalidSessionException)
					InitializeSalesforce ();
				else if (e is WebException)
					ShowGeneralNetworkError ();
				else
					throw e;

				handledAlready = true;
			}

			if (handledAlready)
			{
				SetLoadingState (false);
				return;
			}

			if (response == null)
			{
				Debug.WriteLine("loadaccounts: re-initializing salesforce.");
				InitializeSalesforce (); //StartAuthorization ();
				return;
			}
			var result = response.GetResponseText ();
			var jsonValue = JsonValue.Parse(result);

			if (jsonValue == null) {
				throw new Exception("Could not parse Json data");
			}

			var results = jsonValue["records"];

			// Marshal our data into account objects
			DataSource.Objects = results.OfType<JsonObject>().Select (j => new SObject(j).As<AccountObject> ()).ToList();
			SetLoadingState (false);
		}

		static void ShowGeneralNetworkError ()
		{
			const string message = "Looks like you aren't connected to the Internet.";
			var alertView = new UIAlertView ("Oops!", message, null, "Dismiss", null);
			alertView.Show ();
		}

		internal static void SetLoadingState(bool loading)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = loading;
		}
	}
}
