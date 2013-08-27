using System;
using System.Collections.Generic;

namespace Salesforce
{
	public class InvalidFieldException : Exception
	{
		public IEnumerable<String> Fields { get; private set; }

		public InvalidFieldException (String message) : base(message)
		{
			Fields = new[] { "Id" };
		}

		public InvalidFieldException (String message, IEnumerable<String> fields) : base(message)
		{
			Fields = fields;
		}
	}
}

