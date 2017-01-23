using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Diagnostics;
using System.IO;

using Xamarin.Auth;

namespace Salesforce
{
    public class SalesforceClient
    {
#if URI_FIX
		internal const string RestApiPath = "services/data/";
#else
        internal const string RestApiPath = "/services/data/";
#endif

        volatile static int defaultNetworkTimeout = 90;
        /// <summary>
        /// The default network timeout.
        /// </summary>
        /// <value>The default network timeout.</value>
        public static int DefaultNetworkTimeout
        {
            get
            {
                return defaultNetworkTimeout;
            }
        }

        /// <summary>
        /// The Salesforce OAuth authorization endpoint.
        /// </summary>
        //protected static readonly string AuthPath = @"https://login.salesforce.com/services/oauth2/authorize";
        //----------------------------------------------------------------------------------------
        // moljac# 2015-09-15
        // auth and token endpoint were hardcoded, to use it for development sandboxes
        // hardcoded strings are changed to properties for testing sandboxes
        public static string AuthPath
        {
            get
            {
                return auth_path;
            } // AuthPath.get
            set
            {
                if (auth_path != value)
                {
                    {
                        // Set the property value
                        auth_path = value;
                        // raise/trigger Event if somebody has subscribed to the event
                        if (null != AuthPathChanged)
                        {
                            // raise/trigger Event
                            AuthPathChanged(null, new EventArgs());
                        }
                    }
                }

                return;
            } // AuthPath.set
        } // AuthPath

        private static string auth_path;
        public static event EventHandler AuthPathChanged;
        //-------------------------------------------------------------------------	        

        /// <summary>
        /// The Salesforce OAuth token endpoint.
        /// </summary>
        //protected static readonly string TokenPath = "https://login.salesforce.com/services/oauth2/token";
        //----------------------------------------------------------------------------------------
        // moljac# 2015-09-15
        // auth and token endpoint were hardcoded, to use it for development sandboxes
        // hardcoded strings are changed to properties for testing sandboxes
        public static string TokenPath
        {
            get
            {
                return token_path;
            } // AuthPath.get
            set
            {
                if (token_path != value)
                {
                    {
                        // Set the property value
                        token_path = value;
                        // raise/trigger Event if somebody has subscribed to the event
                        if (null != TokenPathChanged)
                        {
                            // raise/trigger Event
                            TokenPathChanged(null, new EventArgs());
                        }
                    }
                }

                return;
            } // AuthPath.set
        } // AuthPath

        private static string token_path;
        public static event EventHandler TokenPathChanged;
        //----------------------------------------------------------------------------------------

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

        volatile ISalesforceUser currentUser;

        /// <summary>
        /// The currently authenticated Salesforce user.
        /// </summary>
        /// <value>The current user.</value>
        public ISalesforceUser CurrentUser
        {
            get
            {
                return currentUser;
            }
            set
            {
                currentUser = value;
            }
        }

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
        /// Gets or sets the UI thread's scheduler.
        /// </summary>
        /// <remarks>
        /// Constructor should be called from the UI thread
        /// to ensure safe dispatch to UI elements.
        /// </remarks>
        /// <value>The scheduler.</value>
        protected TaskScheduler MainThreadScheduler { get; set; }

        /// <summary>
        /// Your Salesforce application's Customer Id.
        /// </summary>
        /// <value>The app key.</value>
        private readonly string ClientId;

        /// <summary>
        /// Your Salesforce application's Customer Secret.
        /// </summary>
        private readonly string ClientSecret;

