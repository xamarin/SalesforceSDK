using System;
using System.Collections.Generic;

namespace Salesforce
{
	public interface ISalesforceResource : IVersionableRestResource
	{
		string ResourceType { get; }
		string Id { get; }
		IDictionary<string, string> Options { get; }
	}
}

