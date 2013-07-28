using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using System.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SalesforceSample.iOS
{
	public sealed partial class RootViewController : UITableViewController
	{
		public DataSource DataSource { get; private set; }
		public SalesforceClient Client { get; private set; }
		public DetailViewController DetailViewController { get; set; }

		UIViewController LoginController { get; set; }

		public RootViewController () : base ("RootViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Accounts", "Accounts");
		}

		void AddNewItem (object sender, EventArgs args)
		{
			try {
				// TODO: Present an AddAccountController.
				//PresentViewController(LoginController, true, null);
			} catch (Exception ex) {
				Debug.WriteLine (ex.Message);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

//			NavigationItem.RightBarButtonItem = EditButtonItem;
//			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, AddNewItem);

			TableView.Source = DataSource = new DataSource (this);

			InitializeSalesforce ();
		}

		void InitializeSalesforce ()
		{
			const string clientId = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
			const string clientSecret = "5754078534436456018";
			var redirectUrl = new Uri ("com.sample.salesforce:/oauth2Callback");

			Client = new SalesforceClient (clientId, clientSecret, redirectUrl);
			Client.AuthenticationComplete += (sender, e) => OnAuthenticationCompleted (e);

			var users = Client.LoadUsers ();
			if (!users.Any ()) {
				StartAuthorization ();
			}
			else {
				LoadAccounts ();
			}
		}

		public void StartAuthorization ()
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
				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber, Phone, Website, Industry FROM Account" }
			};

			var handledAlready = false;

		var scheduler = TaskScheduler.FromCurrentSynchronizationContext ();
			var response = await Client.ProcessAsync (request).ContinueWith<Response>(r => {
				if (r.IsFaulted && r.Exception.InnerException.InnerException is InvalidSessionException) 
				{
					Debug.WriteLine("loadaccounts: process returned: " + r.Exception);
					return null;
				} 
				else if (r.IsFaulted)
				{
					Debug.WriteLine(r.Exception.Flatten().InnerException);
					ShowGeneralNetworkError();
					handledAlready = true;
					return null;
				}
				return r.Result;
			}, scheduler);

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

			DataSource.Objects = results.OfType<JsonValue>().ToList();
			SetLoadingState (false);
		}

		static void ShowGeneralNetworkError ()
		{
			var message = "Looks like you aren't connected to the Internet.";
			var alertView = new UIAlertView ("Oops!", message, null, "Dismiss", null);
			alertView.Show ();
		}

		internal void SetLoadingState(bool loading)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = loading;
		}
	}
}
