using System;
using System.Collections.Generic;
using ReservationSystem.Database;
using ReservationSystem.Models;
using ReservationSystem.Services;

namespace ReservationSystem.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating all features of the reservation system
    /// </summary>
    public class UsageExamples
    {
        private readonly DB _db;
        private readonly ReservationService _reservationService;
        private readonly TableService _tableService;

        public UsageExamples(string connectionString)
        {
            _db = new DB(connectionString);
            _reservationService = new ReservationService(_db, maxReservationsPerDay: 50);
            _tableService = new TableService(_db);
        }

        /// <summary>
        /// Example 1: Basic database queries using the DB class
        /// </summary>
        public void Example1_DirectDatabaseQueries()
        {
            Console.WriteLine("=== Example 1: Direct Database Queries ===\n");

            // Query all tables
            var tables = _db.Query("SELECT * FROM Tables ORDER BY TableNumber");
            Console.WriteLine($"Found {tables.Count} tables:");
            foreach (var table in tables)
            {
                Console.WriteLine($"  - {table["TableNumber"]}: Capacity {table["Capacity"]}");
            }

            // Query with parameters
            var largeTables = _db.Query(
                "SELECT * FROM Tables WHERE Capacity >= @MinCapacity",
                new Dictionary<string, object> { { "@MinCapacity", 6 } }
            );
            Console.WriteLine($"\nTables with capacity >= 6: {largeTables.Count}");

            // Execute an update
            int rowsUpdated = _db.Execute(
                "UPDATE Tables SET IsAvailable = @IsAvailable WHERE Id = @Id",
                new Dictionary<string, object>
                {
                    { "@Id", 1 },
                    { "@IsAvailable", true }
                }
            );
            Console.WriteLine($"\nUpdated {rowsUpdated} table(s)\n");
        }

        /// <summary>
        /// Example 2: Create a reservation with multiple tables
        /// </summary>
        public void Example2_CreateReservationWithMultipleTables()
        {
            Console.WriteLine("=== Example 2: Create Reservation with Multi-Select Tables ===\n");

            var reservation = new Reservation
            {
                CustomerName = "Alice Johnson",
                CustomerEmail = "alice.johnson@example.com",
                CustomerPhone = "+1555123456",
                ReservationDate = DateTime.Today.AddDays(2),
                ReservationTime = new TimeSpan(18, 30, 0), // 6:30 PM
                NumberOfGuests = 8,
                TableIds = new List<int> { 3, 4 }, // Select tables T3 and T4
                SpecialRequests = "Birthday celebration, need cake storage",
                Status = ReservationStatus.Pending
            };

            try
            {
                int reservationId = _reservationService.CreateReservation(reservation);
                Console.WriteLine($"✓ Reservation #{reservationId} created successfully!");
                Console.WriteLine($"  Customer: {reservation.CustomerName}");
                Console.WriteLine($"  Date: {reservation.ReservationDate:yyyy-MM-dd} at {reservation.ReservationTime}");
                Console.WriteLine($"  Tables: {string.Join(", ", reservation.TableIds)}");
                Console.WriteLine($"  Guests: {reservation.NumberOfGuests}\n");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Example 3: Update a reservation (change tables and guest count)
        /// </summary>
        public void Example3_UpdateReservation()
        {
            Console.WriteLine("=== Example 3: Update Reservation ===\n");

            // First, get the reservation
            var reservation = _reservationService.GetReservation(1);
            if (reservation == null)
            {
                Console.WriteLine("Reservation not found\n");
                return;
            }

            Console.WriteLine("Before update:");
            Console.WriteLine($"  Tables: {string.Join(", ", reservation.TableIds)}");
            Console.WriteLine($"  Guests: {reservation.NumberOfGuests}");
            Console.WriteLine($"  Status: {reservation.Status}");

            // Update the reservation
            reservation.TableIds = new List<int> { 5, 6 }; // Change to different tables
            reservation.NumberOfGuests = 10;
            reservation.Status = ReservationStatus.Confirmed;

            _reservationService.UpdateReservation(reservation);

            Console.WriteLine("\nAfter update:");
            Console.WriteLine($"  Tables: {string.Join(", ", reservation.TableIds)}");
            Console.WriteLine($"  Guests: {reservation.NumberOfGuests}");
            Console.WriteLine($"  Status: {reservation.Status}\n");
        }

        /// <summary>
        /// Example 4: Check customer discount eligibility
        /// </summary>
        public void Example4_CheckDiscountEligibility()
        {
            Console.WriteLine("=== Example 4: Customer Discount System ===\n");

            string[] testCustomers = {
                "john.doe@example.com",
                "alice.johnson@example.com",
                "frequent.diner@example.com"
            };

            foreach (var email in testCustomers)
            {
                var stats = _reservationService.GetCustomerStatistics(email);
                Console.WriteLine($"Customer: {email}");
                Console.WriteLine($"  Total completed reservations: {stats.TotalReservations}");
                Console.WriteLine($"  Eligible for discount: {stats.IsEligibleForDiscount}");

                if (stats.IsEligibleForDiscount)
                {
                    Console.WriteLine($"  🎉 Discount: {stats.DiscountPercentage}% OFF");
                }
                else
                {
                    int remaining = 3 - stats.TotalReservations;
                    Console.WriteLine($"  Need {remaining} more completed reservation(s) for discount");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Example 5: Check table availability and get available tables
        /// </summary>
        public void Example5_CheckTableAvailability()
        {
            Console.WriteLine("=== Example 5: Table Availability ===\n");

            var checkDate = DateTime.Today.AddDays(3);
            var checkTime = new TimeSpan(19, 0, 0); // 7:00 PM

            Console.WriteLine($"Checking availability for {checkDate:yyyy-MM-dd} at {checkTime}");

            var availableTables = _tableService.GetAvailableTables(checkDate, checkTime);

            Console.WriteLine($"\nAvailable tables: {availableTables.Count}");
            foreach (var table in availableTables)
            {
                Console.WriteLine($"  - Table {table.TableNumber}");
                Console.WriteLine($"    Capacity: {table.Capacity} guests");
                Console.WriteLine($"    Status: {(table.IsAvailable ? "Available" : "Unavailable")}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Example 6: Review all reservations
        /// </summary>
        public void Example6_ReviewAllReservations()
        {
            Console.WriteLine("=== Example 6: Review All Reservations ===\n");

            var allReservations = _reservationService.GetAllReservations();
            Console.WriteLine($"Total reservations: {allReservations.Count}\n");

            foreach (var res in allReservations)
            {
                Console.WriteLine($"Reservation #{res.Id}");
                Console.WriteLine($"  Customer: {res.CustomerName}");
                Console.WriteLine($"  Email: {res.CustomerEmail}");
                Console.WriteLine($"  Date: {res.ReservationDate:yyyy-MM-dd} at {res.ReservationTime}");
                Console.WriteLine($"  Tables: {string.Join(", ", res.TableIds)}");
                Console.WriteLine($"  Guests: {res.NumberOfGuests}");
                Console.WriteLine($"  Status: {res.Status}");
                Console.WriteLine($"  Created: {res.CreatedAt:yyyy-MM-dd HH:mm}");

                if (!string.IsNullOrEmpty(res.SpecialRequests))
                {
                    Console.WriteLine($"  Special Requests: {res.SpecialRequests}");
                }

                // Check if customer is eligible for discount
                var stats = _reservationService.GetCustomerStatistics(res.CustomerEmail);
                if (stats.IsEligibleForDiscount)
                {
                    Console.WriteLine($"  💰 Customer eligible for {stats.DiscountPercentage}% discount!");
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Example 7: Test reservation limit enforcement
        /// </summary>
        public void Example7_TestReservationLimit()
        {
            Console.WriteLine("=== Example 7: Reservation Limit Enforcement ===\n");

            var targetDate = DateTime.Today.AddDays(10);

            // Check current count for this date
            var currentCount = _db.Query(
                "SELECT COUNT(*) as Count FROM Reservations WHERE CAST(ReservationDate as DATE) = @Date",
                new Dictionary<string, object> { { "@Date", targetDate.Date } }
            );

            int existing = Convert.ToInt32(currentCount[0]["Count"]);
            Console.WriteLine($"Current reservations for {targetDate:yyyy-MM-dd}: {existing}");
            Console.WriteLine($"Limit: 50 reservations per day\n");

            if (existing >= 50)
            {
                Console.WriteLine("⚠️  Limit already reached for this date");
            }
            else
            {
                Console.WriteLine($"✓ Can accept {50 - existing} more reservations for this date");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Example 8: Complete workflow - Create, Confirm, Complete
        /// </summary>
        public void Example8_CompleteWorkflow()
        {
            Console.WriteLine("=== Example 8: Complete Reservation Workflow ===\n");

            // Step 1: Create reservation
            Console.WriteLine("Step 1: Creating new reservation...");
            var reservation = new Reservation
            {
                CustomerName = "Bob Smith",
                CustomerEmail = "bob.smith@example.com",
                CustomerPhone = "+1555987654",
                ReservationDate = DateTime.Today.AddDays(1),
                ReservationTime = new TimeSpan(20, 0, 0),
                NumberOfGuests = 4,
                TableIds = new List<int> { 1, 2 },
                Status = ReservationStatus.Pending
            };

            int reservationId = _reservationService.CreateReservation(reservation);
            Console.WriteLine($"✓ Created reservation #{reservationId} with status: {reservation.Status}\n");

            // Step 2: Confirm reservation
            Console.WriteLine("Step 2: Confirming reservation...");
            reservation.Id = reservationId;
            reservation.Status = ReservationStatus.Confirmed;
            _reservationService.UpdateReservation(reservation);
            Console.WriteLine($"✓ Reservation confirmed\n");

            // Step 3: Complete reservation (after the dinner)
            Console.WriteLine("Step 3: Completing reservation...");
            reservation.Status = ReservationStatus.Completed;
            _reservationService.UpdateReservation(reservation);
            Console.WriteLine($"✓ Reservation completed\n");

            // Step 4: Check updated statistics
            Console.WriteLine("Step 4: Checking customer statistics...");
            var stats = _reservationService.GetCustomerStatistics(reservation.CustomerEmail);
            Console.WriteLine($"Customer now has {stats.TotalReservations} completed reservation(s)");
            Console.WriteLine($"Discount eligible: {stats.IsEligibleForDiscount}");
            Console.WriteLine();
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public void RunAllExamples()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Restaurant Reservation System - Usage Examples   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Example1_DirectDatabaseQueries();
            Example2_CreateReservationWithMultipleTables();
            Example3_UpdateReservation();
            Example4_CheckDiscountEligibility();
            Example5_CheckTableAvailability();
            Example6_ReviewAllReservations();
            Example7_TestReservationLimit();
            Example8_CompleteWorkflow();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║           All Examples Completed!                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
        }
    }
}
