using System;

namespace Salesforce
{
	public interface IRestResource
	{
		string ResourceName { get; }
		Uri AbsoluteUri { get; }
	}
}

