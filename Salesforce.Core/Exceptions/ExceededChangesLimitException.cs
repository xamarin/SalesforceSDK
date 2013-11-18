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
	public class ExceededChangesLimitException : Exception
	{
        public ExceededChangesLimitException(String message) : base(message) { }
	}
}

