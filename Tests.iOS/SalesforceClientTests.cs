using System;
using System.Diagnostics;
using NUnit.Framework;
using Salesforce;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace Tests.iOS
{
	[TestFixture]
	public class SalesforceClientTests
	{
		[Test]
		public void Pass ()
		{
			var passed = false;

			var key = "3MVG9A2kN3Bn17hueOTBLV6amupuqyVHycNQ43Q4pIHuDhYcP0gUA0zxwtLPCcnDlOKy0gopxQ4dA6BcNWLab";
			var secret = "5754078534436456018";
			var callback = new Uri ("sfdc://success");

			var authMan = new SalesforceClient (key, secret, callback);

			authMan.AuthenticationComplete += (sender, e) => {
				if (e.IsAuthenticated){
					// Invoke completion handler.
					Debug.WriteLine("Auth success: " + e.Account.Username);
					passed = true;
				}
			};

			try 
			{
				var obj = authMan.GetLoginInterface();
				Assert.NotNull(obj);
			} catch (Exception ex) {
				Debug.WriteLine (ex.Message);
			}

			Debug.WriteLine (passed);
			Assert.That (() => passed, new DelayedConstraint(new PredicateConstraint<bool>((o) => passed), 10000));

		}

		[Test]
		public void Fail ()
		{
			Assert.False (true);
		}

		[Test]
		[Ignore ("another time")]
		public void Ignore ()
		{
			Assert.True (false);
		}
	}
}
