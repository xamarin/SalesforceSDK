using System;
using Android.App;
using Android.OS;
using Android.Views;

using System.Json;
using Salesforce;

using Debug = System.Diagnostics.Debug;

namespace SalesforceSample.Droid
{
	
	[Activity (Label = "Add/Edit Account")]			
	public class EditActivity : ListActivity
	{
		JsonObject data;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here

			var extra = Intent.GetStringExtra ("JsonItem");
			Debug.WriteLine ("extra;" + extra);
			data = (JsonObject)JsonValue.Parse (extra);

			ListAdapter = new EditAdapter (this, (JsonObject)data);
		}

		async void Save () 
		{
			var selectedObject = new SObject (data);

			// Delete the row from the data source.
			var request = new CreateRequest (selectedObject) { Resource = selectedObject } ;

			await RootActivity.Client.ProcessAsync (request).ContinueWith (response => {
				Debug.WriteLine ("save finished.");
				StartActivity (typeof(RootActivity));
			});
		}

		/// <summary>shortcut back to the main screen</summary>
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.Save, menu);
			return true;
		}

		/// <summary>shortcut back to the main screen</summary>
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.save) {
				Save ();
			}
			return true;
		}
	}
}
