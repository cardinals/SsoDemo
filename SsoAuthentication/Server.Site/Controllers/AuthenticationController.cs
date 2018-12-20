using Microsoft.AspNetCore.Mvc;
using Server.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Helpers;
using Server.Core.Model;
using Server.Core.Constants;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Server.Core.Enums;

namespace Server.Site.Controllers
{
    public class AuthenticationController : Controller
    {
        #region 构造注入
        private readonly SiteContext siteContext;
        private readonly LoginHelper loginHelper;
        private readonly HttpPostHelper httpPostHelper;
        private readonly ComputeHashHelper computeHashHelper;
        public AuthenticationController(SiteContext _siteContext, HttpPostHelper _httpPostHelper, ComputeHashHelper _computeHashHelper, LoginHelper _loginHelper)
        {
            siteContext = _siteContext;
            httpPostHelper = _httpPostHelper;
            computeHashHelper = _computeHashHelper;
            loginHelper = _loginHelper;
        }
        #endregion

        /// <summary>
        /// 无状态
        /// 回调验证地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public AuthCallbackResponseModel AuthCallback(AuthCallbackRequestModel data)
        {

            #region 验证签名
            var disgest = HttpContext.Request.Headers[HttpHeaders.Disgest];

            var reader = new StreamReader(Request.Body);
            var bodyStr = reader.ReadToEnd();
            byte[] body = computeHashHelper.Charset.GetBytes(bodyStr);
            if (!computeHashHelper.IsComputeHash(disgest, body))
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 1,
                    Message = "Disgest wrong."
                };
            }
            #endregion

            #region 检查 app key
            //#TODO，需要根据来源域名、AppKey一起验证
            var appKey = HttpContext.Request.Headers[HttpHeaders.AppKey];
            var siteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.AppKey == appKey);
            if (siteConfig == null)
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code=1,
                    Message="App Key wrong."
                };
            }
            #endregion

            //#TODO 每次请求的传递变量值验证
            return new AuthCallbackResponseModel()
            {
                Success = true
            };
        }

        #region 关联授权
        [HttpGet]
        public IActionResult AuthMapping(AuthMappingRequestModel requestModel)
        {
            return View(requestModel ?? new AuthMappingRequestModel());
        }
        [HttpPost]
        public IActionResult AuthMappingToDo(AuthMappingRequestModel requestModel)
        {
            #region 检查 app key
            //#TODO，需要根据来源域名、AppKey一起验证
            var siteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.AppKey == requestModel.AppKey);
            if (siteConfig == null)
            {
                return Redirect(requestModel.TargetUrl);
            }
            #endregion
            var userId = loginHelper.GetUserId(HttpContext);
            var authMappingUrl = $"{siteConfig.AuthMapping}?SiteToken={siteConfig.SiteToken}&SsoUserId={userId}&TargetUrl={System.Net.WebUtility.UrlEncode(requestModel.TargetUrl)}";
            return Redirect(authMappingUrl);
        }
        #endregion

        /// <summary>
        /// 跳转到第三方地网站
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult RedirectToSite(RedirectToSiteRequestModel data)
        {
            #region 验证跳转地址
            Uri TargetUrl = new Uri(data.TargetUrl);
            var tagerHost = TargetUrl.Authority.ToUpper();
            //#TODO 后续需要将 SiteConfig 缓存起来，不要每次查询
            var tagerSiteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.Host == tagerHost);
            if (tagerSiteConfig == null)
            {
                //#TODO 将错误码以及错误信息通过URL传递给 client
                return Redirect(data.TargetUrl);
            }
            #endregion

            #region 验证UserToken
            var user = siteContext.User.FirstOrDefault(x=>x.Id==loginHelper.GetUserId(HttpContext));
            if (user == null)
            {
                //#TODO 将错误码以及错误信息通过URL传递给 client
                return Redirect(data.TargetUrl);
            }
            //if(user.Active && user.UserToken.HasValue && user.UserToken.ToString()==data.UserToken && user.ExpiredTime>DateTime.Now)
            if (user.Active && user.UserToken.HasValue && user.ExpiredTime > DateTime.Now)
            {
                //重定向到 target client 写入 session
                var url = $"{tagerSiteConfig.WriteSession}?SsoUserId={user.Id}&UserToken={user.UserToken}&SiteToken={tagerSiteConfig.SiteToken}&TargetUrl={System.Net.WebUtility.UrlEncode(data.TargetUrl)}";
                return Redirect(url);
            }
            #endregion
            return Redirect(data.TargetUrl);
        }

        [AllowAnonymous]
        public AuthCallbackResponseModel AuthUserToken([FromBody]AuthUserTokenRequestModel data)
        {

            var appKey = HttpContext.Request.Headers[HttpHeaders.AppKey];
            var disgest = HttpContext.Request.Headers[HttpHeaders.Disgest];
            Request.Body.Position = 0;
            var reader = new StreamReader(Request.Body);
            var bodyStr = reader.ReadToEnd();
            
            #region 验证签名
            byte[] body = computeHashHelper.Charset.GetBytes(bodyStr);
            if (!computeHashHelper.IsComputeHash(disgest, body))
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 1,
                    Message = "ComputeHash wrong."
                };
            }
            #endregion

            #region 验证 AppKey 
            //#TODO 后续需要将 SiteConfig 缓存起来，不要每次查询
            var siteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.AppKey == appKey);
            if (siteConfig == null)
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 2,
                    Message = "AppKey wrong."
                };
            }
            #endregion

            #region 验证来源地址

            #endregion

            #region 验证用户信息
            //if(loginHelper.GetUserId(HttpContext)!=data.SsoUserId)
            //{
            //    return new AuthCallbackResponseModel()
            //    {
            //        Success = false,
            //        Code = 6,
            //        Message = "User wrong."
            //    };
            //}
            var user = siteContext.User.FirstOrDefault(x => x.Id == data.SsoUserId);
            if (user == null)
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 4,
                    Message = "User wrong."
                };
            } 

            if(user.UserToken.ToString()!=data.UserToken)
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 5,
                    Message = "UserToken wrong."
                };
            }
            #endregion
            return new AuthCallbackResponseModel() {
                Success = true
            };
        }
    }
}
