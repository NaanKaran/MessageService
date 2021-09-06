CREATE TABLE [dbo].[MMSLogTableInfo]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY , 
    [TableName] NVARCHAR(50) NOT NULL UNIQUE, 
    [Year] INT NOT NULL, 
    [QuadrantNumber] TinyInt NOT NULL, 
    [Description] NVARCHAR(50) NOT NULL, 
    [CreatedOn] DATETIME2 NULL DEFAULT GETDATE()
)
