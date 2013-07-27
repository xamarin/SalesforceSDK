using System;

namespace Salesforce
{
	public class InsufficientRightsException : Exception
	{
		public InsufficientRightsException (String message) : base (message) { }
	}
}

