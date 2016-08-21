using System.Data;
using System.Data.SqlClient;

namespace Identity.Infrastructure
{
    public class ConnectionFactory
    {
        private readonly string connectionString;

        public ConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbTransaction NewTransaction()
        {
            var con = new SqlConnection(connectionString);
            con.Open();
            return con.BeginTransaction();
        }
    }
}