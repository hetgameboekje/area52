
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace hello_world.Models
{
    public static class Db
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["Area52"].ConnectionString;

        private static IDbConnection Open()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static IEnumerable<T> Query<T>(string sql, object param = null)
        {
            using (var conn = Open())
            {
                return conn.Query<T>(sql, param);
            }
        }

        public static int Execute(string sql, object param = null)
        {
            using (var conn = Open())
            {
                return conn.Execute(sql, param);
            }
        }
    }
}