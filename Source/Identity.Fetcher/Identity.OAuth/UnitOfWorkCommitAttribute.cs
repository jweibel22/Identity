using System;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using log4net;

namespace Identity.Rest
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)] 
    public class UnitOfWorkCommitAttribute : ActionFilterAttribute
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //log.Debug(actionContext.ActionDescriptor.ActionName + " executing");            
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var uoW = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IDbTransaction)) as IDbTransaction;
            var con = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IDbConnection)) as IDbConnection;
            var channelEventBatch = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(ChannelLinkEventBatch)) as ChannelLinkEventBatch;
            try
            {
                if (actionExecutedContext.Exception == null)
                {
                    uoW.Commit();
                    channelEventBatch.Commit();
                }
                else
                {
                    uoW.Rollback();
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to commit", ex);
            }
            finally
            {
                con.Dispose();
            }
        }
    }
}