﻿using CloudBin.Core;
using CloudBin.Web.Core.Configuration;
using System;
using System.Linq;
using System.Web;

namespace CloudBin.Web.Core
{
    public class OpenDatabaseConnectionPolicy : IOpenDatabaseConnectionPolicy
    {
        protected readonly IWebConfiguration WebConfiguration;
        protected readonly object Locker = new object();
        protected volatile Func<HttpContext, bool>[] PolicyCheckerInternal;
        protected Func<HttpContext, bool>[] PolicyCheckers
        {
            get
            {
                if (PolicyCheckerInternal == null)
                {
                    lock (Locker)
                    {
                        if (PolicyCheckerInternal == null)
                        {
                            PolicyCheckerInternal = RequestCheckersFactory.CreateStaticContentCheckers(HttpContext.Current);
                        }
                    }
                }
                return PolicyCheckerInternal;
            }
        }

        public OpenDatabaseConnectionPolicy(IWebConfiguration webConfiguration)
        {
            WebConfiguration = webConfiguration;
        }
        bool IOpenDatabaseConnectionPolicy.DatabaseConnectionRequired(HttpContext context)
        {   
            if (!WebConfiguration.DoNotOpenDbSessionForScriptAndContent)
            {
                return true;
            }
            return RequestContext.Current.LookUpValue("db_open_required", () => !PolicyCheckers.Any(checker => checker(context)));
        }
    }
}