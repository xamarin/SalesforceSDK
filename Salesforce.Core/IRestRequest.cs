using System;
using Xamarin.Auth;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;

namespace Salesforce
{
	public interface IRestRequest
	{
		string Method { get; }
		Uri AbsoluteUri { get; }
		IDictionary<string, string> Options { get; }
	}
}

