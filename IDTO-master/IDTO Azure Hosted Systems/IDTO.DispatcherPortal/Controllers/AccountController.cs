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
using IDTO.DispatcherPortal.Models;
using IDTO.DispatcherPortal.Common;
using IDTO.DispatcherPortal.Common.Models;
using System.Configuration;

namespace IDTO.DispatcherPortal.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        LoginManager loginManager;

        public AccountController()
        {
            String applicationUrl = ConfigurationManager.AppSettings["ApplicationUrl"];

            String applicationKey = ConfigurationManager.AppSettings["ApplicationKey"];
            loginManager = new LoginManager(applicationUrl, applicationKey);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                LoginResult loginResult = await loginManager.Login(model.Email, model.Password);

                if (loginResult.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);

                    return RedirectToAction("Index", "Dashboard");
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
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
 
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}