using System;
using Xamarin.Auth;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Diagnostics;

namespace Salesforce
{
	public class SalesforceClient
	{
		/// <summary>
		/// The Salesforce OAuth authorization endpoint.
		/// </summary>
		protected static readonly string AuthPath = @"https://login.salesforce.com/services/oauth2/authorize";

		/// <summary>
		/// The Salesforce OAuth token endpoint.
		/// </summary>
		protected static readonly string TokenPath = "https://login.salesforce.com/services/oauth2/token";

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

		/// <summary>
		/// Occurs when Salesforce OAuth authentication has completed.
		/// </summary>
		public event EventHandler<AuthenticatorCompletedEventArgs> AuthenticationComplete;

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
		/// Your Salesforce application's Customer Id.
		/// </summary>
		/// <value>The app key.</value>
		private readonly string ClientId;

		/// <summary>
		/// Your Salesforce application's Customer Secret.
		/// </summary>
		private readonly string ClientSecret;

		/// <summary>
		/// Initializes a new instance of the <see cref="Salesforce.SalesforceClient"/> class.
		/// </summary>
		/// <param name="appKey">App key.</param>
		/// <param name="callbackUri">Callback URI.</param>
		public SalesforceClient (String clientId, String clientSecret, Uri redirectUrl)
		{
			ClientId = clientId;
			ClientSecret = clientSecret;

			Scheduler = TaskScheduler.Default;

			#if PLATFORM_IOS
			Adapter = new UIKitPlatformAdapter();
			#elif PLATFORM_ANDROID
			Adapter = new AndroidPlatformAdapter();
			#endif

			var users = LoadUsers ().ToArray();

			if (users.Count () > 0) {
				CurrentUser = users.First ();

				Debug.WriteLine (CurrentUser);

				foreach (var p in CurrentUser.Properties) {
					Debug.WriteLine ("{0}\t{1}", p.Key, p.Value);
				}
			}

			Authenticator = new OAuth2Authenticator (
				clientId: clientId,
				clientSecret: ClientSecret,
				scope: "api refresh_token", // TODO: Convert this to a static struct. Or not.
				authorizeUrl: new Uri(AuthPath),
				redirectUrl: redirectUrl,
				accessTokenUrl: new Uri(TokenPath),
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

			Adapter.Authenticator = Authenticator;

			Authenticator.Completed += OnCompleted;
		}

		/// <summary>
		/// Sets the current UI context.
		/// </summary>
		/// <remarks>
		/// On Android, the context defaults to the application context.
		/// </remarks>
		/// <param name="context">Context.</param>
		public static void SetCurrentContext (object context)
		{
#if PLATFORM_ANDROID
			AndroidPlatformAdapter.CurrentPlatformContext = (global::Android.Content.Context)context;
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
		/// Initiates a synchronous request to the Salesforce API.
		/// </summary>
		/// <param name="request">An IRestRequest.</param>
		/// <typeparam name="T">The type of the IRestRequest.</typeparam>
		public Response Process<T>(IAuthenticatedRequest request) where T: class, IAuthenticatedRequest
		{
			Task<Response> task = null;
			Response result = null;
			try
			{
				task = ProcessAsync (request);
				task.Wait (TimeSpan.FromSeconds (90)); // TODO: Move this to a config setting.
				result = task.Result;
			}
			catch (AggregateException ex)
			{
				var flatEx = task.Exception.Flatten ();
				if (task.IsFaulted)
				{
					// We only want to swallow this 
					// exception if we have reason to 
					// believe our access_token is stale.
					var webEx = flatEx
						.InnerExceptions
						.FirstOrDefault (e => e.GetType () == typeof(WebException));

					if (webEx == null || ((HttpWebResponse)((WebException)webEx).Response).StatusCode != HttpStatusCode.Unauthorized)
						throw;

					// Refresh the OAuth2 session token.
					CurrentUser = RefreshSessionToken ();

					// Retry our request with the new token.
					var retryTask = ProcessAsync (request);
					retryTask.Wait (TimeSpan.FromSeconds (90)); // TODO: Move this to a config setting.
				}
				throw new ApplicationException(flatEx.Message, flatEx);
			}

			return result;
		}

		/// <summary>
		/// Process the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Task<Response> ProcessAsync<T>(T request) where T: class, IAuthenticatedRequest
		{
			var oauthRequest = request.ToOAuth2Request(CurrentUser);

			Console.WriteLine (oauthRequest.Url);

			Task<Response> task = null;
			task = oauthRequest.GetResponseAsync ().ContinueWith (response => {

				if (!response.IsFaulted) return response.Result;

				if (IsUnauthorizedError(response.Exception))
				{
					// Refresh the OAuth2 session token.
					CurrentUser = RefreshSessionToken ();

					// Retry our request with the new token.
					var retryTask = ProcessAsync (request);
					retryTask.Wait (TimeSpan.FromSeconds (90)); // TODO: Move this to a config setting.
					return retryTask.Result;
				}
				// TODO: Detect if the refresh token itself has expired.

				return response.Result;
			}, Scheduler);

			return task; // TODO: Create a public invoker that returns a Salesforce domain object.
		}

		/// <summary>
		/// Determines whether a request failed due to an invalid access token.
		/// </summary>
		/// <returns><c>true</c> if this threw an WebException with a StatusCode set to Unauthorized; otherwise, <c>false</c>.</returns>
		/// <param name="exception">Exception.</param>
		protected bool IsUnauthorizedError (AggregateException exception)
		{
			var ex = exception.Flatten()
				.InnerExceptions
					.FirstOrDefault (e => e.GetType () == typeof(WebException));

			if (ex == null) return false;

			var webEx = ex as WebException;
			if (webEx == null) return false;

			var response = webEx.Response as HttpWebResponse;
			return response.StatusCode == HttpStatusCode.Unauthorized;
		}

		/// <summary>
		/// Requests a new session token.
		/// </summary>
		/// <returns>The Salesforce user with the updated session token.</returns>
		private ISalesforceUser RefreshSessionToken()
		{
			var refreshToken = CurrentUser.Properties ["refresh_token"];
			var refreshTask = Authenticator.RequestAccessTokenAsync(new Dictionary<string, string> 
			                                                        {
				{ "grant_type", "refresh_token" },
				{ "client_id", ClientId },
				{ "client_secret", ClientSecret },
				{ "refresh_token", refreshToken }
			});
			refreshTask.Wait (TimeSpan.FromSeconds(90)); // TODO: Move this to a config setting.
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

			var ev = AuthenticationComplete;
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

