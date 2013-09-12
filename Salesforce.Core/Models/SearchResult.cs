using System;
using System.Collections.Generic;
using System.Json;

namespace Salesforce
{
	public class SearchResult
	{
		public string Type { get; set; }
		public string Url { get; set; }
		public string Id { get; set; }

		public SearchResult(JsonValue jsonResults)
		{
			Type = jsonResults ["attributes"] ["type"];
			Url = jsonResults ["attributes"] ["url"];
			Id = jsonResults ["Id"];
		}
	}
}

