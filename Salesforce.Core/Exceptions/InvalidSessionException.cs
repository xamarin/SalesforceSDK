using System;

namespace Salesforce
{
	public class InvalidSessionException : Exception 
	{
		public InvalidSessionException(string message) : base(message) { }
	}
}

