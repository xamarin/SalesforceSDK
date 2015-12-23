using System;
using System.Collections.Generic;

namespace Salesforce
{
	public class DeleteFailedException : Exception
	{
		public class FailureReason
		{
			public String Message { get; internal set; }
			public String[] RelatedIds { get; internal set; }
		}

		public IEnumerable<FailureReason> FailureReasons { get; private set; }

		public DeleteFailedException (String message) : base(message)
		{
			FailureReasons = ParseMessage (message);
		}

		IEnumerable<FailureReason> ParseMessage (string message)
		{
			var reasons = message.Split (new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			var results = new FailureReason[reasons.Length];

			for (int i = 0; i < reasons.Length; i++) {
				var reason = reasons [i];
				var listBeginsAtIndex = reason.LastIndexOf (":");
				var explanation = reason.Substring (0, listBeginsAtIndex);
				var listGlob = reason.Trim().Substring (listBeginsAtIndex + 2 /* because there's a space that follows the colon. */);
				var ids = listGlob.Split (new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

				results [i] = new FailureReason { Message = explanation, RelatedIds = ids };
			}

			return results;
		}
	}
}