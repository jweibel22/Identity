using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using log4net;
using log4net.Repository.Hierarchy;

namespace Identity.Rest
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)] 
    public class UnitOfWorkCommitAttribute : ActionFilterAttribute
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {                        
            var uoW = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IDbTransaction)) as IDbTransaction;

            //log.Info(String.Format("Action: {0}.{1}", actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName, 
            //    actionExecutedContext.ActionContext.ActionDescriptor.ActionName));

            //if (UoW.Connection != null)
            {
                if (actionExecutedContext.Exception == null)
                {
                    uoW.Commit();
                }
                else
                {
                    uoW.Rollback();
                }
            }
        }
    }
}