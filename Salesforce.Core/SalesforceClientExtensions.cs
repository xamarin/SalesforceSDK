using System;
using Xamarin.Auth;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Salesforce
{
	public static class SalesforceClientExtensions
	{
		public static async Task<string> CreateAsync (this SalesforceClient self, SObject sobject)
		{
			var createRequest = new CreateRequest (sobject);
			Response result;
			try {
				result = await self.ProcessAsync (createRequest).ConfigureAwait (true);
			} catch (AggregateException ex) {
				Debug.WriteLine (ex.Message);
				return null;
			}
			var json = result.GetResponseText ();
			var jsonValue = JsonValue.Parse (json);
			if (jsonValue == null)
				return null;
			sobject.Id = jsonValue["id"];
			return sobject.Id;
		}

		public static string Create (this SalesforceClient self, SObject sobject)
		{
			var createRequest = new CreateRequest (sobject);
			var result = self.ProcessAsync (createRequest);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout)))
				return null;

			if (result.IsFaulted)
				return null; // TODO: Do error reporting

			var json = result.Result.GetResponseText ();
			var jsonValue = JsonValue.Parse (json);
			if (jsonValue == null)
				return null;
			sobject.Id = jsonValue["id"];
			return sobject.Id;
		}

		public static async Task UpdateAsync (this SalesforceClient self, SObject sobject)
		{
			var updateRequest = new UpdateRequest (sobject);
			try {
				await self.ProcessAsync (updateRequest).ConfigureAwait (true);
			} catch (AggregateException ex) {
				Debug.WriteLine (ex.Message);
			}
		}

		public static void Update (this SalesforceClient self, SObject sobject)
		{
			var updateRequest = new UpdateRequest (sobject);
			var result = self.ProcessAsync (updateRequest);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout)))
				return; // TODO : Error handling/reporting
		}

		public static IEnumerable<SObject> Search (this SalesforceClient self, string search)
		{
			var result = self.SearchAsync (search);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout))) {
				Debug.WriteLine ("Request timed out");
				return Enumerable.Empty<SObject> ();
			}

			return result.Result;
		}

		public static Task<IEnumerable<SObject>> SearchAsync (this SalesforceClient self, string search)
		{
			return self.ReadAsync (new ReadRequest {Resource = new Search {QueryText = search}});
		}

		public static Task<IEnumerable<SObject>> QueryAsync (this SalesforceClient self, string query)
		{
			return self.ReadAsync (new ReadRequest {Resource = new Query {Statement = query}});
		}

		public static async Task<IEnumerable<SObject>> ReadAsync (this SalesforceClient self, ReadRequest request)
		{
			Response response;

			try {
				response = await self.ProcessAsync (request);
			} catch (AggregateException ex) {
				throw ex.Flatten ().InnerException;
			}

			if (response == null) {
				return Enumerable.Empty<SObject> ();
			}

			var result = response.GetResponseText ();
			var jsonValue = JsonValue.Parse (result);

			if (jsonValue == null)
				throw new Exception ("Could not parse Json data");

			var results = jsonValue["records"];
			return results.OfType<JsonObject> ().Select (j => new SObject (j));
		}

		public static IEnumerable<SObject> Read (this SalesforceClient self, ReadRequest request)
		{
			var result = self.ReadAsync (request);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout))) {
				Debug.WriteLine ("Request timed out");
				return Enumerable.Empty<SObject> ();
			}

			return result.Result;
		}

		public static IEnumerable<SObject> Query (this SalesforceClient self, string query)
		{
			var result = self.QueryAsync (query);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout))) {
				Debug.WriteLine ("Request timed out");
				return Enumerable.Empty<SObject> ();
			}

			return result.Result;
		}

		public static async Task<bool> DeleteAsync (this SalesforceClient self, SObject sobject)
		{
			// Delete the row from the data source.
			var request = new DeleteRequest (sobject);
			var response = await self.ProcessAsync (request);
			return response.StatusCode == System.Net.HttpStatusCode.NoContent;
		}

		public static bool Delete (this SalesforceClient self, SObject sobject)
		{
			var result = self.DeleteAsync (sobject);
			if (!result.Wait (TimeSpan.FromSeconds (SalesforceClient.DefaultNetworkTimeout))) {
				Debug.WriteLine ("Request timed out");
				return false;
			}
			return result.Result;
		}
	}
	
}
