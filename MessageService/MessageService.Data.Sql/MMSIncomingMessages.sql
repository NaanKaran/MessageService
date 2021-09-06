CREATE TABLE [dbo].[MMSIncomingMessages]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
	[AccountId] BIGINT NOT NULL,
    [MobileNumber] NVARCHAR(100) NULL, 
	[IsOptOut] bit NOT NULL, 
    [Content] NVARCHAR(MAX) NULL, 
	[JourneyName] NVARCHAR(250) NULL, 
    [CreatedOn] DATETIME NOT NULL,
	[IsUpdatedIntoDE]	     BIT NOT NULL DEFAULT 0, 
)
GO
CREATE NONCLUSTERED INDEX [IX_MMSIncomingMessages_MobileNumber] ON [dbo].[MMSIncomingMessages] (MobileNumber desc)
GO
CREATE NONCLUSTERED INDEX [IX_MMSIncomingMessages_CreatedOn] ON [dbo].[MMSIncomingMessages] (CreatedOn desc)
