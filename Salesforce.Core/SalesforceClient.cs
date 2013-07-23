using System;
using Xamarin.Auth;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;

namespace Salesforce
{
	public class SalesforceClient
	{
		/// <summary>
		/// The Salesforce OAuth authorization endpoint.
		/// </summary>
		protected static readonly string AuthPath = @"https://login.salesforce.com/services/oauth2/authorize";

		/// <summary>
		/// Handles the actual OAuth handshake.
		/// </summary>
		/// <value>The authenticator.</value>
		protected OAuth2Authenticator Authenticator { set; get; }

		/// <summary>
		/// Provides access to native services.
		/// </summary>
		/// <value>The adapter.</value>
		/// <remarks>
		/// This adapter is used to prevent platform abstractions
		/// from leaking into the API. It enables platform
		/// support to be solved via composition.
		/// </remarks>
		protected IPlatformAdapter Adapter { get; set; }

		// TODO: Refactor this to return a Salesforce account object.
		public event EventHandler<AuthenticatorCompletedEventArgs> AuthRequestCompleted;

		/// <summary>
		/// The currently authenticated Salesforce user.
		/// </summary>
		/// <value>The current user.</value>
		public ISalesforceUser CurrentUser { get; set; }

		/// <summary>
		/// Gets or sets the scheduler.
		/// </summary>
		/// <remarks>
		/// Constructor should be called from the UI thread
		/// to ensure safe dispatch to UI elements.
		/// </remarks>
		/// <value>The scheduler.</value>
		protected TaskScheduler Scheduler { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Salesforce.SalesforceClient"/> class.
		/// </summary>
		/// <param name="appKey">App key.</param>
		/// <param name="callbackUri">Callback URI.</param>
		public SalesforceClient (String appKey, Uri callbackUri)
		{
			Scheduler = TaskScheduler.Default; //TaskScheduler.FromCurrentSynchronizationContext();

			// TODO: Need to retrieve accounts before calling the constructor.
			// If we have the account, then we can refresh the session token.

			Authenticator = new OAuth2Authenticator (
				clientId: appKey,
				scope: "full", // TODO: Convert this to a static struct. Or not.
				authorizeUrl: new Uri(AuthPath),
				redirectUrl: callbackUri,
				getUsernameAsync: new GetUsernameAsyncFunc((dict)=>{
					var client = new WebClient();
					client.Headers["Authorization"] = "Bearer " + dict["access_token"];
					var results = client.DownloadString(dict["id"]);
					var resultVals = JsonValue.Parse(results);
					return Task.Factory.StartNew(()=> { 
						Console.WriteLine(results);
						return (String)resultVals["username"];
					});
				})
			);

			Authenticator.Completed += OnCompleted;

#if PLATFORM_IOS
			Adapter = new UIKitPlatformAdapter(Authenticator);
#elif PLATFORM_ANDROID
			Adapter = new AndroidPlatformAdapter(Authenticator);
#endif
		}

		/// <summary>
		/// Returns a platform-specific object used to display the Salesforce login/authorization UI.
		/// </summary>
		/// <param name="authenticationCallback">Authentication callback. Passes in either a UIViewController or Intent.</param>
		public object GetLoginInterface()
		{
			return Adapter.GetLoginUI();
		}

		/// <summary>
		/// Saves the account to the platform-specific credential store.
		/// </summary>
		/// <param name="account">Account.</param>
		public void Save(ISalesforceUser account)
		{
			Adapter.SaveAccount (account);
		}

		/// <summary>
		/// Loads the accounts saved in the platform-specific credential store.
		/// </summary>
		/// <returns>The accounts.</returns>
		public IEnumerable<ISalesforceUser> LoadUsers()
		{
			return Adapter.LoadAccounts ();
		}

		/// <summary>
		/// Initiates a request to the Salesforce API.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Response Process<T>(IRestRequest request) where T: class, IRestRequest
		{
			var task = ProcessRequest (request);

			task.Wait(TimeSpan.FromSeconds(90)); // TODO: Move this to a config setting.

			return task.Result;
		}

		/// <summary>
		/// Process the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected Task<Response> ProcessRequest<T>(T request) where T: class, IRestRequest
		{
			var baseUri = new Uri(CurrentUser.Properties ["instance_url"] + "/services/data/");
			var uri = new Uri (baseUri, request.Resource.AbsoluteUri);

			var oauthRequest = new OAuth2Request (request.Method, uri, request.Resource.Options, this.CurrentUser);

			Console.WriteLine (oauthRequest.Url);
			var task = oauthRequest.GetResponseAsync ().ContinueWith (response => {
				Console.WriteLine(response);
				if (response.IsFaulted)
					// TODO: Insert some basic retry logic here.
					return null;
				return response;
			}, Scheduler);
			return task.Result; // TODO: Create a public invoker that returns a Salesforce domain object.
		}

		private IAccount RefreshSession()
		{
			// 0 - REMOTE_ACCESS_CLIENT_ID
			// 1 REMOTE_ACCESS_CLIENT_SECRET
			@"https://login.salesforce.com/services/oauth2/token -d 'grant_type=password&client_id={0}&client_
   secret={1}&username=user@example.com&password=********' -H ""X-PrettyPrint: 1"""
		}

		/// <summary>
		/// Internal event handler for authentication compelete events.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		private void OnCompleted (object sender, AuthenticatorCompletedEventArgs args)
		{
			if (!args.IsAuthenticated)
				return;

			var ev = AuthRequestCompleted;
			if ( ev != null)
			{
				CurrentUser = args.Account;
				ev (sender, args);
			}
			else
				Console.WriteLine("No activation completion handler.");
		}

	}
}

