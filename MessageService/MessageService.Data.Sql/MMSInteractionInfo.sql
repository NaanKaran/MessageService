CREATE TABLE [dbo].[MMSInteractionInfo]
(
	[InteractionId] NVARCHAR (100) NOT NULL,
    [JourneyId]     NVARCHAR (100)  NOT NULL,
    [Version]       NVARCHAR (100)  NULL,	
    [PublishedDate] DATETIME2 (7)   NULL,
	[AccountId]		BIGINT  NULL DEFAULT 0,
	[TotalCount]	INT  NULL DEFAULT 0,
	[DeliveredCount] INT  NULL DEFAULT 0,
	[DroppedCount]  INT  NULL DEFAULT 0,
	[SendFailedCount]	INT  NULL DEFAULT 0,
	[QuadrantInfo] NVARCHAR (100)  NULL,
	[CreatedOn] datetime2 Null, 
    PRIMARY KEY CLUSTERED ([InteractionId] ASC),
	CONSTRAINT [FK_MMSIntractionInfo_MMSJourneyInfo] FOREIGN KEY ([JourneyId]) REFERENCES [dbo].[MMSJourneyInfo] ([JourneyId])
)

