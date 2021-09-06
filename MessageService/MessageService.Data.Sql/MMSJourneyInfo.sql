CREATE TABLE [dbo].[MMSJourneyInfo]
(
	[JourneyId]     NVARCHAR (100)  NOT NULL,
    [JourneyKey]    NVARCHAR (100)  NULL,
    [JourneyName]   NVARCHAR (500)  NULL,
    [InitiatedDate] DATETIME2 (7)   NULL,
	[AccountId]		BIGINT  NULL DEFAULT 0,
	[TotalCount]	INT  NULL DEFAULT 0,
	[DeliveredCount] INT  NULL DEFAULT 0,
	[DroppedCount]  INT  NULL DEFAULT 0,
	[SendFailedCount]	INT  NULL DEFAULT 0,
	[LastTriggeredOn] datetime2 Null, 
    PRIMARY KEY CLUSTERED ([JourneyId] ASC)
)
GO
CREATE NONCLUSTERED INDEX [IX_MMSJourneyInfo_CreatedOn] ON [dbo].[MMSJourneyInfo] ([LastTriggeredOn] desc)
GO
CREATE NONCLUSTERED INDEX [IX_MMSJourneyInfo_AccountId] ON [dbo].[MMSJourneyInfo] (AccountId desc)
