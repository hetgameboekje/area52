# Restaurant Reservation System

A C# backend application for managing restaurant table reservations with CRUD operations, multi-select table functionality, reservation limits, and automatic discount calculation.

## Features

### Core Functionality
- **CRUD Operations**: Full Create, Read, Update, Delete operations for reservations
- **Multi-Select Tables**: Select multiple tables (by ID) for a single reservation
- **Reservation Limits**: Set a maximum number of reservations per day (default: 50)
- **Customer Discounts**: Automatic 20% discount for customers with 3+ completed reservations
- **Table Management**: Manage restaurant tables with capacity and availability
- **Availability Checking**: Verify table availability for specific dates and times

### Database Template
The project includes a database helper class (`DB`) with:
- **`Query()`** - Execute SQL queries and return results as a list of dictionaries
- **`Execute()`** - Execute INSERT, UPDATE, DELETE commands and return affected rows
- **`ExecuteScalar()`** - Execute commands and return a scalar value (useful for getting inserted IDs)

## Project Structure

```
ReservationSystem/
├── Database/
│   └── DB.cs                    # Database helper class with Query and Execute methods
├── Models/
│   └── Reservation.cs           # Data models (Reservation, Table, CustomerStatistics)
├── Services/
│   ├── ReservationService.cs    # CRUD operations for reservations
│   └── TableService.cs          # CRUD operations for tables
├── SQL/
│   └── schema.sql               # Database schema and sample data
├── Program.cs                   # Example usage and demonstration
└── ReservationSystem.csproj     # Project file
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (Express, LocalDB, or full version)

### Database Setup

1. Create a new database in SQL Server:
```sql
CREATE DATABASE RestaurantReservations;
```

2. Execute the schema script:
```bash
sqlcmd -S localhost -d RestaurantReservations -i SQL/schema.sql
```

Or run it manually in SQL Server Management Studio.

### Configuration

Update the connection string in `Program.cs`:
```csharp
string connectionString = "Server=localhost;Database=RestaurantReservations;User Id=sa;Password=YourPassword;TrustServerCertificate=True;";
```

### Build and Run

```bash
dotnet build
dotnet run
```

## Usage Examples

### 1. Initialize the Database Connection

```csharp
var db = new DB(connectionString);
var reservationService = new ReservationService(db, maxReservationsPerDay: 50);
var tableService = new TableService(db);
```

### 2. Create a Reservation with Multi-Select Tables

```csharp
var reservation = new Reservation
{
    CustomerName = "John Doe",
    CustomerEmail = "john.doe@example.com",
    CustomerPhone = "+1234567890",
    ReservationDate = DateTime.Today.AddDays(1),
    ReservationTime = new TimeSpan(19, 0, 0), // 7:00 PM
    NumberOfGuests = 4,
    TableIds = new List<int> { 1, 2 }, // Multi-select: Tables with IDs 1 and 2
    SpecialRequests = "Window seat preferred",
    Status = ReservationStatus.Pending
};

int reservationId = reservationService.CreateReservation(reservation);
Console.WriteLine($"Reservation created with ID: {reservationId}");
```

### 3. Get a Reservation

```csharp
var reservation = reservationService.GetReservation(reservationId);
if (reservation != null)
{
    Console.WriteLine($"Customer: {reservation.CustomerName}");
    Console.WriteLine($"Tables: {string.Join(", ", reservation.TableIds)}");
}
```

### 4. Update a Reservation

```csharp
reservation.NumberOfGuests = 6;
reservation.TableIds = new List<int> { 3, 4 }; // Change to different tables
reservation.Status = ReservationStatus.Confirmed;
reservationService.UpdateReservation(reservation);
```

### 5. Delete a Reservation

```csharp
reservationService.DeleteReservation(reservationId);
```

### 6. Check Customer Discount Eligibility

```csharp
var stats = reservationService.GetCustomerStatistics("john.doe@example.com");
Console.WriteLine($"Total completed reservations: {stats.TotalReservations}");
Console.WriteLine($"Eligible for discount: {stats.IsEligibleForDiscount}");
Console.WriteLine($"Discount percentage: {stats.DiscountPercentage}%");

