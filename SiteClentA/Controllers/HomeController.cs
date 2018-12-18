using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SiteClient.Core;
using SiteClientA.Models;

namespace SiteClientA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == SessionConstants.UserIdScheme);
            var userNameClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == SessionConstants.UserNameScheme);
            if (userIdClaim != null)
                ViewBag.UserId = userIdClaim.Value;
            if (userNameClaim != null)
                ViewBag.UserName = userNameClaim.Value;
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
