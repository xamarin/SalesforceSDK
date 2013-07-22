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
		public Account CurrentUser { get; set; }

		public SalesforceClient (String appKey, Uri callbackUri)
		{
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
		public void Save(IAccount account)
		{
			Adapter.SaveAccount (account);
		}

		/// <summary>
		/// Loads the accounts saved in the platform-specific credential store.
		/// </summary>
		/// <returns>The accounts.</returns>
		public IEnumerable<IAccount> LoadAccounts()
		{
			return Adapter.LoadAccounts ();
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

