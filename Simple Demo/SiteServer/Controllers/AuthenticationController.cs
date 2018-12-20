using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SiteServer.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using SiteServer.Core.Data;
using SiteServer.Core.Entity;
using SiteServer.Core.Model;
using SiteClient.Core.Model;
using SiteClient.Core.Enums;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiteServer.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly SiteContext siteContext;
        private readonly HttpPostHelper httpPostHelper ;
        private readonly ComputeHashHelper computeHashHelper ;
        public AuthenticationController(SiteContext _siteContext, HttpPostHelper _httpPostHelper, ComputeHashHelper _computeHashHelper)
        {
            siteContext = _siteContext;
            httpPostHelper = _httpPostHelper;
            computeHashHelper = _computeHashHelper;
        }

        /// <summary>
        /// 获取登录用户的UserToken
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public AuthRedirectToSiteResponseModel AuthRedirectToSite()
        {
            /*
             * 1.验证 AppKey、签名
             * 2.验证 UserToken
             *   a.有SsoUserId，则根据SsoUserId 加上 SourceSiteConfigId、TargerSiteConfigId 获取用户信息
             *   b.无SsoUserId，则根据 UserId、SourceSiteCofnigId 获取用户信息
             * 3.向请求 client 发送 AuthCallBack 回调验证，确认此请求是正常请求还是别人伪造的请求  
             *   若为伪造的请求，则直接返回请求认证失败信息以及提示，终止请求
             * 4.若第2步中，获取不到用户信息，则创建 User 信息
             * 5.返回验证成功信息，以及UserToken
             * */
            AuthRedirectToSiteRequesModel data = null;
            var appKey = HttpContext.Request.Headers[HttpHeaders.AppKey];
            var disgest = HttpContext.Request.Headers[HttpHeaders.Disgest];

            var reader = new StreamReader(Request.Body);
            var bodyStr = reader.ReadToEnd();
            data = JsonConvert.DeserializeObject<AuthRedirectToSiteRequesModel>(bodyStr);

            #region 验证签名
            byte[] body = Encoding.UTF8.GetBytes(bodyStr);
            if (!computeHashHelper.IsComputeHash(disgest, body))
            {
                return new AuthRedirectToSiteResponseModel()
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
                return new AuthRedirectToSiteResponseModel()
                {
                    Success = false,
                    Code = 2,
                    Message = "AppKey wrong."
                };
            }
            #endregion

            #region 验证跳转地址
            Uri TargetUrl = new Uri(data.TargetUrl);
            var tagerHost = TargetUrl.Authority.ToUpper();
            //#TODO 后续需要将 SiteConfig 缓存起来，不要每次查询
            var tagerSiteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.Host == tagerHost);
            if (tagerSiteConfig == null)
            {
                return new AuthRedirectToSiteResponseModel()
                {
                    Success = false,
                    Code = 3,
                    Message = "TargetUrl wrong."
                };
            }
            #endregion

            #region 验证用户是否存在，不存在则新增
            UserMapping userMapping = data.SsoUserId > 0 ? siteContext.UserMapping.FirstOrDefault(x => x.SsoUserId == data.SsoUserId && (x.SourceSiteConfigId == siteConfig.Id || x.SourceSiteConfigId == tagerSiteConfig.Id))
                    : siteContext.UserMapping.FirstOrDefault(x => x.UserId == data.UserId && x.SourceSiteConfigId == siteConfig.Id);
            if (userMapping == null)
            {
                //#TODO 需要事物控制，如果回调认证失败，需要回滚
                userMapping = new UserMapping()
                {
                    UserId = data.UserId,
                    SourceSiteConfigId = siteConfig.Id,
                    TargetSiteConfigId = tagerSiteConfig.Id,
                    Token = Guid.NewGuid()
                };
                siteContext.UserMapping.Add(userMapping);
                siteContext.SaveChanges();

                //#TODO 去 client 获取用户基本信息
                var user = new User()
                {
                    SsoUserId = userMapping.SsoUserId
                };
                siteContext.User.Add(user);
                siteContext.SaveChanges();
                #region 回调验证
                var callBackRequest = new AuthCallbackRequestModel()
                {
                    AuthType = AuthTypeEnum.RedirectToSite,
                    SsoUserId = userMapping.SsoUserId,
                    UserId = userMapping.UserId
                };
                var postData = JsonConvert.SerializeObject(callBackRequest);
                var resrponseStr = httpPostHelper.Send(siteConfig.AuthCallBack, siteConfig, postData);
                var reswponseObject = JsonConvert.DeserializeObject<AuthCallbackResponseModel>(resrponseStr);
                if (reswponseObject == null || !reswponseObject.Success)
                {
                    return new AuthRedirectToSiteResponseModel()
                    {
                        Success = false,
                        Code = 4,
                        Message = "AuthCallback wrong."
                    };
                }
                #endregion
            }
            #endregion

            return new AuthRedirectToSiteResponseModel()
            {
                Success = true,
                Code = 0,
                UserToken = userMapping.Token.ToString()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult RedirectToSite(RedirectToSiteRequestModel data)
        {
            /*
             * 1.验证 AppKey
             * 2.验证用户 token
             * 3.重定向到被接入系统的 AuthSession 写入登录用户信息
             * 注意：步骤1、2验证失败的，则表示认证失败
             **/
            #region 验证 AppKey 
            //#TODO 后续需要将 SiteConfig 缓存起来，不要每次查询
            var siteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.AppKey == data.AppKey);
            if (siteConfig == null)
            {
                //#TODO 将错误码以及错误信息通过URL传递给 client
                return Redirect(data.FailUrl);
            }
            #endregion

            #region 验证跳转地址
            Uri TargetUrl = new Uri(data.TargetUrl);
            var tagerHost = TargetUrl.Authority.ToUpper();
            //#TODO 后续需要将 SiteConfig 缓存起来，不要每次查询
            var tagerSiteConfig = siteContext.SiteConfig.FirstOrDefault(x => x.Host == tagerHost);
            if (tagerSiteConfig == null)
            {
                //#TODO 将错误码以及错误信息通过URL传递给 client
                return Redirect(data.FailUrl);
            }
            #endregion

            #region 验证 UserToken
            UserMapping userMapping = data.SsoUserId > 0 ? siteContext.UserMapping.FirstOrDefault(x => x.SsoUserId == data.SsoUserId && (x.SourceSiteConfigId == siteConfig.Id || x.SourceSiteConfigId == tagerSiteConfig.Id))
                    : siteContext.UserMapping.FirstOrDefault(x => x.UserId == data.UserId && x.SourceSiteConfigId == siteConfig.Id);
            if (userMapping == null || userMapping.Token.ToString() != data.UserToken)
            {
                //#TODO 将错误码以及错误信息通过URL传递给 client
                return Redirect(data.FailUrl);
            }
            #endregion
            int userId = userMapping.SourceSiteConfigId == tagerSiteConfig.Id ? userMapping.UserId : 0;
            int otherUserId = userMapping.SourceSiteConfigId == tagerSiteConfig.Id ? 0 : userMapping.UserId;

            //重定向到被接入 client 写入 session
            var url = $"{tagerSiteConfig.AuthSession}?SiteToken={tagerSiteConfig.SiteToken}&UserId={userId}&OtherUserId={otherUserId}&SsoUserId={userMapping.SsoUserId}&UserToken={userMapping.Token}&TargetUrl={data.TargetUrl}&FailUrl={data.FailUrl}";
            return Redirect(url);
        }
    }
}
