using System;
using System.Collections.Generic;
using Xamarin.Auth;

namespace Salesforce
{

	public class ReadRequest : IAuthenticatedRequest
	{
		public String RequestType { get; set; }

		public ISalesforceResource Resource {	get ; set ; }

		public IDictionary<string, string> Headers { get ; set ; }

		public String Method { get { return HttpMethod.Get; } }

		public IDictionary<string, string> Options { get; private set; }

		public OAuth2Request ToOAuth2Request (ISalesforceUser user)
		{
			var baseUri = new Uri (user.Properties ["instance_url"] + "services/data/");
			var uri = new Uri (baseUri, Resource.AbsoluteUri);
			var oauthRequest = new OAuth2Request (Method, uri, Resource.Options, user);
			return oauthRequest;
		}

		public ReadRequest ()
		{
			Options = new Dictionary<string,string> ();
			Headers = new Dictionary<string,string> ();
		}

		public override string ToString ()
		{
			return string.Format (@"{0} {1}", Method, Resource);
		}
	}
}