        static SalesforceClient()
        {
            //----------------------------------------------------------------------------------------
            // moljac# 2015-09-15
            // auth and token endpoint were hardcoded, to use it for development sandboxes
            // hardcoded strings are changed to properties for testing sandboxes
            SalesforceClient.AuthPath = @"https://login.salesforce.com/services/oauth2/authorize";
            SalesforceClient.TokenPath = @"https://login.salesforce.com/services/oauth2/token";
            //----------------------------------------------------------------------------------------

            return;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Salesforce.SalesforceClient"/> class.
        /// </summary>
        /// <param name="appKey">App key.</param>
        /// <param name="callbackUri">Callback URI.</param>
        public SalesforceClient(String clientId, String clientSecret, Uri redirectUrl)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            MainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Scheduler = TaskScheduler.Default;

#if PLATFORM_IOS
			Adapter = new UIKitPlatformAdapter();
#elif __ANDROID__
			Adapter = new AndroidPlatformAdapter();
#endif

            var users = LoadUsers().Where(u => !u.RequiresReauthentication).ToArray();

            if (users.Count() > 0)
            {
                CurrentUser = users.First();

                Debug.WriteLine(CurrentUser);

                foreach (var p in CurrentUser.Properties)
                {
                    Debug.WriteLine("{0}\t{1}", p.Key, p.Value);
                }
            }

            //
            // mark.tap@xamarin.com 2015.01.08
            //
            // Note that here the original authors used the OAuth2Authenticator constructor intended for the Web-Server Flow;
            // however, they then set AccessTokenUrl to null to force the authenticator to run the User-Agent Flow.
            // I think they did this to load the ClientSecret into the authenticator for use with refresh (the ClientSecret
            // isn't needed for the User-Agent Flow).
            //
            Authenticator = new OAuth2Authenticator
                (
                    clientId: clientId,
                    clientSecret: ClientSecret,
                    scope: "api refresh_token",
                    authorizeUrl: new Uri(AuthPath),
                    redirectUrl: redirectUrl,
                    accessTokenUrl: new Uri(TokenPath),
                    getUsernameAsync: new GetUsernameAsyncFunc(HandleGetUsernameAsyncFunc)
                );

            if (CurrentUser == null || CurrentUser.RequiresReauthentication)
                Authenticator.AccessTokenUrl = null;

            Adapter.Authenticator = Authenticator;

            Authenticator.Completed += OnAuthenticationCompleted;
        }

        protected Task<string> HandleGetUsernameAsyncFunc(IDictionary<string, string> accountProperties)
        {
            string results = null;
            #if __IOS__ || __ANDROID__ || MOBILE
            System.Net.WebClient client = new System.Net.WebClient();
            client.Headers["Authorization"] = "Bearer " + accountProperties["access_token"];

            results = client.DownloadString(accountProperties["id"]);
            #else

            #endif
            var resultVals = JsonValue.Parse(results);

            return Task.Factory.StartNew
                                    (
                                        () =>
                                        {
                                            return (String)resultVals["username"];
                                        }
                                    );
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
			Debug.WriteLine ("Saving user: " + account);
			Adapter.SaveAccount (account);
		}

		/// <summary>
		/// Loads the accounts saved in the platform-specific credential store.
		/// </summary>
		/// <returns>The accounts.</returns>
		public IEnumerable<ISalesforceUser> LoadUsers()
		{
			var users = Adapter.LoadAccounts ().Where(u => !u.RequiresReauthentication);
			Debug.WriteLine ("Loading {0} users", users.Count());
			return users;
		}

		/// <summary>
		/// Initiates a synchronous request to the Salesforce API.
		/// </summary>
		/// <param name="request">An IRestRequest.</param>
		/// <typeparam name="T">The type of the IRestRequest.</typeparam>
		public Response Process<T>(IAuthenticatedRequest request) where T: class, IAuthenticatedRequest
		{
			Task<Response> task = null;
			try
			{
				task = ProcessAsync (request);
				task.Wait (TimeSpan.FromSeconds(DefaultNetworkTimeout));
				if (!task.IsFaulted)
					return task.Result;

				Debug.WriteLine ("Process rethrowing exception: " + task.Exception);
				throw task.Exception.InnerException;
			}
			catch (AggregateException)
			{
				// We're hiding the fact that this method
				// is using the async version. That means
				// we need to remove the aggregate exception
				// wrapper and retrow the actual exception.
				throw task.Exception.InnerException;
			}
		}

