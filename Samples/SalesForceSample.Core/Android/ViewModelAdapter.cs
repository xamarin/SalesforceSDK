using System;
using Android.Widget;
using Android.Content;
using Android.Views;

namespace SalesForceSample
{
	public class ViewModelAdapter<T> : BaseAdapter<T>
	{
		Context context;
		ListView listView;

		public ViewModelAdapter (Context context)
		{
//			this.listView = listView;
//			listView.ItemClick += (sender, e) => {
//				model.ItemSelected(this[e.Position]);
//			};
			this.context = context;
			
		}


		public delegate Android.Views.View GetViewEventHandler (object sender, T item, Android.Views.View convertView, Android.Views.ViewGroup parent);
		// the event
		public event GetViewEventHandler ViewForItem;

		int defaultView = Android.Resource.Layout.SimpleListItem1;

		public int DefaultView {
			get {
				return defaultView;
			}
			set {
				defaultView = value;
				this.NotifyDataSetChanged ();
			}
		}

		ViewModel<T> model;

		public ViewModel<T> Model {
			get {
				return model;
			}
			set {
				if (model != null)
					removeModelEvents ();
				model = value;
				addModelEvents ();
			}
		}

		void removeModelEvents ()
		{
			model.ItemsChanged -= HandleItemsChanged;
		}

		void addModelEvents ()
		{
			model.ItemsChanged += HandleItemsChanged;
		}

		void HandleItemsChanged (object sender, EventArgs e)
		{
			this.NotifyDataSetChanged ();
		}

		#region implemented abstract members of BaseAdapter

		public override long GetItemId (int position)
		{
			return defaultView;
		}

		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			if (convertView == null || convertView.Id != defaultView) {
				var inflater = LayoutInflater.FromContext (context);
				convertView = inflater.Inflate (defaultView, null);
			}

			if (ViewForItem != null)
				return ViewForItem (this, this [position], convertView, parent);

			throw new Exception("Event ViewForItem is required");
		}

		public override int Count {
			get {
				if(model == null)
					return 0;
				return model.Rows ();
			}
		}

		#endregion

		#region implemented abstract members of BaseAdapter

		public override T this [int position] {
			get {
				return model.Items [position];
			}
		}

		#endregion
	}
}

