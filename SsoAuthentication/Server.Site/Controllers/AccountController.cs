using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Data;
using Server.Core.Entity;
using Server.Core.Helpers;
using Server.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Core.Constants;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Site.Controllers
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
            return View(new LoginModel() { UserName = "rober", PassWord = "123456", ReturnUrl = returnUrl });
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            var user = siteContext.User.FirstOrDefault(x => x.UserName == model.UserName && x.PassWord == model.PassWord);
            if (user != null)
            {
                #region 修改用户登录状态/过期时间
                if (!user.Active || user.ExpiredTime <= DateTime.Now)
                {
                    user.Active = true;
                    user.UserToken = Guid.NewGuid();
                }
                user.ExpiredTime = DateTime.Now.AddMinutes(SessionConstants.ExpiredTime);
                siteContext.User.Update(user);
                siteContext.SaveChanges(); 
                #endregion

                #region 写入身份信息
                loginHelper.Login(HttpContext, new User()
                {
                    Id = user.Id,
                    UserName = user.UserName
                }, false, user.UserToken.ToString());
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
            new LoginHelper().Logout(HttpContext);
            return RedirectToAction("Login", "Account");
        }
    }
}
