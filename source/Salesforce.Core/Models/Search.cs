using System;
using System.IO;
using System.Json;
using System.Text;

//mc++ using System.Web;

using Xamarin.Utilities;
using System.Collections.Generic;

namespace Salesforce
{
	public class Search : ISalesforceResource
	{
		private static readonly string Format = "{0}/";

		#region ISalesforceResource implementation

		string ISalesforceResource.ResourceType {
			get {
				return "search";
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
		public string QueryText 
		{ 
			get { return Options ["q"]; } 
			set { Options ["q"] = value; } 
		}

		public Search()
		{
			Options = new Dictionary<string, JsonValue> ();
		}

		public override string ToString ()
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

			if (Options.Count > 0)
			{
				str.Append ("?");
				foreach (var option in Options)
				{
					str.AppendFormat
                        (
                           "{0}={1}&",
                           //------------------------------------------------------------
                           // mc++ PCL version
                           // HttpUtility.UrlEncode(option.Key), 
                           Uri.EscapeUriString(option.Key),
                           // HttpUtility.UrlEncode(option.Value)
                           Uri.EscapeUriString(option.Value)
                           //------------------------------------------------------------
                        );
				}
				str.Remove (str.Length - 1, 1); // Remove the trailing ampersand.
			}

			return str.ToString ();
		}
	}
}

