using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Core.Data;
using Client.Core.Entity;
using Client.Core.Helpers;
using Client.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Client.SiteA.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private SiteContext siteContext;
        private LoginHelper loginHelper;
        public AccountController(SiteContext _siteContext, LoginHelper _loginHelper)
        {
            siteContext = _siteContext;
            loginHelper = _loginHelper;
        }


        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            if (loginHelper.IsLogin(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel() { UserName = "admin", PassWord = "123456", ReturnUrl = returnUrl });
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            var user = siteContext.User.FirstOrDefault(x => x.UserName == model.UserName && x.PassWord == model.PassWord);
            if (user != null)
            {
                #region 写入身份信息
                loginHelper.Login(HttpContext, new User()
                {
                    Id = user.Id,
                    UserName = user.UserName
                }, false, null);
                #endregion

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public IActionResult Logout(LoginModel model)
        {
            loginHelper.Logout(HttpContext);
            return RedirectToAction("Login", "Account");
        }
    }
}
