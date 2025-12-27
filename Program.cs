using System;
using System.Collections.Generic;
using ReservationSystem.Database;
using ReservationSystem.Models;
using ReservationSystem.Services;

namespace ReservationSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example connection string - update with your actual database details
            string connectionString = "Server=localhost;Database=RestaurantReservations;User Id=sa;Password=YourPassword;TrustServerCertificate=True;";

            // Initialize database and services
            var db = new DB(connectionString);
            var reservationService = new ReservationService(db, maxReservationsPerDay: 50);
            var tableService = new TableService(db);

            Console.WriteLine("Restaurant Reservation System");
            Console.WriteLine("============================\n");

            // Example: Create a reservation
            Console.WriteLine("Creating a sample reservation...");
            try
            {
                var reservation = new Reservation
                {
                    CustomerName = "John Doe",
                    CustomerEmail = "john.doe@example.com",
                    CustomerPhone = "+1234567890",
                    ReservationDate = DateTime.Today.AddDays(1),
                    ReservationTime = new TimeSpan(19, 0, 0), // 7:00 PM
                    NumberOfGuests = 4,
                    TableIds = new List<int> { 1, 2 }, // Multi-select: Tables T1 and T2
                    SpecialRequests = "Window seat preferred",
                    Status = ReservationStatus.Pending
                };

                int reservationId = reservationService.CreateReservation(reservation);
                Console.WriteLine($"✓ Reservation created with ID: {reservationId}");

                // Get customer statistics and check for discount
                var stats = reservationService.GetCustomerStatistics(reservation.CustomerEmail);
                Console.WriteLine($"\nCustomer Statistics for {stats.CustomerEmail}:");
                Console.WriteLine($"  - Total completed reservations: {stats.TotalReservations}");
                Console.WriteLine($"  - Eligible for discount: {stats.IsEligibleForDiscount}");
                Console.WriteLine($"  - Discount percentage: {stats.DiscountPercentage}%");

                if (stats.IsEligibleForDiscount)
                {
                    Console.WriteLine($"\n🎉 This customer is eligible for a 20% discount!");
                }

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }

            // Example: Get all reservations
            Console.WriteLine("\n\nFetching all reservations...");
            var allReservations = reservationService.GetAllReservations();
            Console.WriteLine($"Total reservations: {allReservations.Count}");

            foreach (var res in allReservations)
            {
                Console.WriteLine($"\nReservation #{res.Id}:");
                Console.WriteLine($"  Customer: {res.CustomerName} ({res.CustomerEmail})");
                Console.WriteLine($"  Date: {res.ReservationDate:yyyy-MM-dd} at {res.ReservationTime}");
                Console.WriteLine($"  Guests: {res.NumberOfGuests}");
                Console.WriteLine($"  Tables: {string.Join(", ", res.TableIds)}");
                Console.WriteLine($"  Status: {res.Status}");
            }

            // Example: Get available tables
            Console.WriteLine("\n\nFetching available tables for tomorrow at 7:00 PM...");
            var availableTables = tableService.GetAvailableTables(
                DateTime.Today.AddDays(1),
                new TimeSpan(19, 0, 0)
            );

            Console.WriteLine($"Available tables: {availableTables.Count}");
            foreach (var table in availableTables)
            {
                Console.WriteLine($"  - {table.TableNumber} (Capacity: {table.Capacity})");
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
