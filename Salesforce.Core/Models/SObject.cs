using System;
using System.Collections.Generic;
using System.Text;
using System.Json;
using System.Linq;

namespace Salesforce
{
	public class SObject : ISalesforceResource
	{
		private static readonly string Format = "{0}/";
		string id;
		IDictionary<string, JsonValue> options;
		string resourceName;
		bool constructedFromJson;
		SObject innerObject;

		public static SObject Parse (string jsonText)
		{
			try {
				var jsonValue = JsonObject.Parse (jsonText);
				return new SObject ((JsonObject) jsonValue);
			} catch {
				throw new ArgumentException ("Could not parse passed input text into JsonObject");
			}
		}

		#region ISalesforceResource implementation

		string ISalesforceResource.ResourceType {
			get {
				return "sobjects";
			}
		}

		public string Id
		{
			get { return innerObject == null ? id : innerObject.Id; }
			set
			{
				if (innerObject == null)
					id = value;
				else
					innerObject.Id = value;
			}
		}

		public IDictionary<string, JsonValue> Options
		{
			get { return innerObject == null ? options : innerObject.Options; }
			protected set { options = value; }
		}

		#endregion

		#region IVersionableRestResource implementation

		public string Version {
			get { return "v28.0"; }
		}

		#endregion

		#region IRestResource implementation

		public virtual string ResourceName
		{
			get { return innerObject == null ? resourceName : innerObject.ResourceName; }
			set
			{
				if (innerObject == null)
					resourceName = value;
				else
					innerObject.ResourceName = value;
			}
		}

		public Uri AbsoluteUri { get { return new Uri (ToUriString (), UriKind.RelativeOrAbsolute); } }

		#endregion

		public SObject() : this(null) { }

		public SObject(JsonObject restObject)
		{
			if (restObject == null) {
				Options = new Dictionary<string, JsonValue> ();
			} else {
				constructedFromJson = true;
				Options = restObject.Where (o => o.Key != "attributes").ToDictionary(k => k.Key, v => v.Value);
				ResourceName = restObject["attributes"]["type"];
				Id = restObject["Id"];
				if (Options.ContainsKey ("Id"))
					Options.Remove ("Id");
			}
		}

		protected T GetOption<T> (string key, T @default, Func<JsonValue, T> convertFunc)
		{
			if (convertFunc == null)
				throw new ArgumentNullException("convertFunc");

			if (!Options.ContainsKey (key)) {
				return @default;
			}
			var obj = Options[key];
			return convertFunc (obj);
		}

		protected string GetOption (string key, string @default = "")
		{
			if (!Options.ContainsKey (key)) {
				return @default;
			}

			var result = Options[key];
			if (result != null && result.JsonType == JsonType.String)
				return result;
			return @default;
		}

		protected void SetOption<T> (string key, T value, Func<T, JsonValue> convertFunc = null)
		{
			if (convertFunc == null)
				Options[key] = value.ToString ();
			else
				Options[key] = convertFunc (value);
		}

		void SetInner (SObject inner)
		{
			if (constructedFromJson)
				throw new InvalidOperationException ("Can't proxy objects constructed from json directly");
			innerObject = inner;
		}

		string ToUriString()
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

		public T As<T> ()
			where T : SObject, new()
		{
			var result = new T ();
			result.SetInner (this);

			return result;
		}
	}
}

