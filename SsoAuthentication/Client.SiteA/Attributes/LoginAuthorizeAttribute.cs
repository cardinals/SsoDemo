using Client.Core.Constants;
using Client.Core.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;


namespace Client.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LoginAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext.Filters.Any(x => x.GetType() == typeof(AllowAnonymousFilter))) { return; }

            //cookie 认证
            //var authenticate = filterContext.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //if (authenticate.Result.Succeeded) { return; }

            //sesson 认证
            if(filterContext.HttpContext.Session.GetInt32(SessionConstants.UserIdScheme)!=null)
            {
                return;
            }
            
            CreateForbiddenResult(filterContext);
        }

        private void CreateForbiddenResult(AuthorizationFilterContext filterContext)
        {
            if (IsAjax(filterContext))
            {
                BaseResponseModel result = new BaseResponseModel()
                {
                    Success = false,
                    Code = 1,
                    Message = "Session checked fail."
                };
                filterContext.Result = new JsonResult(result);
            }
            else
            {
                //filterContext.Result = new UnauthorizedResult();
                filterContext.Result = new RedirectResult("/Account/Login?ReturnUrl=" + System.Net.WebUtility.UrlEncode(filterContext.HttpContext.Request.Path + filterContext.HttpContext.Request.QueryString));
            }
        }

        private bool IsAjax(AuthorizationFilterContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers.ContainsKey("x-requested-with") && filterContext.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest";
        }
    }
}
