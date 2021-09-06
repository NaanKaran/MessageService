CREATE TABLE [dbo].[MMSDeliveryReportNotification]
(
	[Id] NVARCHAR(50) NOT NULL PRIMARY KEY DEFAULT newid(), 
    [AccountId] BIGINT NULL, 
    [RunBy] SMALLINT NULL, 
    [RunDay] SMALLINT NULL, 
    [RunOnTime] SMALLINT NULL, 
    [CreatedOn] DATETIME NULL, 
    [Percentage] INT NULL
)
