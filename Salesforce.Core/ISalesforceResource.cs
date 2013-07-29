using System;
using System.Collections.Generic;
using System.Json;

namespace Salesforce
{
	public interface ISalesforceResource : IVersionableRestResource
	{
		string ResourceType { get; }
		string Id { get; }
		IDictionary<string, JsonValue> Options { get; }
	}
}

