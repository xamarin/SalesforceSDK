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
		object GetLoginUI();
		void SaveAccount(IAccount account);
		IEnumerable<IAccount> LoadAccounts();
	}

	public struct PlatformStrings
	{
		public static readonly String Salesforce = "Salesforce";
	}

#if PLATFORM_ANDROID
	public class AndroidPlatformAdapter : IPlatformAdapter
	{
		#region IPlatformAdapter implementation

		public object GetLoginUI ()
		{
			throw new NotImplementedException ();
		}

		public void SaveAccount (IAccount account)
		{
			AccountStore.Create (this).Save (account, PlatformStrings.Salesforce);
		}

		#endregion

		public AndroidPlatformAdapter ()
		{
		}
	}
#endif

#if PLATFORM_IOS
	public class UIKitPlatformAdapter : IPlatformAdapter
	{
		Authenticator Authenticator { get; set;	}

		#region IPlatformAdapter implementation

		public UIKitPlatformAdapter(Authenticator activator)
		{
			this.Authenticator = activator;
		}

		public object GetLoginUI ()
		{
			return Authenticator.GetUI ();
		}

		public void SaveAccount (IAccount account)
		{
			AccountStore.Create ().Save (account, PlatformStrings.Salesforce);
		}

		public IEnumerable<IAccount> LoadAccounts()
		{
			return AccountStore.Create ().FindAccountsForService (PlatformStrings.Salesforce);
		}

		#endregion

		public UIKitPlatformAdapter ()
		{
		}
	}
#endif
}

