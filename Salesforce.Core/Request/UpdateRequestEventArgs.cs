using System;
using System.Collections.Generic;
using System.Text;
using System.Json;
using System.Linq;

namespace Salesforce
{
	public class UpdateRequestEventArgs : EventArgs
	{
		public IDictionary<string,string> UpdateData { get; private set; }

		public UpdateRequestEventArgs(IDictionary<string,string> updateData)
		{
			UpdateData = updateData;
		}
	}
}

