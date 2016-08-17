using System;
using System.Collections.Generic;
using Xamarin.Auth;
using System.Json;
using System.Linq;
using System.IO;
using System.Threading;

namespace Salesforce
{
    public class ChangesRequest : IAuthenticatedRequest
    {
        public ChangeTypes ChangeType { get; set; }

        public DateTime Since { get; set; }

        public DateTime Until { get; set; }

        public ISalesforceResource Resource {   get ; set ; }

        public IDictionary<string, string> Headers { get ; set ; }

        public String Method { get { return HttpMethod.Get; } }

        public IDictionary<string, JsonValue> Options { get; private set; }

        public OAuth2Request ToOAuth2Request (ISalesforceUser user)
        {
            if (!(Resource is SObject))
                throw new InvalidOperationException ("Only SObjects can have changes. Searches and Queries not elibible.");

            if (Since > Until)
                throw new InvalidOperationException ("Since must preceed Until.");

//            var path = user.Properties ["instance_url"] + SalesforceClient.RestApiPath;
//            var baseUri = new Uri (path);
//            var queryString = String.Format("?start={0:O}&end={1:O}", Since.ToUniversalTime(), Until.ToUniversalTime());
//            var changesPath = Path.Combine(Resource.AbsoluteUri.AbsolutePath, ChangeType.ToString(), queryString);
//            var changesUri = new Uri(changesPath);
//            var uri = new Uri (baseUri, changesUri);
            var path = user.Properties ["instance_url"] + SalesforceClient.RestApiPath;
            var baseUri = new Uri (path);
            var uri = new UriBuilder(new Uri (baseUri, Resource.AbsoluteUri));
            // Custom ISO format:
            var since = Since.ToUniversalTime();
            var sinceString = String.Format("{0}T{1}Z", since.ToString("yyyy-MM-dd"), since.ToString("HH:mm:ss"));
            var until = Until.ToUniversalTime();
            var untilString = String.Format("{0}T{1}Z", until.ToString("yyyy-MM-dd"), until.ToString("HH:mm:ss"));
            uri.Query = String.Format("start={0}&end={1}", sinceString, untilString);
            var oauthRequest = new OAuth2Request (this.Method, uri.Uri, this.Resource.Options.Where (kvp => kvp.Value.JsonType == JsonType.String).ToDictionary (k => k.Key, v => (string) v.Value),Headers, user);
            return oauthRequest;
        }

        public ChangesRequest ()
        {
            Options = new Dictionary<string,JsonValue> ();
            Headers = new Dictionary<string,string> ();
            Since = DateTime.MinValue;
            Until = DateTime.Now;
        }

        public override string ToString ()
        {
            return string.Format (@"{0} {1}", Method, Resource);
        }
    }
}

