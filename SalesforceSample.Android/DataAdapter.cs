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
	public class DataAdapter : BaseAdapter<JsonValue>
	{
		Activity context;
		List<JsonObject> objects = new List<JsonObject> ();

		public DataAdapter(Activity activity, List<JsonObject> items)
			: base()
		{
			context = activity;
			objects = items;
		}

		public List<JsonObject> Objects
		{
			get { return 
				objects; }
			set
			{
				objects = value;

			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}
		public override JsonValue this[int position]
		{
			get { 
				return objects[position]; }
		}
		public override int Count
		{
			get { return objects.Count; }
		}
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var o = (JsonObject) objects[position];
		
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.RootListViewCell, null);
			view.FindViewById<TextView>(Resource.Id.Text1).Text = o["Name"];
			view.FindViewById<TextView>(Resource.Id.Text2).Text =  o["AccountNumber"];

			return view;
		}
	}
}

