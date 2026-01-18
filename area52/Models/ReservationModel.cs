using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace area52.Models
{
    public class ReservationModel
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["Area52"].ConnectionString;
        
        // Properties for form input
        public int Reservering_ID { get; set; }
        public int GastID { get; set; }
        public int Aantal_Personen { get; set; }
        public DateTime Reserveringsdatum { get; set; }
        public decimal Totaal_Bedrag { get; set; }
        public string Opmerkingen { get; set; }

        // Get all reservations
        public static IEnumerable<ReservationModel> GetReservations()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.Query<ReservationModel>(@"
                    SELECT  R.Reservering_ID
                            ,R.GastID
                            ,R.Aantal_Personen
                            ,R.Reserveringsdatum
                            ,R.Totaal_Bedrag
                            ,R.Opmerkingen
                    FROM Reservering R
                    ORDER BY R.Reservering_ID DESC
                    LIMIT 10;"
                );
            }
        }

        // Create new reservation from form data
        public static int CreateReservation(int aantalpersonen, DateTime reserveringsdatum, string opmerkingen)
        {
            // Transform form data to variables
            int aantal = aantalpersonen;
            DateTime datum = reserveringsdatum;
            string opmerking = opmerkingen ?? string.Empty;
            decimal totaalBedrag = aantal * 20; 
            int gastID = 1;

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                
                var result = connection.Execute(@"
                    INSERT INTO Reservering (GastID, Aantal_Personen, Reserveringsdatum, Totaal_Bedrag, Opmerkingen)
                    VALUES (@GastID, @AantalPersonen, @ReserveringsDatum, @TotaalBedrag, @Opmerkingen);
                    SELECT LAST_INSERT_ID();",
                    new
                    {
                        GastID = gastID
                        ,AantalPersonen = aantal
                        ,ReserveringsDatum = datum
                        ,TotaalBedrag = totaalBedrag
                        ,Opmerkingen = opmerking
                    }
                );

                return result;
            }
        }
        
        public static ReservationModel FillForm(int id)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                return connection.QuerySingleOrDefault<ReservationModel>(@"
            SELECT  R.Reservering_ID,
                    R.GastID,
                    R.Aantal_Personen,
                    R.Reserveringsdatum,
                    R.Totaal_Bedrag,
                    R.Opmerkingen
            FROM Reservering R
            WHERE R.Reservering_ID = @Id
            LIMIT 1;",
                    new
                    {
                        Id = id
                    }
                );
            }
        }
    }
}
