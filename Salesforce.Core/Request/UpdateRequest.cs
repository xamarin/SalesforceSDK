using System;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Salesforce
{
	public class UpdateRequest : IAuthenticatedRequest
	{
		public String RequestType { get; set; }

		public ISalesforceResource Resource {	get ; set ; }

		public IDictionary<string, string> Headers { get ; set ; }

		public String Method { get { return HttpMethod.Patch; } }

		public IDictionary<string, string> Options { get; private set; }

		public OAuth2Request ToOAuth2Request (ISalesforceUser user)
		{
			var path = user.Properties ["instance_url"] + SalesforceClient.RestApiPath;
			var baseUri = new Uri (path);
			var uri = new Uri (baseUri, Resource.AbsoluteUri);

			var oauthRequest = new OAuth2Request (Method, uri, Resource.Options, user);

			return oauthRequest;
		}

		public UpdateRequest (ISalesforceResource resource)
		{	
			Headers = new Dictionary<string,string>{
				{ "Content-Type", "application/json" }
			};
			Resource = resource;
			if (Resource == null) return;

			Options = resource.Options ?? new Dictionary<string,string> ();
		}

		public override string ToString () 
		{
			return string.Format (@"{0} {1}", Method, Resource);
		}	
	}
}

