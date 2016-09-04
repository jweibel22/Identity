using System;
using System.Data;
using System.Data.SqlClient;

namespace Identity.Infrastructure
{
    public class DbSession : IDisposable
    {
        private readonly SqlConnection connection;
        private readonly IDbTransaction transaction;

        public DbSession(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public IDbTransaction Transaction
        {
            get
            {
                return transaction;
            }
        }

        public void Dispose()
        {
            transaction.Dispose();
            connection.Close();
        }
    }

    public class ConnectionFactory
    {
        private readonly string connectionString;

        public ConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSession NewTransaction()
        {
            return new DbSession(connectionString);            
        }
    }
}