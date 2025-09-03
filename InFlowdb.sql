USE master
create database Inflow
GO
use Inflow
GO
Create table Account(
	UserId INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(50) UNIQUE NOT NULL,
	Phone VARCHAR(10) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
	ResetCode VARCHAR(6),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);