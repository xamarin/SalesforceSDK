using System;

namespace Salesforce
{
	/// <summary>
	/// Invalid client identifier exception.
	/// </summary>
	/// <remarks>
	/// Maps to the JSON error object {"error":"invalid_client_id","error_description":"client identifier invalid"}
	/// </remarks>
	public class InvalidClientIdException : Exception
	{
		public String ClientId { get; set; }
		public InvalidClientIdException (String message, String clientId) : base (message) 
		{
			ClientId = clientId;
		}
	}
}

