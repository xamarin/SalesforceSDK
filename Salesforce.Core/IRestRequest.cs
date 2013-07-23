using System.Collections.Generic;

namespace Salesforce
{
	public interface IRestRequest
	{
		ISalesforceResource Resource { get; }
		string Method { get; }
		IDictionary<string,string> Headers { get; }
	}
}

