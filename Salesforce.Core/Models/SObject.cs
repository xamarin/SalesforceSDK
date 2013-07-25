using System;
using System.Collections.Generic;
using System.Text;

namespace Salesforce
{
	public class SObject : ISalesforceResource
	{
		private static readonly string Format = "{0}/";

		#region ISalesforceResource implementation

		string ISalesforceResource.ResourceType {
			get {
				return "sobjects";
			}
		}

		public string Id {
			get ;
			private set;
		}

		public IDictionary<string, string> Options {
			get ;
			protected set ;
		}

		#endregion

		#region IVersionableRestResource implementation

		public string Version {
			get { return "v28.0"; }
		}

		#endregion

		#region IRestResource implementation

		public string ResourceName { get ; set; }

		public Uri AbsoluteUri {
			get {
				return new Uri (ToUriString (), UriKind.RelativeOrAbsolute);
			}
		}

		#endregion

		protected virtual string ToUriString()
		{
			var self = (ISalesforceResource)this;
			var str = new StringBuilder ();

			if (!String.IsNullOrWhiteSpace (self.Version))
				str.AppendFormat (Format, self.Version);
			else
				return String.Empty;

			if (!String.IsNullOrWhiteSpace (self.ResourceType))
				str.AppendFormat (Format, self.ResourceType);

			if (!String.IsNullOrWhiteSpace (this.ResourceName))
				str.AppendFormat (Format, this.ResourceName);

			if (!String.IsNullOrWhiteSpace (self.Id))
				str.AppendFormat (Format, self.Id);

			return str.ToString ();
		}

		public override string ToString ()
		{
			return ToUriString ();
		}
	}
}

