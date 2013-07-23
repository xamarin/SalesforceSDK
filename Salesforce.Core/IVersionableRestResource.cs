using System;

namespace Salesforce
{
	public interface IVersionableRestResource : IRestResource
	{
		string Version { get; }
	}
}

