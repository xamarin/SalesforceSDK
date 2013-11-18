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
	public class SalesforceClientTests
	{
		SalesforceClient Client {
			get;
			set;
		}

		[SetUp]
        public async void Setup ()
		{
            var key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";

            var redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback");
            var secret = "5754078534436456018";

			Client = new SalesforceClient (key, secret, redirectUrl);

            // Use username/password flow for the demo.
            // This ensures we always have a valid
            // access_token for about 6 hours after this
            // method returns.
            var tokenClient = new HttpClient();
            var formData = new Dictionary<string,string> {
                {"grant_type", "password"},
                {"client_id", "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab"},
                {"client_secret", "5754078534436456018"},
                {"username", "demo@xamarin.com"},
                {"password", "white1@needyrVpFxD3PAvjdH8svH7wLXTN98"},
            };
            var content = new FormUrlEncodedContent(formData);
            var responseTask = await tokenClient.PostAsync("https://login.salesforce.com/services/oauth2/token", content);
//            responseTask.RunSynchronously(TaskScheduler.Default);
//            responseTask.Wait();
            var responseReadTask = responseTask.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
//            var rawResult = await response.Content.ReadAsStringAsync();
            responseReadTask.RunSynchronously();
            responseReadTask.Wait();
//            var result = JsonValue.Parse(rawResult);
            var result = JsonValue.Parse(responseReadTask.Result);
            var users = Client.LoadUsers ();
			ISalesforceUser user;

			if (users.SingleOrDefault() == null)
			{
				user = new SalesforceUser {
					Username = "zack@xamarin.form",					
				};
				user.Properties ["instance_url"] = @"https://na15.salesforce.com/";
                //user.Properties ["refresh_token"] = @"5Aep861z80Xevi74eVVu3JCJRUeNrRZAcxky4UcHL1MvM2ALL3Wj_phoRIBXVC2ZcbP_BblUk39RfBF6cwu.lx3";
                user.Properties ["access_token"] = result["access_token"]; //@"00Di0000000bhOg!ARYAQN2uT2p0I.g1t03eAfogW8ZostVE61ZTMkkrOb1eiWADj9vEABhGUqqO05PQNdUA4pq60a3JTPTwyN6Z7blXpZXJbyHX";

				Client.Save (user);
			}
			else
			{
				user = users.FirstOrDefault ();
			}

			Client.CurrentUser = user;
		}

		[Test]
		public void QueryTest ()
		{
			var query = "SELECT Id, Name, AccountNumber, Phone, Website, Industry, LastModifiedDate, SLAExpirationDate__c FROM Account";
			var task = Task.Run (()=> Client.Query (query));

			Assert.That (task, Has.Property ("Status").EqualTo (TaskStatus.RanToCompletion).After (5000, 100).And.Property ("Result").Matches(new Predicate<IEnumerable<SObject>>(o => o.ToArray().Length == 11)));
		}
		
		[Test]
		public void SearchTest ()
		{
			var query = "FIND {John*} IN ALL FIELDS RETURNING Account (Id, Name), Contact, Opportunity, Lead";

			Task<IEnumerable<SearchResult>> task = Task.Run (() => Client.Search (query));

			Assert.That (task, Has.Property ("Status").EqualTo (TaskStatus.RanToCompletion).After (10000, 100).And.Property ("Result").Matches (new Predicate<IEnumerable<SearchResult>> (o => {
				var countIsElevent = o.Count() == 11;
				return countIsElevent;
			})));
		}

		[Test]
		public void DescribeTest ()
		{
			var type = "Opportunity";

			Task<JsonObject> task = Task.Run (() => {return Client.Describe(type);});

			Assert.That (task, Has.Property ("Status").EqualTo (TaskStatus.RanToCompletion).After (10000, 100).And.Property ("Result").Matches (new Predicate<JsonObject> (o => {
				return o != null
					&& o["name"] == "Opportunity"
						&& o["fields"].Count == 39;
			})));
		}
		
		[Test]
		public void DescribeAsyncTest ()
		{
			var type = "Opportunity";

			Task<JsonObject> task = Task.Run (() => {
				var t = Client.DescribeAsync (type);
				t.Wait(SalesforceClient.DefaultNetworkTimeout);
				return t.Result;
			});

			Assert.That (task, Has.Property ("Status").EqualTo (TaskStatus.RanToCompletion).After (10000, 100).And.Property ("Result").Matches (new Predicate<JsonObject> (o => {
				return o != null
					&& o["name"] == "Opportunity"
						&& o["fields"].Count == 39;
			})));
		}

        [Test]
        public void ChangesDefaultsTest ()
        {
            var type = "Opportunity";

            Task<JsonObject> task = Task.Run (() => {return Client.Changes(type, ChangeTypes.Updated, DateTime.Now - TimeSpan.FromDays(15), DateTime.UtcNow);});

            Assert.That (task, Has.Property ("Status").EqualTo (TaskStatus.RanToCompletion).After (10000, 100).And.Property ("Result").Matches (new Predicate<JsonObject> (o => {
                return o != null
                    && o["ids"] != null;
            })));
        }
	}
}
