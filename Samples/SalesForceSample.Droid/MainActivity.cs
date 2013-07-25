using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SalesForceSample.Droid
{
	[Activity (Label = "SalesForceSample.Droid", MainLauncher = true)]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			SalesForceService.Shared.ShowLoginScreen += ShowLoginScreen;
			SalesForceService.Shared.LoggedIn += LoggedIn;
			SalesForceService.Shared.Loaded ();

		}

		void ShowLoginScreen (object sender, EventArgs<Salesforce.SalesforceClient> e)
		{
			//window.RootViewController.PresentViewController (e.Data.GetLoginInterface () as UIViewController, true, null);
		}

		void LoggedIn (object sender, EventArgs<Xamarin.Auth.ISalesforceUser> e)
		{
			window.RootViewController.DismissViewController (true, null);
		}


	}
}


