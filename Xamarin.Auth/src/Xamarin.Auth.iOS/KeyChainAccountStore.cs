//
//  Copyright 2012, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Linq;
#if __UNIFIED__
using Security;
using Foundation;
#else
using MonoTouch.Security;
using MonoTouch.Foundation;
#endif

namespace Xamarin.Auth
{
	internal class KeyChainAccountStore : AccountStore
	{
		public override IEnumerable<ISalesforceUser> FindAccountsForService (string serviceId)
		{
			var query = new SecRecord (SecKind.GenericPassword);
			query.Service = serviceId;

			SecStatusCode result;
			var records = SecKeyChain.QueryAsRecord (query, 1000, out result);

			return records != null ?
				records.Select (GetAccountFromRecord).ToList () :
				new List<ISalesforceUser> ();
		}

		ISalesforceUser GetAccountFromRecord (SecRecord r)
		{
			var serializedData = NSString.FromData (r.Generic, NSStringEncoding.UTF8);
			return SalesforceUser.Deserialize (serializedData);
		}

		ISalesforceUser FindAccount (string username, string serviceId)
		{
			var query = new SecRecord (SecKind.GenericPassword);
			query.Service = serviceId;
			query.Account = username;

			SecStatusCode result;
			var record = SecKeyChain.QueryAsRecord (query, out result);

			return record != null ?	GetAccountFromRecord (record) : null;
		}

		public override void Save (ISalesforceUser account, string serviceId)
		{
			var statusCode = SecStatusCode.Success;
			var serializedAccount = account.Serialize ();
			var data = NSData.FromString (serializedAccount, NSStringEncoding.UTF8);

			//
			// Remove any existing record
			//
			var existing = FindAccount (account.Username, serviceId);

			if (existing != null) {
				var query = new SecRecord (SecKind.GenericPassword);
				query.Service = serviceId;
				query.Account = account.Username;

				statusCode = SecKeyChain.Remove (query);
				if (statusCode != SecStatusCode.Success) {
					throw new Exception ("Could not save account to KeyChain: " + statusCode);
				}
			}

			//
			// Add this record
			//
			var record = new SecRecord (SecKind.GenericPassword);
			record.Service = serviceId;
			record.Account = account.Username;
			record.Generic = data;
			record.Accessible = SecAccessible.WhenUnlocked;

			statusCode = SecKeyChain.Add (record);

			if (statusCode != SecStatusCode.Success) {
				throw new Exception ("Could not save account to KeyChain: " + statusCode);
			}
		}

		public override void Delete (ISalesforceUser account, string serviceId)
		{
			var query = new SecRecord (SecKind.GenericPassword);
			query.Service = serviceId;
			query.Account = account.Username;
			
			var statusCode = SecKeyChain.Remove (query);

			if (statusCode != SecStatusCode.Success) {
				throw new Exception ("Could not delete account from KeyChain: " + statusCode);
			}
		}
	}
}

