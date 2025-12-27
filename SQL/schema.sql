-- Create Tables table
CREATE TABLE Tables (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TableNumber NVARCHAR(50) NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Create Reservations table
CREATE TABLE Reservations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(255) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NOT NULL,
    ReservationDate DATE NOT NULL,
    ReservationTime TIME NOT NULL,
    NumberOfGuests INT NOT NULL,
    SpecialRequests NVARCHAR(MAX) NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Confirmed, 2=Cancelled, 3=Completed
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Create ReservationTables junction table for multi-select
CREATE TABLE ReservationTables (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ReservationId INT NOT NULL,
    TableId INT NOT NULL,
    FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE,
    FOREIGN KEY (TableId) REFERENCES Tables(Id) ON DELETE CASCADE,
    CONSTRAINT UK_ReservationTables UNIQUE (ReservationId, TableId)
);

-- Create indexes for better performance
CREATE INDEX IX_Reservations_Email ON Reservations(CustomerEmail);
CREATE INDEX IX_Reservations_Date ON Reservations(ReservationDate);
CREATE INDEX IX_Reservations_Status ON Reservations(Status);
CREATE INDEX IX_ReservationTables_ReservationId ON ReservationTables(ReservationId);
CREATE INDEX IX_ReservationTables_TableId ON ReservationTables(TableId);

-- Insert sample tables
INSERT INTO Tables (TableNumber, Capacity, IsAvailable) VALUES
('T1', 2, 1),
('T2', 2, 1),
('T3', 4, 1),
('T4', 4, 1),
('T5', 6, 1),
('T6', 6, 1),
('T7', 8, 1),
('T8', 8, 1);

-- Create view for reservation details
CREATE VIEW ReservationDetails AS
SELECT 
    r.Id,
    r.CustomerName,
    r.CustomerEmail,
    r.CustomerPhone,
    r.ReservationDate,
    r.ReservationTime,
    r.NumberOfGuests,
    r.SpecialRequests,
    r.Status,
    r.CreatedAt,
    r.UpdatedAt,
    STRING_AGG(t.TableNumber, ', ') as TableNumbers,
    SUM(t.Capacity) as TotalCapacity
FROM Reservations r
LEFT JOIN ReservationTables rt ON r.Id = rt.ReservationId
LEFT JOIN Tables t ON rt.TableId = t.Id
GROUP BY 
    r.Id, r.CustomerName, r.CustomerEmail, r.CustomerPhone,
    r.ReservationDate, r.ReservationTime, r.NumberOfGuests,
    r.SpecialRequests, r.Status, r.CreatedAt, r.UpdatedAt;
