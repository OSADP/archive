namespace IDTO.Common
{
    using IDTO.Common.Models;
    using Microsoft.WindowsAzure.MobileServices;
    using System.Threading.Tasks;
    using System;

    public class LoginManager
    {
		protected static string USER_TRAVELER_ID_KEY = "UserTravelerId";
		protected static string USERNAME_ID_KEY = "Username";
        protected static string USER_ID_KEY = "UserId";
        protected static string USER_TOKEN_KEY = "UserToken";
        protected static string SERVICE_ID = "IDTO";

        protected LocalLoginMSClient MobileService;


        public LoginManager()
        {

			#if DEBUG
			MobileService = new LocalLoginMSClient(
				"https://idto-dev.azure-mobile.net/", "xyFSfirhoENlQSxJeQAfOnKzWVCEIn18");
			#else
			MobileService = new LocalLoginMSClient(
			"https://idto-prod.azure-mobile.net/", "VMbsLGwIwSJsBpSbOOyHulwycpwlYz53");
			#endif


            LoadCredentials();
        }

		public async Task<LoginResult> Login(string username, string password)
        {

            LoginResult loginResult = await MobileService.Login(username, password);

			if (loginResult.Success) {
				AccountManager accountManager = new AccountManager ();

				TravelerModel travelerAccount = await accountManager.GetTravelerByEmail (username);

				StoreCredentials (loginResult.UserName, loginResult.UserId, loginResult.UserToken, travelerAccount.Id);
			} else {
				this.Logout ();
			}

            return loginResult;
        }

		protected virtual void StoreCredentials(string username, string userid, string usertoken, int accountId){}

        protected virtual void ClearCredentials(){}

        protected virtual void LoadCredentials(){}

		public virtual int GetTravelerId(){ return -1;}

		public virtual string GetUsername(){ return "";}

		public virtual string GetUserId(){ return "";}

        public void Logout()
        {
            ClearCredentials();
            MobileService.Logout();
        }

        public async Task<bool> IsLoggedIn()
        {
            bool isLoggedIn = await MobileService.IsLoggedIn();

			if (!isLoggedIn) {
				this.Logout ();
			}

            return isLoggedIn;
        }

		public async Task<LoginResult> Register(string username, string password, string firstname, string lastname)
        {
            LoginResult loginResult = await MobileService.Register(username, password);

			if (loginResult.Success) {

				AccountManager accountManager = new AccountManager ();
				TravelerModel traveler = new TravelerModel ();
				traveler.Email = username;
				traveler.FirstName = firstname;
				traveler.MiddleName = "";
				traveler.LastName = lastname;
				traveler.DefaultBicycleFlag = false;
				traveler.DefaultMobilityFlag = false;
				traveler.LoginId = loginResult.UserId;
				traveler.PhoneNumber = "000-000-0000";
				traveler.InformedConsent = true;
                traveler.InformedConsentDate = DateTime.UtcNow;
				traveler.DefaultPriority = "1";
				traveler.DefaultTimezone = "EST";

				TravelerModel newTraveleraccount = await accountManager.CreateTraveler (traveler);

				StoreCredentials (loginResult.UserName, loginResult.UserId, loginResult.UserToken, newTraveleraccount.Id);
			} else {
				this.Logout ();
			}

            return loginResult;
        }

        //public async Task<LoginResult> DeleteAccount(string username)
        //{
        //    LoginResult loginResult = await MobileService.DeleteUser(username);

        //    return loginResult;
        //}
    }
}