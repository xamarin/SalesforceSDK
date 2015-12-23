using System;
using System.IO;
using System.Json;
using System.Text;

using System.Web;
using System.Web.Util;

using Xamarin.Utilities;
using System.Collections.Generic;

namespace Salesforce
{
	public class Query : ISalesforceResource
	{
		private static readonly string Format = "{0}/";

		#region ISalesforceResource implementation

		string ISalesforceResource.ResourceType {
			get {
				return "query";
			}
		}

		public string Id {
			get ;
			private set;
		}

		public IDictionary<string, JsonValue> Options {
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

		public string ResourceName {
			get ;
			set;
		}

		public Uri AbsoluteUri {
			get {
				return new Uri (ToString (), UriKind.RelativeOrAbsolute);
			}
		}

		#endregion

		/// <summary>
		/// The SOQL query statement.
		/// </summary>
		/// <value>SOQL query text.</value>
		public string Statement 
		{ 
			get { return Options ["q"]; } 
			set { Options ["q"] = value; } 
		}

		public Query()
		{
			Options = new Dictionary<string, JsonValue> ();
		}

		public override string ToString ()
		{
			var self = this;
			var str = new StringBuilder ();

			if (!String.IsNullOrWhiteSpace (self.Version))
				str.AppendFormat (Format, self.Version);
			else
				return String.Empty;

			if (!String.IsNullOrWhiteSpace (((ISalesforceResource)self).ResourceType))
				str.AppendFormat (Format, ((ISalesforceResource)self).ResourceType);

			if (!String.IsNullOrWhiteSpace (self.ResourceName))
				str.AppendFormat (Format, self.ResourceName);

			if (!String.IsNullOrWhiteSpace (self.Id))
				str.AppendFormat (Format, self.Id);

			if (Options.Count > 0)
			{
				str.Append ("?");
				foreach (var option in Options)
				{
					str.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(option.Key), HttpUtility.UrlEncode(option.Value));
				}
				str.Remove (str.Length - 1, 1); // Remove the trailing ampersand.
			}

			return str.ToString ();
		}
	}
}

