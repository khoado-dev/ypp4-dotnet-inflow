USE master
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'Inflow')
BEGIN
    ALTER DATABASE Inflow SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Inflow;
END;
GO

create database Inflow
GO

use Inflow
GO

IF OBJECT_ID('dbo.Account', 'U') IS NOT NULL DROP TABLE dbo.Account;


Create table Account(
	UserId INT PRIMARY KEY IDENTITY(1,1),
  FirstName NVARCHAR(50) NOT NULL,
  Email NVARCHAR(50) UNIQUE NOT NULL,
	Phone VARCHAR(15) UNIQUE NOT NULL,
  PasswordHash NVARCHAR(255) NOT NULL,
	ResetCode VARCHAR(6),
  CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);