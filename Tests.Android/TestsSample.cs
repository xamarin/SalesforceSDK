using System;
using NUnit.Framework;
using Salesforce;

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

			Client = new SalesforceClient (key, redirectUrl);


		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public void Pass ()
		{
			Console.WriteLine ("test1");
			Assert.True (true);
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

		[Test]
		public void Inconclusive ()
		{
			Assert.Inconclusive ("Inconclusive");
		}
	}
}

