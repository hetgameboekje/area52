# Area52 - Restaurant Reservation System

A complete C# backend application for managing restaurant table reservations.

## Quick Start

See [DOCUMENTATION.md](DOCUMENTATION.md) for detailed usage instructions and API reference.

## Features

- ✅ **CRUD Operations** - Full reservation management
- ✅ **Multi-Select Tables** - Assign multiple tables by ID to each reservation
- ✅ **Reservation Limits** - Set maximum reservations per day
- ✅ **Automatic Discounts** - 20% off for customers with 3+ completed reservations
- ✅ **Database Template** - Simple DB class with `Query()` and `Execute()` methods
- ✅ **Table Management** - Manage restaurant tables and check availability

## Project Structure

```
├── Database/DB.cs              # Database helper class
├── Models/Reservation.cs       # Data models
├── Services/                   # Business logic
│   ├── ReservationService.cs
│   └── TableService.cs
├── SQL/schema.sql             # Database schema
└── Program.cs                 # Example usage
```

## Quick Example

```csharp
// Initialize
var db = new DB(connectionString);
var reservationService = new ReservationService(db);

// Create reservation with multiple tables
var reservation = new Reservation
{
    CustomerName = "John Doe",
    CustomerEmail = "john@example.com",
    TableIds = new List<int> { 1, 2 }, // Multi-select
    ReservationDate = DateTime.Today.AddDays(1),
    ReservationTime = new TimeSpan(19, 0, 0),
    NumberOfGuests = 4
};

int id = reservationService.CreateReservation(reservation);

// Check for discount
var stats = reservationService.GetCustomerStatistics("john@example.com");
if (stats.IsEligibleForDiscount)
{
    Console.WriteLine($"Customer gets {stats.DiscountPercentage}% off!");
}
```

## Database Setup

1. Create database: `CREATE DATABASE RestaurantReservations;`
2. Run schema: `sqlcmd -S localhost -d RestaurantReservations -i SQL/schema.sql`
3. Update connection string in `Program.cs`
4. Build and run: `dotnet run`

For complete documentation, see [DOCUMENTATION.md](DOCUMENTATION.md).
