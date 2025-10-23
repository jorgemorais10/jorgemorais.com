-- Create schema for approval hub tables
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Create schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'aprov')
BEGIN
    EXEC('CREATE SCHEMA aprov');
END
GO

PRINT 'Schema aprov created or already exists';
GO
