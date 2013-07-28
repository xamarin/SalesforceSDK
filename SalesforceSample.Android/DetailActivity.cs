using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Json;
using Salesforce;

namespace SalesforceSample.Droid
{
	[Activity (Label = "Account Details")]			
	public class DetailActivity : ListActivity
	{
		JsonValue data;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here

			var extra = Intent.GetStringExtra ("JsonItem");
			System.Diagnostics.Debug.WriteLine ("extra;" + extra);
			data = JsonValue.Parse (extra);


			ListAdapter = new DetailAdapter (this, data);


		}


		async void Delete () 
		{
			var selectedObject = new SObject (data as JsonObject);

			// Delete the row from the data source.
//			var request = new DeleteRequest (selectedObject) {Resource = selectedObject};
//
//			await RootActivity.Client.ProcessAsync (request);
		}

		/// <summary>shortcut back to the main screen</summary>
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.Delete, menu);
			return true;
		}
		/// <summary>shortcut back to the main screen</summary>
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.delete) {
				StartActivity (typeof(RootActivity));

			}
			return true;
		}
	}
}

