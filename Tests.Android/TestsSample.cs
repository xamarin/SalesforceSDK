using System;
using NUnit.Framework;
using Salesforce;
using System.Linq;
using Xamarin.Auth;
using System.Diagnostics;

namespace Tests.Android
{
	[TestFixture]
	public class TestsSample
	{
		SalesforceClient Client { get; set; }

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
				user.Properties ["access_token"] = @"00Di0000000bhOg!ARYAQLWetbW6H_Lw78K0SlJ3IU7bBCeOMEhtlP8hTvaWALsYNuxfkikbC5tbAfgdNvxjSkZJ6wHVr8A5qIKM7.KeBmGnoIlg";

				Client.Save (user);
			}
			else
			{
				user = users.FirstOrDefault ();
			}

			Client.CurrentUser = user;
		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public async void Pass ()
		{
			var json = "{{\"attributes\": {\"type\": \"Account\", \"url\": \"/services/data/v28.0/sobjects/Account/001i000000Jss8EAAR\"}, \"Id\": \"001i000000Jss8EAAR\", \"Name\": \"Joe's Hostel\", \"AccountNumber\": \"FDFGGDFG345\", \"Phone\": \"415-555-1212\", \"Website\": \"http://www.foofoo.com\", \"Industry\": \"Hotels\"}}";

			var account = new SObject { Id = "001i000000Jss8EAAR", ResourceName = "Account" };
			account.Options.Add("Website", "http://hostilehostel.com");
			var request = new UpdateRequest {
//				Resource = new Search { QueryText = "FIND {John}" }
//				Resource = new Query { Statement = "SELECT Id, Name, AccountNumber FROM Account" }
				Resource = account
			};

			var response = await Client.ProcessAsync<UpdateRequest> (request);
			var result = response.GetResponseText ();

			var results = System.Json.JsonValue.Parse(result)["records"];

			foreach(var r in results)
			{
				Debug.WriteLine (r);
			}
			Assert.True (true);
		}
	}
}

