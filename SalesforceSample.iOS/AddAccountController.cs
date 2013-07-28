using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Salesforce;
using Xamarin.Auth;
using System.Linq;
using System.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;

namespace SalesforceSample.iOS
{
	public partial class AddAccountController : UIViewController
	{
		public Action Finished { get; set; }

		public AddAccountController() : base ("AddAccountController", null)
		{
			TextFieldDelegate = new InputTextFieldDelegate ();
		}

		public class InputTextFieldDelegate : UITextFieldDelegate
		{
			public override bool ShouldReturn (UITextField textField)
			{
				textField.ResignFirstResponder ();
				return false;
			}
		}

		public UITextFieldDelegate TextFieldDelegate { get; set; }

		public string Name { get; set; }
		public string Phone { get; set; }
		public string Industry { get; set; }
		public string AccountNumber { get; set; }
		public string Website { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			var view = View;
			var scrollView = new UIScrollView (
				new RectangleF (0, 0, View.Frame.Width, View.Frame.Height)
				);
			View = scrollView;
			scrollView.ContentSize = view.Frame.Size;
			scrollView.Add (view);
		}

		public override bool DisablesAutomaticKeyboardDismissal
		{
			get { return false; }
		}


		partial void NameChanged (NSObject sender)
		{
			Name = sender.AsInput().Text;
		}

		partial void PhoneChanged (NSObject sender)
		{
			Phone = sender.AsInput().Text;
		}

		partial void AccountNumberChanged (NSObject sender)
		{
			AccountNumber = sender.AsInput().Text;
		}

		partial void IndustryChanged (NSObject sender)
		{
			Industry = sender.AsInput().Text;
		}

		partial void WebsiteChanged (NSObject sender)
		{
			Website = sender.AsInput().Text;
		}

		partial void DoneClicked (NSObject sender)
		{
			var ev = Finished;
			if (ev != null)
			{
				ev();
			}
		}
	}

	public static class SenderUtil
	{
		public static UITextField AsInput(this NSObject sender)
		{
			return (UITextField)sender;
		}
	}
}
