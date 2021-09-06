CREATE TABLE [dbo].[MMSActivityInfo]
(
	[ActivityId]   NVARCHAR (100)  NOT NULL,
    [JourneyId]    NVARCHAR (100)  NOT NULL,
    [InteractionId] NVARCHAR (100)  NOT NULL,
    [ActivityName] NVARCHAR (512)  NULL,
	[AccountId]		BIGINT  NULL DEFAULT 0,
	[TotalCount]	INT  NULL DEFAULT 0,
	[DeliveredCount] INT  NULL DEFAULT 0,
	[DroppedCount]  INT  NULL DEFAULT 0,
	[SendFailedCount]	INT  NULL DEFAULT 0,
	[QuadrantInfo] NVARCHAR (100)  NULL,
	[CreatedOn] datetime2 Null, 
    PRIMARY KEY CLUSTERED ([ActivityId] ASC),
    CONSTRAINT [FK_MMSActivityInfo_MMSInteractionInfo] FOREIGN KEY ([InteractionId]) REFERENCES [dbo].[MMSInteractionInfo] ([InteractionId]),
    CONSTRAINT [FK_MMSActivityInfo_MMSJourneyInfo] FOREIGN KEY ([JourneyId]) REFERENCES [dbo].[MMSJourneyInfo] ([JourneyId])
)
