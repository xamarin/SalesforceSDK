using System;
#if __UNIFIED__
using Foundation;
using UIKit;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using System.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;

namespace SalesforceSample.iOS
{
	[Register("ContactRowCell")]
	public partial class ContactRowCell : UITableViewCell
	{
		[Outlet]
		public UILabel Label { get; set; }
		[Outlet]
		public UITextField Value { get; set; }
	}
}
