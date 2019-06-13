using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using NHibernate;
using NHibernate.Connection;

namespace CoreSharp.NHibernate.Conventions.Mssql
{
    public static class NameNormalizer
    {
        public static void NormalizeNames(ISession session)
        {
            var type = typeof (NameNormalizer);
            var assembly = type.Assembly;
            var sqlName = $"{type.Namespace}.Monster.sql";

            using (var stream = assembly.GetManifestResourceStream(sqlName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var sql = reader.ReadToEnd();

                    var property = ((ConnectionProvider)session.GetSessionImplementation().Factory.ConnectionProvider).GetType().GetProperty("ConnectionString", BindingFlags.NonPublic | BindingFlags.Instance);
                    var connectionString = (string)property.GetValue(session.GetSessionImplementation().Factory.ConnectionProvider);

                    using (var sqlConnection = new SqlConnection(connectionString))
                    {
                        sqlConnection.Open();

                        try
                        {
                            var cmd = sqlConnection.CreateCommand();
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            // Ignore cautions
                            // http://sqlhints.com/tag/caution-changing-any-part-of-an-object-name-could-break-scripts-and-stored-procedures/
                        }

                        sqlConnection.Close();
                    }
                }
            }
        }
    }
}
