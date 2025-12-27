using System;
using System.Collections.Generic;

namespace ReservationSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public List<int> TableIds { get; set; } = new List<int>();
        public string? SpecialRequests { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Table
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CustomerStatistics
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public int TotalReservations { get; set; }
        public bool IsEligibleForDiscount => TotalReservations >= 3;
        public decimal DiscountPercentage => IsEligibleForDiscount ? 20m : 0m;
    }
}
