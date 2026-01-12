using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;

namespace hello_world.Models
{
    public class Reservation
    {
        // data (kolommen)
        public int Id { get; set; }
        public string Voornaam { get; set; }
        public string Email { get; set; }

        // connection string vanuit web.config
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["Area52"].ConnectionString;

        public static IEnumerable<Reservation> GetAll()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.Query<Reservation>(@"
                    SELECT
                        Voornaam,
                        Email
                    FROM Gast
                    WHERE Id = @Id;",
                    new { Id = 1 }
                );
            }
        }
    }
}