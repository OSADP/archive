using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Web;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json.Linq;
using IDTO.TravelerPortal.Common.Models;
using System.Security.Claims;

namespace IDTO.TravelerPortal.Common
{
    public class LocalLoginMSClient : MobileServiceClient
    {
        public string UserName { get; set; }

        protected static string AUD_HEADER = "Custom:";

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

        public bool ValidateUser(string username)
        {
            if (this.CurrentUser != null)
            {
                FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                FormsAuthenticationTicket ticket = (id.Ticket);
                if (!ticket.Expired && ticket.Name.Equals(username))
                    return true;
            }

            return false;
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

        public async Task<LoginResult> ChangePasswordIfForgotten(String username, string newPassword)
        {
            IMobileServiceTable accounts = this.GetTable("logins");

            LoginResult loginResults = new LoginResult();

            JObject credentialsObj = JObject.FromObject(new
            {
                username = username,
            }
            );

            try
            {
                Dictionary<String, String> parameters = new Dictionary<String, String>();

                parameters.Add("getUser", "true");

                Task<JToken> x = accounts.InsertAsync(credentialsObj, parameters);

                JToken jt = await x;

                string userid = jt.Value<String>("userId").Replace(AUD_HEADER, "");

                //call the update function for this user

                JObject updateObj = JObject.FromObject(new
                {
                    id = userid,
                    password = newPassword,
                }
                );

                x = accounts.UpdateAsync(updateObj);

                jt = await x;

                userid = jt.Value<String>("userId");

                loginResults.Success = true;
                loginResults.UserId = userid;
                loginResults.UserName = username;
            }
            catch (Exception ex)
            {
                loginResults.Success = false;
                loginResults.ErrorString = ex.Message;
            }

            return loginResults;            
        }


        public async Task<LoginResult> ChangePassword(String username, String oldPassword, string newPassword)
        {
            IMobileServiceTable accounts = this.GetTable("logins");

            LoginResult loginResults = new LoginResult();

            JObject credentialsObj = JObject.FromObject(new
            {
                username = username,
                password = oldPassword,
            }
            );

            try
            {
                Dictionary<String, String> parameters = new Dictionary<String, String>();

                parameters.Add("login", "true");

                Task<JToken> x = accounts.InsertAsync(credentialsObj, parameters);

                JToken jt = await x;

                string token = jt.Value<String>("token");
                string userid = jt.Value<String>("userId").Replace(AUD_HEADER, "");

                //call the update function for this user

                JObject updateObj = JObject.FromObject(new
                {
                    id = userid,
                    password = newPassword,
                }
                );

                x = accounts.UpdateAsync(updateObj);

                jt = await x;

                token = jt.Value<String>("token");
                userid = jt.Value<String>("userId");

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