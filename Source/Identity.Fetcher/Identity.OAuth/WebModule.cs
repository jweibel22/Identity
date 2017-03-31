using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.OAuth;
using Identity.OAuth.EventHandlers;
using log4net;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Identity.Rest
{
    public class WebModule : NinjectModule
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Load()
        {
            Bind<IDbConnection>()
                .ToMethod(x =>
                {
                    var con =
                        new SqlConnection(
                            ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
                    //log.Info("Opening connection ... " + RequestScope.Scope + " - " + con.ClientConnectionId);
                    con.Open();
                    
                    return con;
                })
                .InScope(c => RequestScope.Scope.Value);


            Bind<IDbTransaction>().ToMethod(x =>
            {
                var t = x.Kernel.Get<IDbConnection>().BeginTransaction();
                return t;
            }).InScope(c => RequestScope.Scope.Value);

            Bind<ILoadDtos>().To<DtoLoader>().InScope(c => RequestScope.Scope.Value);
            Bind<ChannelRepository>().ToSelf().InScope(c => RequestScope.Scope.Value);
            Bind<CommentRepostitory>().ToSelf().InScope(c => RequestScope.Scope.Value);
            Bind<PostRepository>().ToSelf().InScope(c => RequestScope.Scope.Value);
            Bind<UserRepository>().ToSelf().InScope(c => RequestScope.Scope.Value);
            Bind<Bus>().ToSelf().InSingletonScope();

            //TODO: keeping all channel links in memory is of course not scalable!
            Bind<ChannelLinkGraphCache>().ToSelf().InSingletonScope();
            Bind<ChannelLinkGraph>().ToMethod(x =>
            {
                var repo = x.Kernel.Get<ChannelLinkRepository>();
                return repo.GetGraph();
            }).InSingletonScope();

            Bind<ChannelLinkEventBatch>().ToSelf().InScope(c => RequestScope.Scope.Value);

            Bind<IArticleContentsFetcher>().To<ArticleContentsFetcher>().InScope(c => RequestScope.Scope.Value);
        }
    }
}