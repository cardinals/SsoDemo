using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SiteClient.Core.Entity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SiteClient.Core
{
    public class LoginHelper
    {
        public void Login(HttpContext httpContext, User user,bool remember,string userToken)
        {
            #region 写入身份信息
            List<Claim> cailms = new List<Claim>() {
                new Claim(SessionConstants.UserIdScheme,user.Id.ToString()),
                new Claim(SessionConstants.UserNameScheme,user.UserName)
            };
            if (!string.IsNullOrEmpty(userToken))
                cailms.Add(new Claim(SessionConstants.UserIdScheme, userToken));

            var userClaimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                        new Claim(SessionConstants.UserIdScheme,user.Id.ToString()),
                        new Claim(SessionConstants.UserNameScheme,user.UserName)
                }, CookieAuthenticationDefaults.AuthenticationScheme)
            );
            if (remember)
            {
                httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userClaimsPrincipal, new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.Now.Add(TimeSpan.FromDays(7)), // 有效时间
                    IsPersistent = true,
                    AllowRefresh = false
                });
            }
            else
            {
                httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userClaimsPrincipal);
            }
            #endregion
        }

        public async void Logout(HttpContext httpContext)
        {
            await httpContext.SignOutAsync();
            DeleteCookies(httpContext, "UserToken");
        }

        public bool IsLogin(HttpContext httpContext)
        {
            var authenticate = httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return authenticate.Result.Succeeded;
        }

        public void SetUserToken(HttpContext httpContext,string token)
        {
            SetCookies(httpContext, "UserToken", token);
        }

        public string GetUserToken(HttpContext httpContext)
        {
            return GetCookies(httpContext, "UserToken");
        }

        #region 
        /// <summary>
        /// 设置本地cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>  
        /// <param name="minutes">过期时长，单位：分钟</param>      
        protected void SetCookies(HttpContext httpContext,string key, string value, int minutes = 30)
        {
            httpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(minutes)
            });
        }
        /// <summary>
        /// 删除指定的cookie
        /// </summary>
        /// <param name="key">键</param>
        protected void DeleteCookies(HttpContext httpContext, string key)
        {
            httpContext.Response.Cookies.Delete(key);
        }

        /// <summary>
        /// 获取cookies
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        protected string GetCookies(HttpContext httpContext, string key)
        {
            httpContext.Request.Cookies.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
        }
        #endregion
    }
}
