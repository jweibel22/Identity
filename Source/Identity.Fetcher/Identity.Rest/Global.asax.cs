using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Identity.Domain;

namespace Identity.Rest
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        //public static IDbConnection con;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AutoMapper.Mapper.CreateMap<Channel, Infrastructure.DTO.Channel>()
                .ForMember(x => x.IsPrivate, _ => _.MapFrom(src => !src.IsPublic));

            AutoMapper.Mapper.CreateMap<Post, Infrastructure.DTO.Post>()
                .ForMember(x => x.Type, _ => _.UseValue("link"));

            AutoMapper.Mapper.CreateMap<Comment, Infrastructure.DTO.Comment>()
                .ForMember(x => x.Body, _ => _.MapFrom(src => src.Text));

            AutoMapper.Mapper.CreateMap<RssFeeder, Infrastructure.DTO.RssFeeder>();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //con = new SqlConnection(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
            //con.Open();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            //con.Close();
            //con.Dispose();
        }
    }
}
