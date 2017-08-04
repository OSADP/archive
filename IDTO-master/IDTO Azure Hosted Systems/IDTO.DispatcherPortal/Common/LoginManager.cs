namespace IDTO.DispatcherPortal.Common
{
    using Microsoft.WindowsAzure.MobileServices;
    using System.Threading.Tasks;
    using IDTO.DispatcherPortal.Common.Models;

    public class LoginManager
    {
		protected static string USER_TRAVELER_ID_KEY = "UserTravelerId";
        protected static string USER_ID_KEY = "UserId";
        protected static string USER_TOKEN_KEY = "UserToken";
        protected static string SERVICE_ID = "IDTO";
        protected static LoginManager instance;

        protected static LocalLoginMSClient MobileService; 
        //= new LocalLoginMSClient("https://idto-dev.azure-mobile.net/", "xyFSfirhoENlQSxJeQAfOnKzWVCEIn18");

        public LoginManager(string applicationUrl, string applicationKey)
        {
            MobileService = new LocalLoginMSClient(
            applicationUrl, applicationKey);

            LoadCredentials();
        }

		public async Task<LoginResult> Login(string username, string password)
        {
            LoginResult loginResult = await MobileService.Login(username, password);

			if (loginResult.Success) {
				
				StoreCredentials (loginResult.UserName, loginResult.UserId, loginResult.UserToken, 0, "");
			}

            return loginResult;
        }

		protected virtual void StoreCredentials(string username, string userid, string usertoken, int accountId, string firstname){}

        protected virtual void ClearCredentials(){}

        protected virtual void LoadCredentials(){}

		public virtual int GetTravelerId(){ return -1;}

		public virtual string GetUsername(){ return "";}

        public void Logout()
        {
            ClearCredentials();
            MobileService.Logout();
        }

        public async Task<bool> IsLoggedIn()
        {
            bool isLoggedIn = await MobileService.IsLoggedIn();
            return isLoggedIn;
        }

		public async Task<LoginResult> Register(string username, string password, string firstname, string lastname)
        {
            LoginResult loginResult = await MobileService.Register(username, password);

			if (loginResult.Success) 
            {
				StoreCredentials (loginResult.UserName, loginResult.UserId, loginResult.UserToken, 0, "");
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