using System;
using System.Collections.Generic;
using System.Linq;
using ReservationSystem.Database;
using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    public class TableService
    {
        private readonly DB _db;

        public TableService(DB db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all tables
        /// </summary>
        public List<Table> GetAllTables()
        {
            var sql = "SELECT * FROM Tables ORDER BY TableNumber";
            var results = _db.Query(sql);

            return results.Select(MapToTable).ToList();
        }

        /// <summary>
        /// Get available tables for a specific date and time
        /// </summary>
        public List<Table> GetAvailableTables(DateTime date, TimeSpan time)
        {
            var sql = @"SELECT DISTINCT t.* FROM Tables t
                       WHERE t.IsAvailable = 1
                       AND t.Id NOT IN (
                           SELECT rt.TableId 
                           FROM ReservationTables rt
                           INNER JOIN Reservations r ON rt.ReservationId = r.Id
                           WHERE CAST(r.ReservationDate as DATE) = @Date
                           AND r.Status NOT IN (@CancelledStatus)
                           AND ABS(DATEDIFF(MINUTE, CAST(r.ReservationTime as TIME), @Time)) < 120
                       )
                       ORDER BY t.TableNumber";

            var results = _db.Query(sql, new Dictionary<string, object>
            {
                { "@Date", date.Date },
                { "@Time", time.ToString() },
                { "@CancelledStatus", (int)ReservationStatus.Cancelled }
            });

            return results.Select(MapToTable).ToList();
        }

        /// <summary>
        /// Get a table by ID
        /// </summary>
        public Table? GetTable(int id)
        {
            var sql = "SELECT * FROM Tables WHERE Id = @Id";
            var results = _db.Query(sql, new Dictionary<string, object> { { "@Id", id } });

            return results.Count > 0 ? MapToTable(results[0]) : null;
        }

        /// <summary>
        /// Create a new table
        /// </summary>
        public int CreateTable(Table table)
        {
            var sql = @"INSERT INTO Tables (TableNumber, Capacity, IsAvailable) 
                       OUTPUT INSERTED.Id
                       VALUES (@TableNumber, @Capacity, @IsAvailable)";

            var parameters = new Dictionary<string, object>
            {
                { "@TableNumber", table.TableNumber },
                { "@Capacity", table.Capacity },
                { "@IsAvailable", table.IsAvailable }
            };

            return _db.ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// Update a table
        /// </summary>
        public void UpdateTable(Table table)
        {
            var sql = @"UPDATE Tables SET 
                       TableNumber = @TableNumber,
                       Capacity = @Capacity,
                       IsAvailable = @IsAvailable
                       WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", table.Id },
                { "@TableNumber", table.TableNumber },
                { "@Capacity", table.Capacity },
                { "@IsAvailable", table.IsAvailable }
            };

            _db.Execute(sql, parameters);
        }

        /// <summary>
        /// Delete a table
        /// </summary>
        public void DeleteTable(int id)
        {
            _db.Execute("DELETE FROM Tables WHERE Id = @Id",
                       new Dictionary<string, object> { { "@Id", id } });
        }

        private Table MapToTable(Dictionary<string, object> row)
        {
            return new Table
            {
                Id = Convert.ToInt32(row["Id"]),
                TableNumber = row["TableNumber"].ToString() ?? string.Empty,
                Capacity = Convert.ToInt32(row["Capacity"]),
                IsAvailable = Convert.ToBoolean(row["IsAvailable"])
            };
        }
    }
}
