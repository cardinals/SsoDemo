using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SiteClient.Core;
using SiteClient.Core.Model;
using SiteClient.Core.Helpers;
using SiteServer.Core.Model;
using Newtonsoft.Json;
using SiteClient.Core.Data;
using SiteServer.Core.Helpers;
using SiteClient.Core.Entity;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiteClientA.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly SsoConfig ssoConfigs;
        private readonly LoginHelper loginHelper;
        private readonly HttpPostHelper httpPostHelper;
        private readonly SiteContext siteContext;
        private readonly ComputeHashHelper computeHashHelper;
        public AuthenticationController(IOptions<SsoConfig> _ssoConfigs, LoginHelper _loginHelper, HttpPostHelper _httpPostHelper,SiteContext _siteContext, ComputeHashHelper _computeHashHelper)
        {
            ssoConfigs = _ssoConfigs.Value;
            loginHelper = _loginHelper;
            httpPostHelper = _httpPostHelper;
            siteContext = _siteContext;
            computeHashHelper = _computeHashHelper;
        }

        /// <summary>
        /// 请求登录第三方系统
        /// </summary>
        /// <returns></returns>
        public IActionResult RedirectToSite(RedirectToSiteRequestModel data)
        {
            /*
             * 1.检查用户是否登录
             * 2.检查是否存在SSO中的UserToken
             *   若不存在，则请求 sso Authentication/AuthRedirectToSite 获取UserToken
             * 3.判断UserToken
             *   若不存在，则返回认证失败
             *   若存在，则重定向到 sso Authentication/RedirectToSite
             * */
            //1.检查用户是否登录
            if (!loginHelper.IsLogin(HttpContext))
            {
                return Redirect(ssoConfigs.AuthFail);
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == SessionConstants.UserIdScheme);
            var userTokenClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == SessionConstants.UserTokenScheme);

            var userId = userIdClaim == null ? "" : userIdClaim.Value;
            var userToken = userTokenClaim == null ? "" : userTokenClaim.Value;

            var mapping = siteContext.UserMapping.FirstOrDefault(x => x.UserId == int.Parse(userId));
            var ssoUserId = mapping == null ? 0 : mapping.SsoUserId;

            //检查 userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect(ssoConfigs.AuthFail);
            }
            
            //2.检查 UserToken
            if (string.IsNullOrWhiteSpace(userToken))
            {
                var siteToken = ssoConfigs.SiteToken;
                var requestModel = new AuthRedirectToSiteRequesModel() {
                    UserId=int.Parse(userId),
                    SsoUserId= ssoUserId,
                    TargetUrl=data.TargetUrl,
                    FailUrl= ssoConfigs.AuthFail
                };

                var responseStr = httpPostHelper.Send(ssoConfigs.AuthRedirectToSite, JsonConvert.SerializeObject(requestModel));
                var responseObject = JsonConvert.DeserializeObject<AuthRedirectToSiteResponseModel>(responseStr);
                if (responseObject == null || !responseObject.Success || string.IsNullOrWhiteSpace(responseObject.UserToken))
                {
                    return Redirect(ssoConfigs.AuthFail);
                }
                userToken = responseObject.UserToken;
            }

            string url = $"{ssoConfigs.RedirectToSite}?AppKey={ssoConfigs.AppKey}&UserToken={userToken}&UserId={userId}&SsoUserId={ssoUserId}&TargetUrl={data.TargetUrl}&FailUrl={ssoConfigs.AuthFail}";
            return Redirect(url);
        }

        /// <summary>
        /// 回调验证地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public AuthCallbackResponseModel AuthCallback(AuthCallbackRequestModel data)
        {
            var siteToken = HttpContext.Request.Headers[HttpHeaders.SiteToken];
            var disgest = HttpContext.Request.Headers[HttpHeaders.Disgest];

            var reader = new StreamReader(Request.Body);
            var bodyStr = reader.ReadToEnd();
            byte[] body = Encoding.UTF8.GetBytes(bodyStr);
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
        /// 向本系统写入登录信息地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult AuthSession(AuthSessionRequestModel data)
        {
            /*
             * 1.验证 SiteToken 
             * 2.判断被接入系统是否存在 UserMapping,若不存在则新增 User、UserMapping 数据
             * 3.写入登录信息
             * 4.重定向到接入Client传递过来的TargetUrl地址
             * */

            #region 验证 SiteToken 
            if (ssoConfigs.SiteToken != data.SiteToken)
            {
                return Redirect(data.FailUrl);
            }
            #endregion

            //#TODO 调用 Server 的CallBack 验证请求
            #region 补充用户信息    
            /*
             * 1.判断 UserMapping 是否存在
             * 若不存在则有下面2个可能
             *      I. 最开始由a系统接入到b系统，这个时候 a系统中 UserMapping 无数据，b系统有UserMapping 数据。这个时候再由 b 系统到 a 系统，仅仅补充 UserMapping数据
             *      II.从未开始接入，补充 User,UserMapping数据
             * */
            //#TODO
            UserMapping userMapping = data.SsoUserId > 0 ? siteContext.UserMapping.FirstOrDefault(x => x.SsoUserId == data.SsoUserId) : null;
            if (userMapping == null)
            {
                
                if(data.UserId>0)
                {
                    #region a 系统接入 b 系统，b 系统再跳转到 a系统，此时 a 系统补充 UserMapping 关系
                    userMapping = new UserMapping()
                    {
                        UserId = data.UserId,
                        SsoUserId = data.SsoUserId
                    };
                    siteContext.UserMapping.Add(userMapping);
                    siteContext.SaveChanges();
                    #endregion
                }
                else
                {
                    #region 从未接入系统
                   
                    //第三方用户第一进入，自动创建账号关联信息
                    //step a.新增用户到自己系统
                    var user = new User()
                    {
                        UserName = string.Format("{0}_SSO_{1}", data.SsoUserId, data.OtherUserId),
                        PassWord = Guid.NewGuid().ToString()
                    };
                    siteContext.User.Add(user);
                    siteContext.SaveChanges();                   
                   
                    userMapping = new UserMapping()
                    {
                        UserId = user.Id,
                        SsoUserId = data.SsoUserId
                    };
                    siteContext.UserMapping.Add(userMapping);
                    siteContext.SaveChanges();

                    #endregion
                }
            }
            #endregion

            #region 写入登录信息
            //根据ID获取用户信息
            User user2 = siteContext.User.Find(userMapping.UserId);

            //写入登录信息
            loginHelper.Login(HttpContext, user2, false, data.UserToken);
            #endregion

            return Redirect(data.TargetUrl);
        }
    }
}
