using System;
using Xamarin.Auth;
using System.Collections.Generic;

namespace Salesforce
{
	/// <summary>
	/// Prevents platform abstractions from leaking.
	/// </summary>
	public interface IPlatformAdapter
	{
		Authenticator Authenticator { get; set;	}
		object GetLoginUI();
		void SaveAccount(ISalesforceUser account);
		IEnumerable<ISalesforceUser> LoadAccounts();
	}

	public struct PlatformStrings
	{
		public static readonly String Salesforce = "Salesforce";
	}

#if PLATFORM_ANDROID
	internal class AndroidPlatformAdapter : IPlatformAdapter
	{
		#region IPlatformAdapter implementation

		public object GetLoginUI ()
		{
			throw new NotImplementedException ();
		}

		public void SaveAccount (ISalesforceUser account)
		{
			AccountStore.Create (this).Save (account, PlatformStrings.Salesforce);
		}

		#endregion

		public AndroidPlatformAdapter ()
		{
		}

		public AndroidPlatformAdapter (Authenticator activator)
		{
			this.Authenticator = activator;
		}

		public IEnumerable<ISalesforceUser> LoadAccounts()
		{
			return AccountStore.Create (this).FindAccountsForService (PlatformStrings.Salesforce);
		}

}
#endif

#if PLATFORM_IOS
	internal class UIKitPlatformAdapter : IPlatformAdapter
	{
		public  Authenticator Authenticator { get; set;	}

		#region IPlatformAdapter implementation

		public UIKitPlatformAdapter() : this(null)
		{ }

		public UIKitPlatformAdapter(Authenticator activator)
		{
			this.Authenticator = activator;
		}

		public object GetLoginUI ()
		{
			return Authenticator.GetUI ();
		}

		public void SaveAccount (ISalesforceUser account)
		{
			AccountStore.Create ().Save (account, PlatformStrings.Salesforce);
		}

		public IEnumerable<ISalesforceUser> LoadAccounts()
		{
			return AccountStore.Create ().FindAccountsForService (PlatformStrings.Salesforce);
		}

		#endregion

	}
#endif
}

