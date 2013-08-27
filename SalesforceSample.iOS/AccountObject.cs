using Salesforce;
using System;
using System.Runtime.Serialization;
using System.Collections;

namespace SalesforceSample.iOS
{
	public class AccountObject : SObject
	{
		public AccountObject()
		{
			this.FieldsWithNullValues = new System.Collections.Hashtable ();
			this.PreparingUpdateRequest += PrepareForUpdate;
		}

		public override string ResourceName { get { return "Account"; } set { } }

		public string Name
		{
			get { return GetOption ("Name"); }
			set { SetOption ("Name", value); }
		}

		public string Phone
		{
			get { return GetOption ("Phone"); }
			set { SetOption ("Phone", value); }
		}

		public string Industry
		{
			get { return GetOption ("Industry"); }
			set { SetOption ("Industry", value); }
		}

		public string Website
		{
			get { return GetOption ("Website"); }
			set { SetOption ("Website", value); }
		}

		public string AccountNumber
		{
			get { return GetOption ("AccountNumber"); }
			set { SetOption ("AccountNumber", value); }
		}

		public DateTime? LastModified
		{
			get { 
				var val = GetOption ("LastModifiedDate");
				return !String.IsNullOrWhiteSpace(val) ? DateTime.Parse(val) : default(DateTime?) ; 
			}
			set { 
				throw new InvalidOperationException ("LastModifiedDate is a read-only field."); 
			}
		}

		public DateTime? AssignedOn
		{
			get { 
				var val = GetOption ("Assigned__c");
				return !String.IsNullOrWhiteSpace(val) ? DateTime.Parse(val) : default(DateTime?) ; 
			}
			set {
				if (value.HasValue) {
					SetOption ("Assigned__c", value.Value.ToString ("o"));
				} else
					SetOption ("Assigned__c", String.Empty); // Effectively resets the field to blank.

			}
		}

		public void PrepareForUpdate(object sender, EventArgs args)
		{
			Options.Remove ("LastModifiedDate"); // Prevents attempting to update a read-only field.
		}
	}
}