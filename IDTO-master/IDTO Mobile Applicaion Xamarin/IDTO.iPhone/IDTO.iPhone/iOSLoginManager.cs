using System;

using IDTO.Common;
using Xamarin.Auth;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.WindowsAzure.MobileServices;

namespace IDTO.iPhone
{
	public class iOSLoginManager : LoginManager
	{
		private static iOSLoginManager instance;


		private iOSLoginManager ():base ()
		{
			CurrentPlatform.Init ();
		}

		public static iOSLoginManager Instance
		{
			get
			{
				if (instance == null) {
					instance = new iOSLoginManager ();
				}

				return instance;
			}
		}

		protected override void LoadCredentials()
		{
//			try{
				AccountStore acStore = AccountStore.Create();
				IEnumerable<Account> accounts = acStore.FindAccountsForService(SERVICE_ID);
				if (accounts.Count() > 0)
				{
					Account account = accounts.First();

					String userid = account.Properties[USER_ID_KEY];
					String usertoken = account.Properties[USER_TOKEN_KEY];

					MobileServiceUser user = new MobileServiceUser(userid);
					user.MobileServiceAuthenticationToken = usertoken;
					MobileService.CurrentUser = user;
					MobileService.UserName = account.Username;

				}

				base.LoadCredentials();
//			}catch(Exception ex){
//				this.Logout ();
//			}
		}

		public override int GetTravelerId()
		{
			AccountStore acStore = AccountStore.Create();
			IEnumerable<Account> accounts = acStore.FindAccountsForService(SERVICE_ID);
			if (accounts.Count() > 0)
			{
				try{
				Account account = accounts.First();

				String travelerIdString = account.Properties[USER_TRAVELER_ID_KEY];
				int travelerId = Int32.Parse (travelerIdString);
				return travelerId;
				}catch(Exception ex) {
				}
			}
				
			return -1;
		}

		public override string GetUsername()
		{
			AccountStore acStore = AccountStore.Create();
			IEnumerable<Account> accounts = acStore.FindAccountsForService(SERVICE_ID);
			if (accounts.Count() > 0)
			{
				try{
					Account account = accounts.First();

					String username = account.Properties[USERNAME_ID_KEY];
					return username;
				}catch(Exception ex) {
				}
			}
				
			return "";
		}

		public override string GetUserId()
		{
			AccountStore acStore = AccountStore.Create();
			IEnumerable<Account> accounts = acStore.FindAccountsForService(SERVICE_ID);
			if (accounts.Count() > 0)
			{
				try{
					Account account = accounts.First();

					String userId = account.Properties[USER_ID_KEY];
					return userId;
				}catch(Exception ex) {
				}
			}
				
			return "";
		}

		protected override void StoreCredentials(string username, string userid, string usertoken,int accountId)
		{
			base.StoreCredentials(username, userid, usertoken, accountId);

			Dictionary<String, String> accountOptions = new Dictionary<String, String>();
			accountOptions.Add(USER_TOKEN_KEY, usertoken);
			accountOptions.Add(USER_ID_KEY, userid);
			accountOptions.Add (USERNAME_ID_KEY, username);
			accountOptions.Add (USER_TRAVELER_ID_KEY, accountId.ToString());

			Account account = new Account(username, accountOptions);

			AccountStore acStore = AccountStore.Create();
			acStore.Save(account, SERVICE_ID);
		}

		protected override void ClearCredentials()
		{
			AccountStore acStore = AccountStore.Create();
			IEnumerable<Account> accounts = acStore.FindAccountsForService(SERVICE_ID);

			foreach (Account account in accounts)
			{
				acStore.Delete(account, SERVICE_ID);
			}
			base.ClearCredentials();
		}
	}
}

