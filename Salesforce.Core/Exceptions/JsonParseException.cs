using System;

namespace Salesforce
{
	public class JsonParseException : Exception
	{
		public JsonParseException (String message) : base(message)
		{
		}
	}
}

