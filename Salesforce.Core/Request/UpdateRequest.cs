using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using Xamarin.Auth;

namespace Salesforce
{
	public class UpdateRequest : IAuthenticatedRequest
	{
		public String RequestType { get; set; }

		public ISalesforceResource Resource {	get ; set ; }

		public IDictionary<string, string> Headers { get ; set ; }

		public String Method { get { return HttpMethod.Patch; } }

		public IDictionary<string, JsonValue> Options { get; private set; }

		public OAuth2Request ToOAuth2Request (ISalesforceUser user)
		{
			if (!(Resource is SObject))
				throw new InvalidOperationException ();

			var path = user.Properties ["instance_url"] + SalesforceClient.RestApiPath;
			var baseUri = new Uri (path);
			var uri = new Uri (baseUri, Resource.AbsoluteUri);

			var r = Resource as SObject;
			r.OnPreparingUpdateRequest ();
			var oauthRequest = new OAuth2Request (Method, uri, r.Options.ToDictionary (k => k.Key, v => (String)v.Value), user);

			return oauthRequest;
		}

		public UpdateRequest () : this (null) { }

		public UpdateRequest (ISalesforceResource resource)
		{	
			Headers = new Dictionary<string,string>{
				{ "Content-Type", "application/json" }
			};
			Resource = resource;
			if (Resource == null) return;

			Options = resource.Options ?? new Dictionary<string,JsonValue> ();
		}

		public override string ToString () 
		{
			return string.Format (@"{0} {1}", Method, Resource);
		}	
	}
}

