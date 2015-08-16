using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using Identity.Infrastructure.Services;
using log4net;
//using Ninject;
//using Ninject.Modules;
//using Ninject.Web.Common;

namespace Identity.Rest
{
    //public class WebModule : NinjectModule
    //{
    //    private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    //    public override void Load()
    //    {
    //        Bind<IDbConnection>()
    //            .ToMethod(x =>
    //            {
    //                var con =
    //                    new SqlConnection(
    //                        ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
    //                con.Open();
    //                return con;
    //            })
    //            .InRequestScope();


    //        Bind<IDbTransaction>().ToMethod(x =>
    //        {
    //            var t = x.Kernel.Get<IDbConnection>().BeginTransaction();
    //            return t;
    //        })
    //            .InRequestScope();

    //        Bind<ILoadDtos>().To<DtoLoader>().InRequestScope();
    //    }
    //}
}