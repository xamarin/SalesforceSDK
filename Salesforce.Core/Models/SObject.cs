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
				var jsonValue = JsonValue.Parse (jsonText);
				return new SObject ((JsonObject) jsonValue);
			} catch {
				throw new ArgumentException ("Could not parse passed input text into JsonObject");
			}
		}

		/// <summary>
		/// Allow pre-processing before an UpdateRequest is sent.
		/// </summary>
		public event EventHandler<UpdateRequestEventArgs> PreparingUpdateRequest;

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
            get { return "v29.0"; }
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

		internal IDictionary<string,string> OnPreparingUpdateRequest()
		{
			var evt = PreparingUpdateRequest;
			IDictionary<string,string> opts = null;
			if (evt != null)
			{
				opts = Options.ToDictionary (k => k.Key, v => (String)v.Value);
				evt (this, new UpdateRequestEventArgs (opts));
			}
			return opts;
		}

		protected T GetOption<T> (string key, T defaultValue, Func<JsonValue, T> convertFunc)
		{
			if (convertFunc == null)
				throw new ArgumentNullException("convertFunc");

			if (!Options.ContainsKey (key)) {
				return defaultValue;
			}
			var obj = Options[key];
			return convertFunc (obj);
		}

		protected JsonValue GetOption (string key, string defaultValue = "")
		{
			if (!Options.ContainsKey (key)) {
				return defaultValue;
			}

			var result = Options[key];
			if (result != null && result.JsonType == JsonType.String)
				return result;
			else if (result == null)
				return defaultValue;
			return result;
		}

		protected void SetOption<T> (string key, T value, Func<T, JsonValue> convertFunc = null)
		{
            if (value == null) // TODO: Compare to default(T) instead.
				Options.Remove (key);
			else if (convertFunc == null)
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

