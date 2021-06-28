using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace uStoreInstallerCommonLogic
{
    public class DbQueries
    {
        public string ConnectString { get; set; }
        public List<string> Queries { get; set; }
    }

    public class DbCommand
    {
        public static string GetDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        public static void ExecuteTransaction(List<DbQueries> dbQueries)
        {
            foreach (var dbQuery in dbQueries)
            {
                using (var con = new SqlConnection(dbQuery.ConnectString))
                {
                    string sqlCmd = string.Empty;
                    try
                    {
                        con.Open();
                        if (con.State == ConnectionState.Open)
                        {
                            var transaction = con.BeginTransaction();
                            try
                            {
                                foreach (string query in dbQuery
                                    .Queries
                                    .Select(_ => string.Format(_, con.Database)))
                                {
                                    sqlCmd = query;
                                    using (var cmd = new SqlCommand(query, con))
                                    {

                                        //MessageBox.Show("query: " + query);
                                        cmd.Transaction = transaction;
                                        cmd.CommandType = CommandType.Text;
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Error during execute sql query: '{0}'", sqlCmd);
                        MessageBox.Show(message, "SQL execution error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        var sqlException = new SystemException(
                            message,
                            ex);
                        throw sqlException;
                    }
                }
            }
        }

        public static void Execute(List<DbQueries> dbQueries)
        {
            foreach (var dbQuery in dbQueries)
            {
                using (var con = new SqlConnection(dbQuery.ConnectString))
                {
                    con.Open();
                    if (con.State == ConnectionState.Open)
                    {
                        foreach (string query in dbQuery
                            .Queries
                            .Select(_ => string.Format(_, con.Database)))
                        {
                            using (var cmd = new SqlCommand(query, con))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        public static DbQueries CreateDbQueries(string connStr, params string[] queries)
        {
            List<string> list = new List<string>();

            foreach (var query in queries)
            {
                if (!String.IsNullOrWhiteSpace(query)) list.Add(query);
            }

            DbQueries dbQueries = new DbQueries { ConnectString = connStr, Queries = list };

            return dbQueries;
        }
    }
}
