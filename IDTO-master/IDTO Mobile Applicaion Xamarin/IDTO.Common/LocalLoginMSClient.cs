using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.WindowsAzure.MobileServices;

using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using IDTO.Common.Models;

namespace IDTO.Common
{
    public class LocalLoginMSClient : MobileServiceClient
    {
        public string UserName { get; set; }

        public LocalLoginMSClient(string applicationUrl,
            string applicationKey)
            : base(applicationUrl, applicationKey)
        {

        }

        //public async Task<LoginResult> DeleteUser(String username)
        //{
        //    IMobileServiceTable accounts = this.GetTable("logins");
        //    LoginResult loginResults = new LoginResult();
        //    JObject credentialsObj = JObject.FromObject(new
        //    {
        //        username = username,
        //    }
        //    );
        //    try
        //    {
        //        JToken jt = await accounts.DeleteAsync(credentialsObj);
        //        loginResults.Success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        loginResults.Success = false;
        //        loginResults.ErrorString = ex.Message;
        //    }

        //    return loginResults;
        //}

        public async Task<LoginResult> Register(String username, String password)
        {
            IMobileServiceTable accounts = this.GetTable("logins");

            LoginResult loginResults = new LoginResult();

            JObject credentialsObj = JObject.FromObject(new
            {
                username = username,
                password = password,
            }
            );

            try
            {
                Task<JToken> x = accounts.InsertAsync(credentialsObj);

                JToken jt = await x;

                string token = jt.Value<String>("token");
                string userid = jt.Value<String>("userId");

                MobileServiceUser user = new MobileServiceUser(userid);
                user.MobileServiceAuthenticationToken = token;
                this.CurrentUser = user;
                this.UserName = username;

                loginResults.Success = true;
                loginResults.UserId = userid;
                loginResults.UserToken = token;
                loginResults.UserName = username;
            }
            catch (Exception ex)
            {
                loginResults.Success = false;
                loginResults.ErrorString = ex.Message;
            }

            return loginResults;

        }
			
		public void LogoutUser()
        {
			this.Logout ();
            this.CurrentUser = null;
            this.UserName = null;
        }

        public async Task<bool> IsLoggedIn()
        {
            if (this.CurrentUser == null)
                return false;
            else
            {

                var todo = this.GetTable("TodoItem");
                try
                {
                    JToken results = await todo.ReadAsync("$filter=(text eq 'gregtest')");  
                    return true;
                }
                catch(Exception ex)
                {
					Console.WriteLine (ex.Message);
                    return false;
                }
                
            }
        }

        public async Task<LoginResult> Login(String username, String password)
        {
            IMobileServiceTable accounts = this.GetTable("logins");

            LoginResult loginResults = new LoginResult();

            JObject credentialsObj = JObject.FromObject(new
            {
                username = username,
                password = password,
            }
            );

            try
            {
                Dictionary<String, String> parameters = new Dictionary<String, String>();

                parameters.Add("login","true");


                Task<JToken> x = accounts.InsertAsync(credentialsObj,parameters);

                JToken jt = await x;


                string token = jt.Value<String>("token");
                string userid = jt.Value<String>("userId");

                MobileServiceUser user = new MobileServiceUser(userid);
                user.MobileServiceAuthenticationToken = token;
                this.CurrentUser = user;

                this.UserName = username;

                loginResults.Success = true;
                loginResults.UserId = userid;
                loginResults.UserToken = token;
                loginResults.UserName = username;
            }
            catch (Exception ex)
            {
                loginResults.Success = false;
                loginResults.ErrorString = ex.Message;
            }

            return loginResults;
        }
    }
}