IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CardDisputeDB')
BEGIN
    CREATE DATABASE CardDisputeDB;
END