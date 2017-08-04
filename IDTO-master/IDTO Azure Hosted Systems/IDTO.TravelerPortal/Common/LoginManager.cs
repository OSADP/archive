namespace IDTO.TravelerPortal.Common
{
    using IDTO.TravelerPortal.Common.Models;
    using Microsoft.WindowsAzure.MobileServices;
    using System.Threading.Tasks;
    using System.Configuration;

    public class LoginManager
    {
        protected static string USER_TRAVELER_ID_KEY = "UserTravelerId";
        protected static string USER_ID_KEY = "UserId";
        protected static string USER_TOKEN_KEY = "UserToken";
        protected static string SERVICE_ID = "IDTO";
        protected static LoginManager instance;
        private string firstName = "";

        protected static LocalLoginMSClient MobileService;

        private static AccountManager accountManager;

        public LoginManager()
        {
            string mobileServiceURL = Config.MobileServiceUrl;
            string mobileAppKey = Config.MobileAppKey;

            MobileService = new LocalLoginMSClient(
            mobileServiceURL, mobileAppKey);

            LoadCredentials();
            accountManager = new AccountManager();
        }

        public string FirstName()
        {
            return firstName;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            LoginResult loginResult = await MobileService.Login(username, password);

            if (loginResult.Success) {
                
                TravelerModel travelerAccount = accountManager.GetTravelerByEmail (username);

                if (travelerAccount == null)
                {
                    loginResult.ErrorString = "Account not found";
                    loginResult.Success = false;
                }
                else
                {
                    firstName = travelerAccount.FirstName;
                    StoreCredentials(loginResult.UserName, loginResult.UserId, loginResult.UserToken, travelerAccount.Id, travelerAccount.FirstName);
                }
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

        public async Task<TravelerModel> GetTravelerByEmail(string email)
        {
            TravelerModel travelerAccount = accountManager.GetTravelerByEmail(email);
            return travelerAccount;
        }

        public async Task<LoginResult> Register(string username, string password, string firstname, string lastname)
        {
            LoginResult loginResult = await MobileService.Register(username, password);

            if (loginResult.Success) {

                TravelerModel traveler = new TravelerModel ();
                traveler.Email = username;
                traveler.FirstName = firstname;
                traveler.MiddleName = "";
                traveler.LastName = lastname;
                traveler.DefaultBicycleFlag = false;
                traveler.DefaultMobilityFlag = false;
                traveler.LoginId = loginResult.UserId;
                traveler.PhoneNumber = "000-000-0000";
                traveler.InformedConsent = false;
                traveler.DefaultPriority = "1";
                traveler.DefaultTimezone = "EST";

                TravelerModel newTraveleraccount = accountManager.CreateTraveler (traveler);

                StoreCredentials (loginResult.UserName, loginResult.UserId, loginResult.UserToken, newTraveleraccount.Id, newTraveleraccount.FirstName);
            }

            return loginResult;
        }

        public TravelerModel UpdateTraveler(TravelerModel traveler)
        {
            
            TravelerModel newTraveleraccount = accountManager.UpdateTraveler(traveler);

            return newTraveleraccount;
        }


        public async Task<LoginResult> ChangePasswordIfForgotten(string username, string newPassword)
        {
            LoginResult loginResult = await MobileService.ChangePasswordIfForgotten(username, newPassword);

            return loginResult;
        }

        public async Task<LoginResult> ChangePassword(string username, string oldPassword, string newPassword)
        {
            LoginResult loginResult = await MobileService.ChangePassword(username, oldPassword, newPassword);

            if (loginResult.Success)
            {
                TravelerModel travelerAccount = accountManager.GetTravelerByEmail(username);

                StoreCredentials(loginResult.UserName, loginResult.UserId, loginResult.UserToken, travelerAccount.Id, travelerAccount.FirstName);
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