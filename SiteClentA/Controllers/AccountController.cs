using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteClient.Core;
using SiteClient.Core.Data;
using SiteClient.Core.Entity;
using SiteClient.Core.Model;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiteClientA.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private SiteContext siteContext;
        public AccountController(SiteContext _siteContext)
        {
            siteContext = _siteContext;
        }


        [HttpGet]
        public IActionResult Login()
        {
            if (new LoginHelper().IsLogin(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginModel() { UserName = "admin", PassWord = "123456" });
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            var user = siteContext.User.FirstOrDefault(x => x.UserName == model.UserName && x.PassWord == model.PassWord);
            if (user != null)
            {
                #region 写入身份信息
                new LoginHelper().Login(HttpContext, new User()
                {
                    Id = user.Id,
                    UserName = user.UserName
                }, false, null);
                #endregion
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public IActionResult Logout(LoginModel model)
        {
            new LoginHelper().Logout(HttpContext);
            return RedirectToAction("Login","Account");
        }
    }
}
