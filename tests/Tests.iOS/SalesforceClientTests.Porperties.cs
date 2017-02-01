using System;
using System.Diagnostics;
using NUnit.Framework;
using Salesforce;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using Xamarin.Auth;
using System.Linq;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;

namespace Tests.iOS
{
	[TestFixture]
	public partial class SalesforceClientTests
	{

		[Test]
        public void TestPropertiesAuthPathAndTokenPath ()
        {
			string path_auth = @"http://xamarin.com/auth";
			string path_token = @"http://xamarin.com/token";

			SalesforceClient.AuthPath = path_auth;
			SalesforceClient.TokenPath = path_token;

			string client_id = "ajde klajent";
			string client_secret = "very very secret";
			Uri uri_redirect =  new Uri("http://holisticware.net");

            SalesforceClient client = new SalesforceClient
            									(
            										client_id,
            										client_secret,
            										uri_redirect
            									);

            Assert.AreNotEqual
            			(
            				SalesforceClient.AuthPath,
							@"https://login.salesforce.com/services/oauth2/authorize"
            			);
			Assert.AreNotEqual
            			(
            				SalesforceClient.AuthPath,
							@"https://login.salesforce.com/services/oauth2/authorize"
            			);

			Assert.AreEqual(SalesforceClient.AuthPath, path_auth);
			Assert.AreEqual(SalesforceClient.TokenPath, path_token);

            return;
        }
	}
}
