using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using Xamarin.Auth;

namespace Salesforce
{
	public class DescribeRequest : IAuthenticatedRequest
	{
		public ISalesforceResource Resource {	get ; set ; }

		public IDictionary<string, string> Headers { get ; set ; }

		public String Method { get { return HttpMethod.Get; } }

		public IDictionary<string, JsonValue> Options { get; private set; }

		public OAuth2Request ToOAuth2Request (ISalesforceUser user)
		{
			var path = user.Properties ["instance_url"] + SalesforceClient.RestApiPath;
			var baseUri = new Uri (path);
			var uri = new Uri (baseUri, Resource.AbsoluteUri);
			var oauthRequest = new OAuth2Request 
                                        (
                                            Method, 
                                            uri, 
                                            Resource.Options
                                                        .Where (kvp => kvp.Value.JsonType == JsonType.String)
                                                        .ToDictionary (k => k.Key, v => (string) v.Value), 
                                            (Xamarin.Auth.Account) user
                                        );
			return oauthRequest;
		}

		public DescribeRequest ()
		{
			Options = new Dictionary<string,JsonValue> ();
			Headers = new Dictionary<string,string> ();
		}

		public override string ToString ()
		{
			return string.Format (@"{0} {1}", Method, Resource);
		}
	}
}

