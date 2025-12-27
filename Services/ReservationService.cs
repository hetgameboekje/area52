using System;
using System.Collections.Generic;
using System.Linq;
using ReservationSystem.Database;
using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    public class ReservationService
    {
        private readonly DB _db;
        private readonly int _maxReservationsPerDay;

        public ReservationService(DB db, int maxReservationsPerDay = 50)
        {
            _db = db;
            _maxReservationsPerDay = maxReservationsPerDay;
        }

        /// <summary>
        /// Create a new reservation
        /// </summary>
        public int CreateReservation(Reservation reservation)
        {
            // Check if limit is reached
            if (!CanAcceptReservation(reservation.ReservationDate))
            {
                throw new InvalidOperationException($"Reservation limit reached for {reservation.ReservationDate:yyyy-MM-dd}");
            }

            // Validate tables availability
            if (!AreTablesAvailable(reservation.TableIds, reservation.ReservationDate, reservation.ReservationTime))
            {
                throw new InvalidOperationException("One or more selected tables are not available for the specified time");
            }

            var sql = @"INSERT INTO Reservations (CustomerName, CustomerEmail, CustomerPhone, ReservationDate, 
                       ReservationTime, NumberOfGuests, SpecialRequests, Status, CreatedAt) 
                       OUTPUT INSERTED.Id
                       VALUES (@CustomerName, @CustomerEmail, @CustomerPhone, @ReservationDate, 
                       @ReservationTime, @NumberOfGuests, @SpecialRequests, @Status, @CreatedAt)";

            var parameters = new Dictionary<string, object>
            {
                { "@CustomerName", reservation.CustomerName },
                { "@CustomerEmail", reservation.CustomerEmail },
                { "@CustomerPhone", reservation.CustomerPhone },
                { "@ReservationDate", reservation.ReservationDate },
                { "@ReservationTime", reservation.ReservationTime.ToString() },
                { "@NumberOfGuests", reservation.NumberOfGuests },
                { "@SpecialRequests", reservation.SpecialRequests ?? (object)DBNull.Value },
                { "@Status", (int)reservation.Status },
                { "@CreatedAt", DateTime.Now }
            };

            int reservationId = _db.ExecuteScalar(sql, parameters);

            // Insert table associations
            foreach (var tableId in reservation.TableIds)
            {
                var tableSql = "INSERT INTO ReservationTables (ReservationId, TableId) VALUES (@ReservationId, @TableId)";
                _db.Execute(tableSql, new Dictionary<string, object>
                {
                    { "@ReservationId", reservationId },
                    { "@TableId", tableId }
                });
            }

            return reservationId;
        }

        /// <summary>
        /// Get a reservation by ID
        /// </summary>
        public Reservation? GetReservation(int id)
        {
            var sql = "SELECT * FROM Reservations WHERE Id = @Id";
            var results = _db.Query(sql, new Dictionary<string, object> { { "@Id", id } });

            if (results.Count == 0)
                return null;

            var reservation = MapToReservation(results[0]);

            // Get associated tables
            var tablesSql = "SELECT TableId FROM ReservationTables WHERE ReservationId = @ReservationId";
            var tableResults = _db.Query(tablesSql, new Dictionary<string, object> { { "@ReservationId", id } });
            reservation.TableIds = tableResults.Select(r => Convert.ToInt32(r["TableId"])).ToList();

            return reservation;
        }

        /// <summary>
        /// Get all reservations
        /// </summary>
        public List<Reservation> GetAllReservations()
        {
            var sql = "SELECT * FROM Reservations ORDER BY ReservationDate DESC, ReservationTime DESC";
            var results = _db.Query(sql);

            var reservations = new List<Reservation>();
            foreach (var row in results)
            {
                var reservation = MapToReservation(row);

                // Get associated tables
                var tablesSql = "SELECT TableId FROM ReservationTables WHERE ReservationId = @ReservationId";
                var tableResults = _db.Query(tablesSql, new Dictionary<string, object> { { "@ReservationId", reservation.Id } });
                reservation.TableIds = tableResults.Select(r => Convert.ToInt32(r["TableId"])).ToList();

                reservations.Add(reservation);
            }

            return reservations;
        }

        /// <summary>
        /// Update an existing reservation
        /// </summary>
        public void UpdateReservation(Reservation reservation)
        {
            var sql = @"UPDATE Reservations SET 
                       CustomerName = @CustomerName,
                       CustomerEmail = @CustomerEmail,
                       CustomerPhone = @CustomerPhone,
                       ReservationDate = @ReservationDate,
                       ReservationTime = @ReservationTime,
                       NumberOfGuests = @NumberOfGuests,
                       SpecialRequests = @SpecialRequests,
                       Status = @Status,
                       UpdatedAt = @UpdatedAt
                       WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", reservation.Id },
                { "@CustomerName", reservation.CustomerName },
                { "@CustomerEmail", reservation.CustomerEmail },
                { "@CustomerPhone", reservation.CustomerPhone },
                { "@ReservationDate", reservation.ReservationDate },
                { "@ReservationTime", reservation.ReservationTime.ToString() },
                { "@NumberOfGuests", reservation.NumberOfGuests },
                { "@SpecialRequests", reservation.SpecialRequests ?? (object)DBNull.Value },
                { "@Status", (int)reservation.Status },
                { "@UpdatedAt", DateTime.Now }
            };

            _db.Execute(sql, parameters);

            // Update table associations
            _db.Execute("DELETE FROM ReservationTables WHERE ReservationId = @ReservationId",
                       new Dictionary<string, object> { { "@ReservationId", reservation.Id } });

            foreach (var tableId in reservation.TableIds)
            {
                var tableSql = "INSERT INTO ReservationTables (ReservationId, TableId) VALUES (@ReservationId, @TableId)";
                _db.Execute(tableSql, new Dictionary<string, object>
                {
                    { "@ReservationId", reservation.Id },
                    { "@TableId", tableId }
                });
            }
        }

        /// <summary>
        /// Delete a reservation
        /// </summary>
        public void DeleteReservation(int id)
        {
            _db.Execute("DELETE FROM ReservationTables WHERE ReservationId = @ReservationId",
                       new Dictionary<string, object> { { "@ReservationId", id } });
            _db.Execute("DELETE FROM Reservations WHERE Id = @Id",
                       new Dictionary<string, object> { { "@Id", id } });
        }

        /// <summary>
        /// Get customer statistics including discount eligibility
        /// </summary>
        public CustomerStatistics GetCustomerStatistics(string email)
        {
            var sql = @"SELECT COUNT(*) as TotalReservations 
                       FROM Reservations 
                       WHERE CustomerEmail = @Email AND Status = @CompletedStatus";

            var results = _db.Query(sql, new Dictionary<string, object>
            {
                { "@Email", email },
                { "@CompletedStatus", (int)ReservationStatus.Completed }
            });

            var totalReservations = results.Count > 0 ? Convert.ToInt32(results[0]["TotalReservations"]) : 0;

            return new CustomerStatistics
            {
                CustomerEmail = email,
                TotalReservations = totalReservations
            };
        }

        /// <summary>
        /// Check if a reservation can be accepted based on daily limit
        /// </summary>
        private bool CanAcceptReservation(DateTime date)
        {
            var sql = "SELECT COUNT(*) as Count FROM Reservations WHERE CAST(ReservationDate as DATE) = @Date";
            var results = _db.Query(sql, new Dictionary<string, object> { { "@Date", date.Date } });

            var count = results.Count > 0 ? Convert.ToInt32(results[0]["Count"]) : 0;
            return count < _maxReservationsPerDay;
        }

        /// <summary>
        /// Check if tables are available for the given date and time
        /// </summary>
        private bool AreTablesAvailable(List<int> tableIds, DateTime date, TimeSpan time)
        {
            if (tableIds.Count == 0)
                return true;

            var tableIdsStr = string.Join(",", tableIds);
            var sql = $@"SELECT COUNT(*) as Count FROM ReservationTables RT
                        INNER JOIN Reservations R ON RT.ReservationId = R.Id
                        WHERE RT.TableId IN ({tableIdsStr})
                        AND CAST(R.ReservationDate as DATE) = @Date
                        AND R.Status NOT IN (@CancelledStatus)
                        AND ABS(DATEDIFF(MINUTE, CAST(R.ReservationTime as TIME), @Time)) < 120";

            var results = _db.Query(sql, new Dictionary<string, object>
            {
                { "@Date", date.Date },
                { "@Time", time.ToString() },
                { "@CancelledStatus", (int)ReservationStatus.Cancelled }
            });

            var count = results.Count > 0 ? Convert.ToInt32(results[0]["Count"]) : 0;
            return count == 0;
        }

        private Reservation MapToReservation(Dictionary<string, object> row)
        {
            return new Reservation
            {
                Id = Convert.ToInt32(row["Id"]),
                CustomerName = row["CustomerName"].ToString() ?? string.Empty,
                CustomerEmail = row["CustomerEmail"].ToString() ?? string.Empty,
                CustomerPhone = row["CustomerPhone"].ToString() ?? string.Empty,
                ReservationDate = Convert.ToDateTime(row["ReservationDate"]),
                ReservationTime = TimeSpan.Parse(row["ReservationTime"].ToString() ?? "00:00:00"),
                NumberOfGuests = Convert.ToInt32(row["NumberOfGuests"]),
                SpecialRequests = row["SpecialRequests"]?.ToString(),
                Status = (ReservationStatus)Convert.ToInt32(row["Status"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : null
            };
        }
    }
}
