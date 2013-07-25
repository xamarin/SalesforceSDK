using System;
using Xamarin.Auth;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;
using System.Linq;

namespace Salesforce
{
	public class SalesforceClient
	{
		private readonly string ClientSecret = "5754078534436456018"; // TODO: Convert to ctor param.

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

		string AppKey {
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Salesforce.SalesforceClient"/> class.
		/// </summary>
		/// <param name="appKey">App key.</param>
		/// <param name="callbackUri">Callback URI.</param>
		public SalesforceClient (String appKey, Uri redirectUrl)
		{
			AppKey = appKey;
			Scheduler = TaskScheduler.Default;

			#if PLATFORM_IOS
			Adapter = new UIKitPlatformAdapter();
			#elif PLATFORM_ANDROID
			Adapter = new AndroidPlatformAdapter();
			#endif

			var users = LoadUsers ();

			if (users.Count() > 0)
			{
				CurrentUser = users.First ();

				Console.WriteLine (CurrentUser);

				foreach(var p in CurrentUser.Properties)
				{
					Console.WriteLine("{0}\t{1}", p.Key, p.Value);
				}

				Authenticator = new OAuth2Authenticator (
					clientId: appKey,
					clientSecret: ClientSecret,
					scope: "refresh_token", // TODO: Convert this to a static struct. Or not.
					authorizeUrl: new Uri(AuthPath),
					redirectUrl: redirectUrl,
					accessTokenUrl: new Uri("https://login.salesforce.com/services/oauth2/token"),
					getUsernameAsync: null
				);
			} 
			else
			{
				Authenticator = new OAuth2Authenticator (
					clientId: appKey,
					clientSecret: ClientSecret,
					scope: "api refresh_token", // TODO: Convert this to a static struct. Or not.
					authorizeUrl: new Uri(AuthPath),
					redirectUrl: redirectUrl,
					accessTokenUrl: new Uri("https://login.salesforce.com/services/oauth2/token"),
					getUsernameAsync: new GetUsernameAsyncFunc((dict)=>{

						var client = new WebClient();
						client.Headers["Authorization"] = "Bearer " + dict["access_token"];

						var results = client.DownloadString(dict["id"]);
						var resultVals = JsonValue.Parse(results);

						return Task.Factory.StartNew(()=> { 
							return (String)resultVals["username"];
						});
					})
				);
			}

			Adapter.Authenticator = Authenticator;

			Authenticator.Completed += OnCompleted;
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
		/// Initiates a synchronous request to the Salesforce API.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Response Process<T>(IRestRequest request) where T: class, IRestRequest
		{
			Task<Response> task = null;
			Response result = null;
			try
			{
				task = ProcessRequest (request);
				task.Wait (TimeSpan.FromSeconds (90)); // TODO: Move this to a config setting.
				result = task.Result;
			}
			catch (AggregateException)
			{

				if (task.IsFaulted)
				{
					// We only want to swallow this 
					// exception if we have reason to 
					// believe our access_token is stale.
					var webEx = task.Exception.Flatten().InnerExceptions.FirstOrDefault (e => e.GetType () == typeof(WebException));
					if (webEx == null || ((HttpWebResponse)((WebException)webEx).Response).StatusCode != HttpStatusCode.Unauthorized)
						throw;

					// Refresh the OAuth2 session token.
					CurrentUser = RefreshSessionToken ();
					// Retry our request with the new token.
					var retryTask = ProcessRequest (request);
					retryTask.Wait (TimeSpan.FromSeconds (90)); // TODO: Move this to a config setting.
					result = retryTask.Result;
				}
			}

			return result;
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
				return response.Result;
			}, Scheduler);
			return task; // TODO: Create a public invoker that returns a Salesforce domain object.
		}

		/// <summary>
		/// Requests a new session token.
		/// </summary>
		/// <returns>The Salesforce user with the updated session token.</returns>
		private ISalesforceUser RefreshSessionToken()
		{
			var refreshTask = Authenticator.RequestAccessTokenAsync(new Dictionary<string, string> 
			                                                        {
				{ "grant_type", "refresh_token" },
				{ "client_id", AppKey },
				{ "client_secret", ClientSecret },
				{ "refresh_token", CurrentUser.Properties["refresh_token"] }
			});
			refreshTask.Wait ();
			CurrentUser.Properties ["access_token"] = refreshTask.Result ["access_token"];
			return CurrentUser;
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

