using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Client.SiteA.Models;
using Client.Core.Constants;
using Client.Core.Helpers;

namespace Client.SiteA.Controllers
{
    public class HomeController : Controller
    {
        private LoginHelper loginHelper;
        public HomeController( LoginHelper _loginHelper)
        {
            loginHelper = _loginHelper;
        }

        public IActionResult Index()
        {
            ViewBag.UserId = loginHelper.GetUserId(HttpContext);
            ViewBag.UserName = loginHelper.GetUserName(HttpContext);
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