if (stats.IsEligibleForDiscount)
{
    Console.WriteLine("Customer gets 20% off!");
}
```

### 7. Get Available Tables

```csharp
var availableTables = tableService.GetAvailableTables(
    DateTime.Today.AddDays(1),
    new TimeSpan(19, 0, 0) // 7:00 PM
);

foreach (var table in availableTables)
{
    Console.WriteLine($"Table {table.TableNumber}: Capacity {table.Capacity}");
}
```

### 8. Direct Database Queries with DB Class

```csharp
// Using Query method
var results = db.Query("SELECT * FROM Tables WHERE Capacity >= @Capacity", 
    new Dictionary<string, object> { { "@Capacity", 4 } });

foreach (var row in results)
{
    Console.WriteLine($"Table: {row["TableNumber"]}, Capacity: {row["Capacity"]}");
}

// Using Execute method
int rowsAffected = db.Execute(
    "UPDATE Tables SET IsAvailable = @IsAvailable WHERE Id = @Id",
    new Dictionary<string, object> 
    { 
        { "@Id", 1 },
        { "@IsAvailable", false }
    }
);
```

## Database Schema

### Tables

**Reservations**
- `Id` - Primary key
- `CustomerName` - Customer full name
- `CustomerEmail` - Customer email address
- `CustomerPhone` - Customer phone number
- `ReservationDate` - Date of reservation
- `ReservationTime` - Time of reservation
- `NumberOfGuests` - Number of guests
- `SpecialRequests` - Any special requests (optional)
- `Status` - Reservation status (Pending, Confirmed, Cancelled, Completed)
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp

**Tables**
- `Id` - Primary key
- `TableNumber` - Unique table identifier
- `Capacity` - Number of guests the table can accommodate
- `IsAvailable` - Whether the table is available for reservations

**ReservationTables** (Junction table for multi-select)
- `Id` - Primary key
- `ReservationId` - Foreign key to Reservations
- `TableId` - Foreign key to Tables

## Features in Detail

### Reservation Limits
- Set a maximum number of reservations per day
- Automatically enforced when creating new reservations
- Throws `InvalidOperationException` if limit is reached

```csharp
var reservationService = new ReservationService(db, maxReservationsPerDay: 50);
```

### Discount System
- Automatically tracks completed reservations per customer email
- Customers with 3+ completed reservations receive 20% discount
- Use `GetCustomerStatistics()` to check eligibility

### Multi-Select Tables
- Assign multiple tables to a single reservation using table IDs
- Prevents double-booking of tables for the same time slot
- 2-hour buffer prevents overlapping reservations

## API Reference

### DB Class Methods

#### `Query(string sql, Dictionary<string, object>? parameters = null)`
Executes a SQL query and returns results as a list of dictionaries.

#### `Execute(string sql, Dictionary<string, object>? parameters = null)`
Executes a SQL command and returns the number of affected rows.

#### `ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)`
Executes a SQL command and returns a scalar value (e.g., newly inserted ID).

### ReservationService Methods

- `CreateReservation(Reservation reservation)` - Create a new reservation
- `GetReservation(int id)` - Get a reservation by ID
- `GetAllReservations()` - Get all reservations
- `UpdateReservation(Reservation reservation)` - Update an existing reservation
- `DeleteReservation(int id)` - Delete a reservation
- `GetCustomerStatistics(string email)` - Get customer statistics and discount eligibility

### TableService Methods

- `GetAllTables()` - Get all tables
- `GetAvailableTables(DateTime date, TimeSpan time)` - Get available tables for a specific date/time
- `GetTable(int id)` - Get a table by ID
- `CreateTable(Table table)` - Create a new table
- `UpdateTable(Table table)` - Update a table
- `DeleteTable(int id)` - Delete a table

## Error Handling

The application includes built-in validation:
- Reservation limit enforcement
- Table availability checking
- Parameterized queries to prevent SQL injection
- Proper exception handling with meaningful error messages

## License

This project is provided as-is for educational and commercial purposes.

## Contributing

Feel free to submit issues and enhancement requests!
