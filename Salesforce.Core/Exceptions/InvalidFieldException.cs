using System;

namespace Salesforce
{
	public class InvalidFieldException : Exception
	{
		public InvalidFieldException (String message) : base(message)
		{
		}
	}
}

