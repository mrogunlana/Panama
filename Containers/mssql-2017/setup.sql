CREATE DATABASE $(MSSQL_DB);
CREATE DATABASE $(MSSQL_DB_AUDIT_LOG);
GO
USE $(MSSQL_DB);
GO
CREATE LOGIN $(MSSQL_USER) WITH PASSWORD = '$(MSSQL_PASSWORD)';
GO
CREATE USER $(MSSQL_USER) FOR LOGIN $(MSSQL_USER);
GO
ALTER SERVER ROLE sysadmin ADD MEMBER [$(MSSQL_USER)];
GO

USE $(MSSQL_DB_AUDIT_LOG);
GO

CREATE TABLE Log (
	Id INT NOT NULL IDENTITY(1, 1) PRIMARY KEY, 
	CorrelationId UNIQUEIDENTIFIER NULL,
	MachineName varchar(200) NOT NULL,
	Logged datetime NOT NULL,
	Level varchar(50) NULL,
	Message text NOT NULL,
    Logger varchar(250) NULL,
	Callsite varchar(250) NULL,
	Exception text
);
GO
USE $(MSSQL_DB);
GO
CREATE TABLE [User] (
	_ID INT NOT NULL IDENTITY(1, 1) PRIMARY KEY, 
	ID uniqueidentifier not null,
    FirstName varchar(25) null,
    LastName varchar(25) null,
    Email varchar(100) null,
    Password varchar(100) null,
    Created datetime not null
);



Go
