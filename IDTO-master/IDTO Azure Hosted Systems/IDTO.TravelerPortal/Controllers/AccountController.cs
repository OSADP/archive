using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using IDTO.TravelerPortal.Models;
using IDTO.TravelerPortal.Common;
using IDTO.TravelerPortal.Common.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.SmtpApi;
using System.Net;
using System.Net.Mail;


namespace IDTO.TravelerPortal.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        LoginManager loginManager;

        protected static string SENDGRID_USERNAME = "azure_ebd97354501a43f2f9d293a272665645@azure.com";
        protected static string SENDGRID_PASSWORD = "341S3yHEKdT5d7R";

        public AccountController()
        {
            loginManager = new LoginManager();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            TempData["IsRegisterSelected"] = false;
            Response.StatusCode = 307;
            Response.RedirectLocation = returnUrl;
            return new ContentResult();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                LoginResult loginResult = await loginManager.Login(model.Email, model.Password);

                if (loginResult.Success)
                {
                    UserData userdata = new UserData();
                    userdata.FirstName = loginManager.FirstName();
                    SetAuthCookie(model.Email, model.RememberMe, userdata);

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "TripDashboard");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Login Failed - " + loginResult.ErrorString);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /LoginPartial/
        [AllowAnonymous]
        public PartialViewResult LoginPartial()
        {
            return PartialView();
        }

        //
        // POST: /Account/LoginPartial
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginPartial(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                LoginResult loginResult = await loginManager.Login(model.Email, model.Password);

                if (loginResult.Success)
                {
                    UserData userdata = new UserData();
                    userdata.FirstName = loginManager.FirstName();
                    SetAuthCookie(model.Email, model.RememberMe, userdata);
                    TempData.Clear();

                    return Json(new { success = true, redirect = Url.Action("Index", "TripDashboard") });
                }
                else
                {
                    ModelState.AddModelError("", "Login Failed - " + loginResult.ErrorString);
                }
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        [AllowAnonymous]
        public PartialViewResult LoginWidget(UserData userData)
        {
           
            AccountLoginWidgetViewModel model = new AccountLoginWidgetViewModel();
            if (null != userData)
                model.UserData = userData;
            return PartialView(model);
        }


        private void SetAuthCookie(string userName, bool createPersistentCookie, UserData userData)
        {
            HttpCookie cookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie);
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                 ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration
                , ticket.IsPersistent, JsonConvert.SerializeObject(userData), ticket.CookiePath
            );

            string encTicket = FormsAuthentication.Encrypt(newTicket);
            cookie.Value = encTicket;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            TempData["IsRegisterSelected"] = true;
            Response.StatusCode = 307;
            Response.RedirectLocation = returnUrl;
            return new ContentResult();
        }

        //
        // GET: /RegisterPartial/
        [AllowAnonymous]
        public PartialViewResult RegisterPartial()
        {
            return PartialView();
        }

        //
        // POST: /Account/RegisterPartial
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterPartial(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                LoginResult loginResult = await loginManager.Register(model.Email, model.Password, model.FirstName, model.LastName);

                if (loginResult.Success)
                {
                    UserData userdata = new UserData();
                    userdata.FirstName = model.FirstName;
                    SetAuthCookie(model.Email, false, userdata);
                    TempData.Clear();

                    return Json(new { success = true, redirect = Url.Action("Index", "TripDashboard") });
                }
                else
                {
                    ModelState.AddModelError("", "Registration Failed - " + loginResult.ErrorString);
                }
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                LoginResult loginResult = await loginManager.Register(model.Email, model.Password, model.FirstName, model.LastName);

                if (loginResult.Success)
                {
                    UserData userdata = new UserData();
                    userdata.FirstName = model.FirstName;
                    SetAuthCookie(model.Email, false, userdata);
                    return RedirectToAction("Index", "TripDashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Registration Failed - " + loginResult.ErrorString);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Manage
        [Authorize]
        public async Task<ActionResult> Manage(string email)
        {
            AccountViewModel model = new AccountViewModel();
            LocalPasswordModel localPasswordModel = new LocalPasswordModel();

            TravelerModel traveler = await loginManager.GetTravelerByEmail(email);
            model.accountInfoModel = new AccountInfoModel(traveler);
            model.traveler = traveler;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ManagePassword(AccountViewModel model)
        {
           
            LoginResult loginResult = await loginManager.ChangePassword(model.accountInfoModel.Email, model.localPasswordModel.OldPassword,  model.localPasswordModel.NewPassword);

            if (loginResult.Success)
            {
                ModelState.AddModelError("localPasswordModel", "Password changed");
            }
            else
            {
                ModelState.AddModelError("localPasswordModel", loginResult.ErrorString);
            }

            return View("Manage", model);
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Manage(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                TravelerModel traveler = await loginManager.GetTravelerByEmail(model.accountInfoModel.Email);

                traveler.PromoCode = model.accountInfoModel.PromoCode;

                model.accountInfoModel.PromoCodeUpdated = true;
                try
                {
                    TravelerModel updatedTraveler = loginManager.UpdateTraveler(traveler);
                    if (updatedTraveler.Email == model.accountInfoModel.Email)
                    {
                        ModelState.AddModelError("accountInfoModel", "Settings Updated");
                        model.accountInfoModel.PromoCodeValid = true;
                    }
                    else
                    {
                        ModelState.AddModelError("accountInfoModel", "Update Failed");
                        model.accountInfoModel.PromoCodeValid = false;
                        model.accountInfoModel.PromoCode = null;
                    }
                }catch(Exception ex)
                {
                    model.accountInfoModel.PromoCodeValid = false;
                    model.accountInfoModel.PromoCode = null;
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ForgotPasswordViewModel vm = new ForgotPasswordViewModel();
            vm.Email = "";
            return View(vm);
        }


        [AllowAnonymous]
        public async Task<ActionResult> SendLoginEmail(ForgotPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                //Generate a new random password and change the password for this user
                string tempPassword = Membership.GeneratePassword(8, 0);
                LoginResult updateResult = await loginManager.ChangePasswordIfForgotten(vm.Email, tempPassword);

                if (updateResult.Success)
                {
                    //Send Email with new Password

                    // Create the email object first, then add the properties.
                    SendGridMessage myMessage = new SendGridMessage();
                    myMessage.AddTo(vm.Email);
                    myMessage.From = new MailAddress("connectandride@battelle.org", "Connect and Ride");
                    myMessage.Subject = "Connect and Ride Login";
                    myMessage.Text = "Please use the link below to login using the temporary password:\n\n" + tempPassword + "\n\nhttp://www.connectandride.com\n\nYou may change your password once you login.\n\n"
                        + "Thank you,\n\nThe Battelle research team";

                    // Create credentials, specifying your user name and password.
                    var credentials = new NetworkCredential(SENDGRID_USERNAME, SENDGRID_PASSWORD);

                    // Create an Web transport for sending email.
                    var transportWeb = new Web(credentials);

                    // Send the email.
                    transportWeb.Deliver(myMessage);
                }
                else
                {
                    ModelState.AddModelError("Email", updateResult.ErrorString);
                    return View("ForgotPassword", vm);
                }

                return View(vm);

            }

            return View("ForgotPassword", vm);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //Sign out here
            FormsAuthentication.SignOut();
            loginManager.Logout();

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        #region Helpers

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion
    }
}