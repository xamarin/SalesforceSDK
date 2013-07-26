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
	public class DetailAdapter : BaseAdapter<Tuple<string,string>>
	{
		Activity context;
		JsonValue data;

		public DetailAdapter(Activity activity, JsonValue item)
			: base()
		{
			context = activity;
			data = item;
		}

		public JsonValue Data
		{
			get { return data; }
			set
			{
				if (data == value)
					return;
				data = value;
				// refresh ((ListActivity)context).ListView.
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}
		public override Tuple<string,string> this[int position]
		{
			get { 
				//var key = 
				return new Tuple<string,string> ("",""); }//datapairs[position]; 
		}
		public override int Count
		{
			get { return data.Count; }
		}
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			Data = (JsonObject)data;

			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.DetailListViewCell, null);

			// clear field
			view.FindViewById<TextView>(Resource.Id.Text1).Text = "";
			view.FindViewById<TextView>(Resource.Id.Text2).Text = "";

			switch (position) {
				case 0:
				view.FindViewById<TextView>(Resource.Id.Text1).Text = "Name";
				view.FindViewById<TextView>(Resource.Id.Text2).Text =  Data["Name"];
				break;
				case 1:
				view.FindViewById<TextView>(Resource.Id.Text1).Text = "Industry";
				view.FindViewById<TextView>(Resource.Id.Text2).Text =  Data["Industry"];
				break;
				case 2:
				view.FindViewById<TextView>(Resource.Id.Text1).Text = "Phone";
				view.FindViewById<TextView>(Resource.Id.Text2).Text =  Data["Phone"];
				break;
				case 3:
				view.FindViewById<TextView>(Resource.Id.Text1).Text = "Website";
				view.FindViewById<TextView>(Resource.Id.Text2).Text =  Data["Website"];
				break;
				case 4:
				view.FindViewById<TextView>(Resource.Id.Text1).Text = "Account Number";
				view.FindViewById<TextView>(Resource.Id.Text2).Text =  Data["AccountNumber"];
				break;
			}

			return view;
		}
	}
}