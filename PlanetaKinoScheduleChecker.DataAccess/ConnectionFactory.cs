using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class ConnectionFactory
    {
        private static readonly string Connection = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

        public static Func<DbConnection> GetConnection = () => new SqlConnection(Connection);
    }
}
