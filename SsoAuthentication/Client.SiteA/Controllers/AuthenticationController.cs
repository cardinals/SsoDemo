using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Client.Core.Constants;
using Client.Core.Data;
using Client.Core.Entity;
using Client.Core.Helpers;
using Client.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Server.Core.Model;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Client.SiteA.Controllers
{
    public class AuthenticationController : Controller
    {
        #region 构造注入
        private readonly SsoConfig ssoConfigs;
        private readonly LoginHelper loginHelper;
        private readonly HttpPostHelper httpPostHelper;
        private readonly SiteContext siteContext;
        private readonly ComputeHashHelper computeHashHelper;
        public AuthenticationController(IOptions<SsoConfig> _ssoConfigs, LoginHelper _loginHelper, HttpPostHelper _httpPostHelper, SiteContext _siteContext, ComputeHashHelper _computeHashHelper)
        {
            ssoConfigs = _ssoConfigs.Value;
            loginHelper = _loginHelper;
            httpPostHelper = _httpPostHelper;
            siteContext = _siteContext;
            computeHashHelper = _computeHashHelper;
        }

        #endregion
        /// <summary>
        /// 跳转到其他系统的中转地址
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult RedirectToSite(RedirectToSiteRequestModel data)
        {
            #region 未登录，直接跳转到 sso ，由Sso跳转到登录页，登录后转向到 return url
            if (!loginHelper.IsLogin(HttpContext))
            {
                return Redirect($"{ssoConfigs.RedirectToSite}?TargetUrl={System.Net.WebUtility.UrlEncode(data.TargetUrl)}");
            }
            #endregion

            int userId = loginHelper.GetUserId(HttpContext);
            string userToken = loginHelper.GetUserToken(HttpContext);

            #region 检查是否已关联到 SSO 系统,没关联到，则连接到 SSO 做关联认证
            var mapping = siteContext.UserMapping.FirstOrDefault(x => x.UserId == userId);
            if (mapping == null)
            {
                var authMappingUrl = $"{ssoConfigs.AuthMapping}?AppKey={ssoConfigs.AppKey}&UserId={userId}&TargetUrl={System.Net.WebUtility.UrlEncode(data.TargetUrl)}";
                return Redirect(authMappingUrl);
            }
            #endregion

            string url = $"{ssoConfigs.RedirectToSite}?TargetUrl={System.Net.WebUtility.UrlEncode(data.TargetUrl)}";
            return Redirect(url);
        }

        /// <summary>
        /// 授权接入 SSO
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult AuthMapping(AuthMappingRequestModel data)
        {
            #region 验证SiteToken
            if (ssoConfigs.SiteToken != data.SiteToken)
            {
                return View(data);
            }
            #endregion

            #region 向 Server 做回调验证，谨防伪造
            var callBackRequest = new AuthMappingCallBackRequestModel()
            {
                AppKey = ssoConfigs.AppKey,
                SsoUserId = data.SsoUserId
            };
            var postData = JsonConvert.SerializeObject(callBackRequest);
            var resrponseStr = httpPostHelper.Send(ssoConfigs.AuthMappingCallBack, postData);
            var reswponseObject = JsonConvert.DeserializeObject<AuthMappingCallBackResponseModel>(resrponseStr);
            if (reswponseObject == null || !reswponseObject.Success)
            {
                //TODO 
                //若未登录，则跳转到登录页
                //若已登录，则跳转到授权接入页
                return Redirect(data.TargetUrl);
            }
            #endregion

            #region 新增UserMapping记录
            int userId = loginHelper.GetUserId(HttpContext);
            if (userId > 0)
            {
                UserMapping userMapping = new UserMapping()
                {
                    UserId = userId,
                    SsoUserId = data.SsoUserId
                };
                siteContext.UserMapping.Add(userMapping);
                siteContext.SaveChanges();

                return RedirectToAction("RedirectToSite", new { TargetUrl = data.TargetUrl});
            }
            else
            {
                return View(data);
            }
            #endregion
        }

        /// <summary>
        /// 无状态
        /// 回调验证地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public AuthCallbackResponseModel AuthCallback([FromBody]AuthCallbackRequestModel data)
        {
            var siteToken = HttpContext.Request.Headers[HttpHeaders.SiteToken];
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
            if (ssoConfigs.SiteToken != siteToken)
            {
                return new AuthCallbackResponseModel()
                {
                    Success = false,
                    Code = 1,
                    Message = "SiteToken wrong."
                };
            }
            //#TODO 每次请求的传递变量值验证
            return new AuthCallbackResponseModel()
            {
                Success = true
            };
        }

        /// <summary>
        /// SSO向本系统写入登录信息接口地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult WriteSession(AuthSessionRequestModel data)
        {
            #region 验证 SiteToken 
            if (ssoConfigs.SiteToken != data.SiteToken)
            {
                return Redirect(data.TargetUrl);
            }
            #endregion

            #region 向 sso AuthUserToken 发送回调验证
            var callBackRequest = new AuthUserTokenRequestModel()
            {
                AppKey=ssoConfigs.AppKey,
                SsoUserId=data.SsoUserId,
                UserToken=data.UserToken
            };
            var postData = JsonConvert.SerializeObject(callBackRequest);
            var resrponseStr = httpPostHelper.Send(ssoConfigs.AuthUserToken, postData);
            var reswponseObject = JsonConvert.DeserializeObject<AuthUserTokenResponseModel>(resrponseStr);
            if (reswponseObject == null || !reswponseObject.Success)
            {
                //TODO 
                //若未登录，则跳转到登录页
                //若已登录，则跳转到授权接入页
                return Redirect(data.TargetUrl);
            }
            #endregion

            #region 验证UserMapping
            UserMapping userMapping = siteContext.UserMapping.FirstOrDefault(x => x.SsoUserId == data.SsoUserId);
            if (userMapping == null)
            {
                //TODO 
                //若未登录，则跳转到登录页
                //若已登录，则跳转到授权接入页
                return Redirect(data.TargetUrl);
            }
            #endregion

            User user = siteContext.User.Find(userMapping.UserId);
            if (user == null)
            {
                //TODO 
                //若未登录，则跳转到登录页
                //若已登录，则跳转到授权接入页
                return Redirect(data.TargetUrl);
            }

            //写入登录信息
            loginHelper.Login(HttpContext, user, false, data.UserToken);

            return Redirect(data.TargetUrl);
        }
    }
}
