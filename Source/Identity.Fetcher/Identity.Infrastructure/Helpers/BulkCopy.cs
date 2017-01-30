using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Helpers
{
    public static class BulkCopy
    {
        public static void Copy(SqlConnection con, DataTable dataTable, SqlTransaction transaction)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = dataTable.TableName;
                bulkCopy.BulkCopyTimeout = 600;

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    bulkCopy.ColumnMappings.Add(dataTable.Columns[i].ColumnName, dataTable.Columns[i].ColumnName);
                }
                bulkCopy.WriteToServer(dataTable);
            }
        }

        public static void Copy(SqlConnection con, DataTable dataTable)
        {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                {
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    bulkCopy.BulkCopyTimeout = 600;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(dataTable.Columns[i].ColumnName, dataTable.Columns[i].ColumnName);
                    }
                    bulkCopy.WriteToServer(dataTable);
                }
        }

    }
}
