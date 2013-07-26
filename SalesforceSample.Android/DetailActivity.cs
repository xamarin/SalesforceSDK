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
			Console.WriteLine ("extra;" + extra);
			data = JsonValue.Parse (extra);


			ListAdapter = new DetailAdapter (this, data);


		}


	}
}

