using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Core.Constants;
using Server.Core.Helpers;
using Server.Site.Models;

namespace Server.Site.Controllers
{
    public class HomeController : Controller
    {
        private LoginHelper loginHelper;
        public HomeController(LoginHelper _loginHelper)
        {
            loginHelper = _loginHelper;
        }

        public IActionResult Index()
        {
            ViewBag.UserId = loginHelper.GetUserId(HttpContext);
            ViewBag.UserName = loginHelper.GetUserName(HttpContext);
            ViewBag.UserToken = loginHelper.GetUserToken(HttpContext);
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
