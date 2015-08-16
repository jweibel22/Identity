using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using Dapper;
using Identity.Domain;
using Identity.OAuth.Models;

namespace Identity.OAuth
{
    public class AuthRepository : IDisposable
    {
        private readonly IDbConnection con;

        public AuthRepository()
        {
            con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Identity;Persist Security Info=True;User ID=sa;Password=pass1word.;Connect Timeout=30");
            con.Open();
        }

        public void RegisterUser(UserModel userModel)
        {
            con.Execute("insert [User] values (@Username, @SavedChannel, @StarredChannel,@LikedChannel,@IdentityId)",
                new
                {
                    Username = userModel.UserName,
                    SavedChannel = 0,
                    StarredChannel = 0,
                    LikedChannel = 0,
                    IdentityId = Guid.NewGuid().ToString()
                });
        }

        //TODO: add hashed password to user model
        public User FindUser(string userName, string password)
        {
            return con.Query<User>("select * from [User] where username = @Username",
                new
                {
                    Username = userName,
                    Password = password
                }).SingleOrDefault();
        }

        public Client FindClient(string clientId)
        {
            if (clientId == "ngAuthApp")
            {
                return new Client
                {
                    Active = true,
                    AllowedOrigin = "http://localhost:51863",
                    ApplicationType = ApplicationTypes.JavaScript,
                    Secret = "5YV7M1r981yoGhELyB84aC+KiYksxZf1OY3++C1CtRM=",
                    Id = "ngAuthApp",
                    RefreshTokenLifeTime = 7200,
                    Name = ""
                };
            }
            else
            {
                return null;
            }
        }

        public User Find(string loginProvider, string providerKey)
        {
            return con.Query<User>(
                "select * from [User] join UserLogins on [User].Id = UserLogins.UserId where UserLogins.ProviderKey = @ProviderKey AND UserLogins.LoginProvider = @LoginProvider",
                new { LoginProvider = loginProvider, ProviderKey = providerKey })
                .SingleOrDefault();
        }

        public User Create(string userId, string username)
        {
            con.Execute("insert [User] values (@Username, @SavedChannel, @StarredChannel,@LikedChannel, @IdentityId)",
                new
                {
                    Username = username,
                    SavedChannel = 0,
                    StarredChannel = 0,
                    LikedChannel = 0,
                    IdentityId = userId
                });

            return
                con.Query<User>("select * from [User] where IdentityId=@IdentityId", new {IdentityId = userId})
                    .SingleOrDefault();
        }

        public void AddLogin(string userId, string loginProvider, string providerKey)
        {
            var user = con.Query<User>("select * from [User] where IdentityId = @UserId", new { UserId = userId }).SingleOrDefault();

            if (user == null)
            {
                throw new Exception("unknown user with id " + userId);
            }

            con.Execute("insert UserLogins values (@UserId, @ProviderKey, @LoginProvider)", new
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            });
        }

        public void Dispose()
        {

        }
    }
}