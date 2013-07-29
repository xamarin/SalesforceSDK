using System;

namespace Salesforce
{
	public class MissingResourceException : Exception
	{
		public MissingResourceException (String message) : base(message)
		{
		}
	}
}

