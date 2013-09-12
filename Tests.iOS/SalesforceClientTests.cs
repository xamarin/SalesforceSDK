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
		public void Setup ()
		{
			var key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";

			var redirectUrl = new Uri("com.sample.salesforce:/oauth2Callback"); // TODO: Move oauth redirect to constant or config
			var secret = "5754078534436456018";

			Client = new SalesforceClient (key, secret, redirectUrl);


			var users = Client.LoadUsers ();
			ISalesforceUser user;

			if (users.SingleOrDefault() == null)
			{
				user = new SalesforceUser {
					Username = "zack@xamarin.form",					
				};
				user.Properties ["instance_url"] = @"https://na15.salesforce.com/";
				user.Properties ["refresh_token"] = @"5Aep861z80Xevi74eVVu3JCJRUeNrRZAcxky4UcHL1MvM2ALL0djQp.rF2CFYJCWPzjzhYmMv2Ks.RZJGfYsCf3";
				user.Properties ["access_token"] = @"00Di0000000bhO!ARYAQC.1PSh5ZNZGKjtB5H5kMY3QPPiBrEMroBRWJfr3fHlObU7GrYsDshCqpp6Mtt13LD0NP2N00CZ_xP29RYZcSprRF1Rt";

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
	}
}
