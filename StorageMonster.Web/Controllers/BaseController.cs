﻿using System.Globalization;
using StorageMonster.Web.Services.ActionAnnotations;
using StorageMonster.Web.Services.Configuration;
using StorageMonster.Web.Services.Security;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace StorageMonster.Web.Controllers
{
    [TempDataTransfer]
    public abstract class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(filterContext.HttpContext.User.Identity is Identity))
                throw new InvalidOperationException("Storage monster custom identity is supported only.");
        }

        private const string DefaultUrlScheme = "http";
        protected string BaseSiteUrl()
        {
            var webConfig = (WebConfigurationSection)ConfigurationManager.GetSection(WebConfigurationSection.SectionLocation);
            if (webConfig.AutoDetectSiteUrl)
            {
                string scheme;
                if (Request == null || Request.Url == null)
                    scheme = DefaultUrlScheme;
                else
                    scheme = Request.Url.Scheme;
                return Url.Action("Index", "Home", null, scheme);
            }
            return webConfig.SiteUrl;
        }

        protected string FullUrlForAction(string action, string controller, object routeValues)
        {
            var webConfig = (WebConfigurationSection)ConfigurationManager.GetSection(WebConfigurationSection.SectionLocation);
            if (webConfig.AutoDetectSiteUrl)
            {
                string scheme;
                if (Request == null || Request.Url == null)
                    scheme = DefaultUrlScheme;
                else
                    scheme = Request.Url.Scheme;
                return Url.Action(action, controller, routeValues, scheme);
            }
            string url = Url.Action(action, controller, routeValues);
            if (url== null)
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Url not found for action {0} and controller {1}", action, controller));
            string relativeUrl = url.TrimStart(new[] { '~', '/' });
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", webConfig.SiteUrl.TrimEnd('/'), relativeUrl);
        }
    }
}
