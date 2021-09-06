CREATE TABLE [dbo].[MMSLog_2019_Q2]
([SendId]                NVARCHAR(50) NOT NULL PRIMARY KEY, 
 [MobileNumber]          NVARCHAR(20) NOT NULL, 
 [MMSTemplateId]         NVARCHAR(20) NOT NULL, 
 [DynamicParamsValue]    NVARCHAR(500) NULL, 
 [AccountId]             BIGINT NOT NULL, 
 [SentStatus]            TINYINT NULL, 
 [SendDate]              DATETIME2 NOT NULL, 
 [DeliveryStatus]        TINYINT, 
 [DeliveryDate]          DATETIME2 NULL, 
 [DropErrorCode]         NVARCHAR(20)  NULL, 
 [ErrorMessage]          NVARCHAR(500)  NULL, 
 [InteractionId]         NVARCHAR(100) NOT NULL, 
 [JourneyId]             NVARCHAR(100) NOT NULL, 
 [ActivityId]            NVARCHAR(100) NOT NULL, 
 [ActivityInteractionId] NVARCHAR(100) NOT NULL,
 [IsUpdatedIntoDE]	     BIT NOT NULL DEFAULT 0, 
 CONSTRAINT [FK_MMSLog_2019_Q2_MMSActivityInfo] FOREIGN KEY([ActivityId]) REFERENCES [dbo].[MMSActivityInfo]([ActivityId]), 
 CONSTRAINT [FK_MMSLog_2019_Q2_MMSInteractionInfo] FOREIGN KEY([InteractionId]) REFERENCES [dbo].[MMSInteractionInfo]([InteractionId]), 
 CONSTRAINT [FK_MMSLog_2019_Q2_MMSJourneyInfo] FOREIGN KEY([JourneyId]) REFERENCES [dbo].[MMSJourneyInfo]([JourneyId])
)

GO
CREATE NONCLUSTERED INDEX [IX_MMSLog_2019_Q2_SendDate] ON [dbo].[MMSLog_2019_Q2] (SendDate desc)
GO
CREATE NONCLUSTERED INDEX [IX_MMSLog_2019_Q2_DeliveryDate] ON [dbo].[MMSLog_2019_Q2] (DeliveryDate desc)
GO
CREATE NONCLUSTERED INDEX [IX_MMSLog_2019_Q2_AccountId] ON [dbo].[MMSLog_2019_Q2] (AccountId desc)
GO
CREATE NONCLUSTERED INDEX [IX_MMSLog_2019_Q2_DeliveryStatus] ON [dbo].[MMSLog_2019_Q2] (DeliveryStatus desc)

