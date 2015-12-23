using System;
using System.Collections.Generic;

namespace Salesforce
{
	public class InvalidFieldException : Exception
	{
		private readonly static IEnumerable<string> IdFieldset = new[] { "Id" };

		public IEnumerable<String> Fields { get; private set; }

		public InvalidFieldException (String message) : base(message)
		{
			Fields = IdFieldset;
		}

		public InvalidFieldException (String message, IEnumerable<String> fields) : base(message)
		{
			Fields = fields;
		}
	}
}