		/// <summary>
		/// Process the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Task<Response> ProcessAsync<T>(T request) where T: class, IAuthenticatedRequest
		{
			if ((CurrentUser = Adapter.LoadAccounts().FirstOrDefault()) == null)
			{
				var message = String.Format ("No user available in credential store for service {0}.", PlatformStrings.CredentialStoreServiceName);
				throw new InvalidSessionException(message);
			}

			var oauthRequest = request.ToOAuth2Request(CurrentUser);

			Debug.WriteLine (oauthRequest.Url);

			Task<Response> task = null;

			task = oauthRequest.GetResponseAsync ().ContinueWith (response => {

				if (!response.IsFaulted) return response.Result;

				var innerEx = response.Exception.Flatten().InnerException;
				if (!(innerEx is System.Net.WebException))
					throw innerEx;

				var responseBody = ProcessResponseBody (response);
				if (responseBody == String.Empty) throw innerEx;

				Debug.WriteLine("request fail: " + responseBody);

				var errorDetails = JsonValue.Parse(responseBody).OfType<JsonObject>().ToArray();

				//TODO: Needs refactoring. This method is too long.

				if (errorDetails.Any (e => e.ContainsKey("error") && e["error"] == "invalid_client_id"))
				{
					var message = errorDetails [0] ["error_description"];
					Debug.WriteLine("reason: " + message);
					throw new InvalidClientIdException (String.Format("{0}. The value passed to the SalesforceClient constructor was does not match the value set in your Salesforce application's configuration.", message), ClientId);
				}

				if (errorDetails.Any (e => e.ContainsKey("error") && e["error"] == "invalid_grant"))
				{
					ForceUserReauthorization(true);

					var message = errorDetails [0] ["error_description"];
					Debug.WriteLine("reason: " + message);
					throw new InvalidSessionException (message);
				}

				// Handles: [{"message":"Cannot deserialize instance of datetime from VALUE_STRING value \"2013-08-12T15:20:00+0000\" at [line:1, column:179]","errorCode":"JSON_PARSER_ERROR"}]
				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "JSON_PARSER_ERROR"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					throw new JsonParseException (message);
				}

                // Handles EXCEEDED_ID_LIMIT
                if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "EXCEEDED_ID_LIMIT"))
                {
                    var message = errorDetails [0] ["message"];
                    Debug.WriteLine("reason: " + message);
                    throw new ExceededChangesLimitException (message);
                }

				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "INVALID_SESSION_ID"))
				{
					// Refresh the OAuth2 session token.
					CurrentUser = RefreshSessionToken ();
					Save (CurrentUser);

					Debug.WriteLine("reason: invalid session id.");

					// Retry our request with the new token.
					var retryTask = ProcessAsync (request).ContinueWith(retryResponse => {
						Debug.WriteLine("Retrying with new token.");
						if (!retryResponse.IsFaulted) return retryResponse.Result;

						ForceUserReauthorization(true);

						var retryResponseBody = ProcessResponseBody(retryResponse);
						var retryErrorDetails = JsonValue.Parse(retryResponseBody).OfType<JsonObject>().ToArray();

						Debug.WriteLine("retry request fail: " + retryResponseBody);

						if (retryErrorDetails.Any (e => 
						                      (e.ContainsKey("errorCode") && e["errorCode"] == "INVALID_SESSION_ID") ||
						    				  (e.ContainsKey("error") && e["error"] == "invalid_grant")))
							throw new InvalidSessionException(retryResponse.Exception.Message);

						return retryResponse.Result;
					}, TaskScheduler.Default);

					return retryTask.Result;
				}

				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "DELETE_FAILED"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					throw new DeleteFailedException (message);
				}

				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "INSUFFICIENT_ACCESS_OR_READONLY"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					throw new InsufficientRightsException (message);
				}

				// Handles: [{"message":"The requested resource does not exist","errorCode":"NOT_FOUND"}]
				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "NOT_FOUND"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					throw new MissingResourceException (message);
				}

				// Handles: [{"message":"The Id field should not be specified in the sobject data.","errorCode":"INVALID_FIELD"}]
				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "INVALID_FIELD"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					throw new InvalidFieldException (message);
				}

				// Handles: [{"fields": ["LastModifiedDate"], "message": "Unable to create/update fields: LastModifiedDate. Please check the security settings of this field and verify that it is read/write for your profile or permission set.", "errorCode": "INVALID_FIELD_FOR_INSERT_UPDATE"}]
				if (errorDetails.Any (e => e.ContainsKey("errorCode") && e["errorCode"] == "INVALID_FIELD_FOR_INSERT_UPDATE"))
				{
					var message = errorDetails [0] ["message"];
					Debug.WriteLine("reason: " + message);
					var fields = errorDetails[0]["fields"] as JsonArray;
					throw new InvalidFieldException (message, fields.Cast<String>());
				}

				Debug.WriteLine("reason: returning result b/c not sure how to handle this exception: " + response.Exception);
				throw innerEx;

				//return response.Result;
			});

			return task;
		}

		static string ProcessResponseBody (Task response)
		{
			var webEx = response.Exception.InnerException.InnerException as System.Net.WebException;
			if (webEx == null) return string.Empty;
			if (webEx.Response == null) return string.Empty;

			var stream = webEx.Response.GetResponseStream ();
			if (stream == null) return string.Empty;

			var responseBody = new StreamReader (stream).ReadToEnd ();
			return responseBody;
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
					.FirstOrDefault (e => e.GetType () == typeof(System.Net.WebException));

			if (ex == null) return false;

			var webEx = ex as System.Net.WebException;
			if (webEx == null) return false;

			var response = webEx.Response as System.Net.HttpWebResponse;
			if (response == null) return false;

			return response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
		}

		void ForceUserReauthorization (bool required)
		{
			if (CurrentUser == null) return;

			((SalesforceUser)CurrentUser).RequiresReauthentication = required;
			Debug.WriteLine ("{0} forced to reauth? {1}", CurrentUser.Username, required);
			Save (CurrentUser);
		}

		ISalesforceUser RefreshSessionToken() // Request a new access_token and return the user with the updated token
		{
			//
			// mark.tap@xamarin.com 2015.01.08
			//
			// RefreshSessionToken was failing if the user had logged in during the current session. This is because
			// the SalesforceClient constructor sets AccessTokenUrl to null when a login is needed in order to force
			// the OAuth2Authenticator to use the implicit flow rather than the web server flow. Since AccessTokenUrl
			// is null in the Authenticator, the WebRequest.Create() call in the Authenticator throws an exception
			// when this refresh code executes.
			//
			// Refresh did work if the user logged in, saved the user to disk, closed the app, and restarted it since
			// then the SalesforceClient constructor did not set AccessTokenUrl to null in the Authenticator.
			//
			// The added lines of code below reload the AccessTokenUrl into the Authenticator if needed.
			// We then reset to null to preserve all other behavior in SalesforceClient that might rely on
			// AccessTokenUrl being null (e.g. if the app asks the user to login again, the AccessTokenUrl
			// must be null for the OAuth2Authenticator to choose the User-Agent Flow...if it's not null
			// the authenticator will perform the Web-Server Flow).
			//
			// This code is not ideal but it fixes a bug. Perhaps two OAuth2Authenticators should be used by SalesforceClient,
			// one for login and one for refresh. Using a single Authenticator for both is confusing and error prone as
			// demonstrated by the need for this patch. Using two would also allow the login Authenticator to use
			// the constructor intended for the User-Agent Flow...right now they use the constructor that should
			// perform the Web-Server Flow and then set AccessTokenUrl to null to change the behavior.
			// 
			bool updateAccessTokenUrl = false;                     // mark.tap@xamarin.com 2015.01.08
			if (Authenticator.AccessTokenUrl == null)              // mark.tap@xamarin.com 2015.01.08
			{
				updateAccessTokenUrl = true;                       // mark.tap@xamarin.com 2015.01.08
				Authenticator.AccessTokenUrl = new Uri(TokenPath); // mark.tap@xamarin.com 2015.01.08
			}

			var queryOptions = new Dictionary<string, string>
			{
				{ "grant_type",   "refresh_token"                          },
				{ "client_id",     ClientId                                },
				{ "client_secret", ClientSecret                            },
				{ "refresh_token", CurrentUser.Properties["refresh_token"] }
			};

			var refreshTask = Authenticator.RequestAccessTokenAsync(queryOptions).ContinueWith(response =>
				{
					if (updateAccessTokenUrl)                // mark.tap@xamarin.com 2015.01.08
						Authenticator.AccessTokenUrl = null; // mark.tap@xamarin.com 2015.01.08

					if (!response.IsFaulted)
					{
						ForceUserReauthorization(false);

						return response.Result;
					}

				ForceUserReauthorization(true);

				var responseBody = ProcessResponseBody(response);
				Debug.WriteLine(responseBody);
				var responseData = JsonValue.Parse(responseBody);

				if (responseData.ContainsKey("error") && responseData["error"] == "invalid_grant")
					throw new InvalidSessionException(responseData["error_description"]);

				return response.Result;
			}, TaskScheduler.Default);

			// Don't need to await here, b/c we're already inside of a task.
			refreshTask.Wait (TimeSpan.FromSeconds (DefaultNetworkTimeout));
			CurrentUser.Properties ["access_token"] = refreshTask.Result ["access_token"];
			return CurrentUser;
		}

		void OnAuthenticationCompleted(object sender, AuthenticatorCompletedEventArgs args)
		{
			if (args.IsAuthenticated)
			{
                CurrentUser = (ISalesforceUser) args.Account;      // load newly-authenticated user as CurrentUser
				ForceUserReauthorization(false); // applies to new user, necessary because the default is 'true' in SaleforceUser
			}
			else
			{
				ForceUserReauthorization(true);  // applies to previous user
			}

			Invoke(AuthenticationComplete, this, args);
		}

		void Invoke<A>(EventHandler<A> ev, object sender, A args)
		{
			if (ev != null)
				ev.Invoke(sender, args);
		}
	}
}