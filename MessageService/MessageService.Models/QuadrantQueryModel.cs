using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models
{
    public class QuadrantQueryModel
    {
        public string CreateSMSLogTable => $@"CREATE TABLE [dbo].[@@TABLENAME]
                                                        ([SendId]                NVARCHAR(50) NOT NULL PRIMARY KEY, 
                                                         [MobileNumber]          NVARCHAR(20) NOT NULL, 
                                                         [AccountId]             BIGINT NOT NULL, 
                                                         [SentStatus]            TINYINT NULL, 
                                                         [SendDate]              DATETIME2 NOT NULL, 
                                                         [DeliveryStatus]        TINYINT, 
                                                         [DeliveryDate]          DATETIME2 NULL, 
                                                         [DropErrorCode]         NVARCHAR(200)  NULL, 
                                                         [ErrorMessage]          NVARCHAR(500)  NULL, 
                                                         [InteractionId]         NVARCHAR(100) NOT NULL, 
                                                         [JourneyId]             NVARCHAR(100) NOT NULL, 
                                                         [ActivityId]            NVARCHAR(100) NOT NULL, 
                                                         [ActivityInteractionId] NVARCHAR(100) NOT NULL,  
                                                         [ContactKey]			 NVARCHAR(100) NULL,
                                                         [JourneyKey]			 NVARCHAR(100) NULL,
                                                         [PersonalizationData]	 NVARCHAR(1000) NULL,
                                                         [IsUpdatedIntoDE]	     BIT NOT NULL DEFAULT 0, 
                                                         CONSTRAINT [FK_@@TABLENAME_SMSActivityInfo] FOREIGN KEY([ActivityId]) REFERENCES [dbo].[SMSActivityInfo]([ActivityId]), 
                                                         CONSTRAINT [FK_@@TABLENAME_SMSInteractionInfo] FOREIGN KEY([InteractionId]) REFERENCES [dbo].[SMSInteractionInfo]([InteractionId]), 
                                                         CONSTRAINT [FK_@@TABLENAME_SMSJourneyInfo] FOREIGN KEY([JourneyId]) REFERENCES [dbo].[SMSJourneyInfo]([JourneyId])
                                                        )

                                                        GO
                                                        CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_SendDate] ON [dbo].[@@TABLENAME] (SendDate desc)
                                                        GO
                                                        CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_DeliveryDate] ON [dbo].[@@TABLENAME] (DeliveryDate desc)
                                                        GO
                                                        CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_AccountId] ON [dbo].[@@TABLENAME] (AccountId desc)
                                                        GO
                                                        CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_DeliveryStatus] ON [dbo].[@@TABLENAME] (DeliveryStatus desc)
                                                        ";
        public string CreateSMSLogInsertProcedure => $@"CREATE PROCEDURE [dbo].[sp_@@TABLENAME_Insert] 
                                                                    @ActivityId nvarchar(50),
                                                                    @InteractionId nvarchar(50),
                                                                    @JourneyId nvarchar(50),
                                                                    @SendId nvarchar(50),
                                                                    @MobileNumber nvarchar(50),
                                                                    @AccountId bigint,
                                                                    @SentStatus tinyint,
                                                                    @SendDate datetime,
                                                                    @DeliveryStatus tinyint,
                                                                    @DropErrorCode nvarchar(200),
                                                                    @ErrorMessage nvarchar(250),
                                                                    @ActivityInteractionId nvarchar(50),
                                                                    @ContactKey nvarchar(100),
                                                                    @JourneyKey nvarchar(100),
                                                                    @Content nvarchar(MAX),
                                                                    @PersonalizationData nvarchar(MAX)
                                                                    AS
                                                                    BEGIN

                                                                    SET XACT_ABORT ON
                                                                    BEGIN TRAN
                                                                      INSERT INTO [dbo].[@@TABLENAME]
                                                                                                       ([SendId]
                                                                                                       ,[MobileNumber]                                 
                                                                                                       ,[AccountId]
                                                                                                       ,[SentStatus]
                                                                                                       ,[SendDate]
                                                                                                       ,[DeliveryStatus]
                                                                                                       ,[DropErrorCode]
                                                                                                       ,[ErrorMessage]
                                                                                                       ,[InteractionId]
                                                                                                       ,[JourneyId]
                                                                                                       ,[ActivityId]
                                                                                                       ,[ActivityInteractionId]
								                                                                       ,[ContactKey]
								                                                                       ,[JourneyKey]
								                                                                       )
                                                                                                 VALUES
                                                                                                       (@SendId
                                                                                                       ,@MobileNumber                                
                                                                                                       ,@AccountId
                                                                                                       ,@SentStatus
                                                                                                       ,@SendDate
                                                                                                       ,@DeliveryStatus
                                                                                                       ,@DropErrorCode
                                                                                                       ,@ErrorMessage
                                                                                                       ,@InteractionId
                                                                                                       ,@JourneyId
                                                                                                       ,@ActivityId
                                                                                                       ,@ActivityInteractionId
								                                                                       ,@ContactKey
								                                                                       ,@JourneyKey
								                                                                       )

	                                                                    INSERT INTO [dbo].[SMSContent]
                                                                                                       ([SendId]
								                                                                       ,[Content]
                                                                                                       ,[PersonalizationData]
								                                                                       )
                                                                                                 VALUES
                                                                                                       (@SendId
                                                                                                       ,@Content
								                                                                       ,@PersonalizationData
								                                                                       )
                                                                    COMMIT TRAN
                                                                    END";
        public string CreateSMSLogUpdateProcedure => $@"CREATE PROCEDURE [dbo].[sp_@@TABLENAME_Update] 
                                                            @SendId nvarchar(50),
                                                            @DeliveryDate datetime,
                                                            @DeliveryStatus tinyint,
                                                            @DropErrorCode nvarchar(200),
                                                            @ErrorMessage nvarchar(250)
                                                            AS
                                                            BEGIN

                                                            SET XACT_ABORT ON
                                                            BEGIN TRAN
                                                              UPDATE [dbo].[@@TABLENAME]
                                                                                           SET [DeliveryStatus] = @DeliveryStatus
                                                                                              ,[DeliveryDate] = @DeliveryDate
                                                                                              ,[DropErrorCode] = @DropErrorCode
	                                                                                          ,[ErrorMessage]  = @ErrorMessage
	                                                                                          ,[IsUpdatedIntoDE]  = 0
                                                                                         WHERE SendId =@SendId;
                                                            COMMIT TRAN
                                                            END";

        public string CreateMMSLogTableQuery => @"CREATE TABLE [dbo].[@@TABLENAME]
                                                ([SendId]                NVARCHAR(50) NOT NULL PRIMARY KEY, 
                                                 [MobileNumber]          NVARCHAR(20) NOT NULL, 
                                                 [MMSTemplateId]         NVARCHAR(20) NOT NULL, 
                                                 [DynamicParamsValue]    NVARCHAR(500) NULL, 
                                                 [AccountId]             BIGINT NOT NULL, 
                                                 [SentStatus]            TINYINT NULL, 
                                                 [SendDate]              DATETIME2 NOT NULL, 
                                                 [DeliveryStatus]        TINYINT NULL, 
                                                 [DeliveryDate]          DATETIME2 NULL, 
                                                 [DropErrorCode]         NVARCHAR(200)  NULL, 
                                                 [ErrorMessage]          NVARCHAR(1000)  NULL, 
                                                 [InteractionId]         NVARCHAR(100) NOT NULL, 
                                                 [JourneyId]             NVARCHAR(100) NOT NULL, 
                                                 [ActivityId]            NVARCHAR(100) NOT NULL, 
                                                 [ActivityInteractionId] NVARCHAR(100) NOT NULL, 
                                                  [IsUpdatedIntoDE]	     BIT NOT NULL DEFAULT 0, 
                                                 CONSTRAINT [FK_@@TABLENAME_MMSActivityInfo] FOREIGN KEY([ActivityId]) REFERENCES [dbo].[MMSActivityInfo]([ActivityId]), 
                                                 CONSTRAINT [FK_@@TABLENAME_MMSInteractionInfo] FOREIGN KEY([InteractionId]) REFERENCES [dbo].[MMSInteractionInfo]([InteractionId]), 
                                                 CONSTRAINT [FK_@@TABLENAME_MMSJourneyInfo] FOREIGN KEY([JourneyId]) REFERENCES [dbo].[MMSJourneyInfo]([JourneyId])
                                                )

                                                GO
                                                CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_SendDate] ON [dbo].[@@TABLENAME] (SendDate desc)
                                                GO
                                                CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_DeliveryDate] ON [dbo].[@@TABLENAME] (DeliveryDate desc)
                                                GO
                                                CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_AccountId] ON [dbo].[@@TABLENAME] (AccountId desc)
                                                GO
                                                CREATE NONCLUSTERED INDEX [IX_@@TABLENAME_DeliveryStatus] ON [dbo].[@@TABLENAME] (DeliveryStatus desc)";

        public string CreateMMSLogInsertProcQuery => @"CREATE PROCEDURE [dbo].[sp_@@TABLENAME_Insert]
                                                        @SendId nvarchar(100),
                                                        @MobileNumber nvarchar(40),
                                                        @DynamicParamsValue nvarchar(1000),
                                                        @MMSTemplateId nvarchar(40),
                                                        @AccountId bigint,
                                                        @SentStatus TINYINT,
                                                        @SendDate datetime,
                                                        @DeliveryStatus TINYINT,
                                                        @DropErrorCode nvarchar(200),
                                                        @ErrorMessage nvarchar(1000),
                                                        @InteractionId nvarchar(100),
                                                        @JourneyId nvarchar(100),
                                                        @ActivityId nvarchar(100),
                                                        @ActivityInteractionId nvarchar(100)
                                                        AS
                                                        BEGIN

                                                        SET XACT_ABORT ON
                                                        --SET NOCOUNT ON
                                                        BEGIN TRAN

                                                         INSERT INTO[dbo].[@@TABLENAME]
                                                                ([SendId]
                                                                           ,[MobileNumber]
                                                                           ,[DynamicParamsValue]
                                                                           ,[MMSTemplateId]
                                                                           ,[AccountId]
                                                                           ,[SentStatus]
                                                                           ,[SendDate]
                                                                           ,[DeliveryStatus]
                                                                           ,[DropErrorCode]
                                                                           ,[ErrorMessage]
                                                                           ,[InteractionId]
                                                                           ,[JourneyId]
                                                                           ,[ActivityId]
                                                                           ,[ActivityInteractionId])
                                                                       VALUES
                                                                           (@SendId

                                                                           , @MobileNumber

                                                                           , @DynamicParamsValue

                                                                           , @MMSTemplateId

                                                                           , @AccountId

                                                                           , @SentStatus

                                                                           , @SendDate

                                                                           , @DeliveryStatus

                                                                           , @DropErrorCode

                                                                           , @ErrorMessage

                                                                           , @InteractionId

                                                                           , @JourneyId

                                                                           , @ActivityId

                                                                           , @ActivityInteractionId)

                                                        COMMIT TRAN
                                                        END";

        public string CreateMMSLogUpdateProcQuery => @"
                                                    CREATE PROCEDURE [dbo].[sp_@@TABLENAME_Update]
                                                    @SendId					nvarchar(100),
                                                    @DeliveryDate			datetime,
                                                    @DeliveryStatus			TINYINT,
                                                    @DropErrorCode			nvarchar(200),
                                                    @ErrorMessage			nvarchar(1000)
                                                    AS
                                                    BEGIN

                                                    SET XACT_ABORT ON
                                                    BEGIN TRAN

                                                     UPDATE [dbo].[@@TABLENAME]
                                                                    SET [DeliveryStatus] = @DeliveryStatus
                                                                        ,[DeliveryDate] = @DeliveryDate
                                                                        ,[DropErrorCode] = @DropErrorCode
	                                                                    ,[ErrorMessage]  = @ErrorMessage
	                                                                    ,[IsUpdatedIntoDE]  = 0
                                                                    WHERE SendId = @SendId

                                                    COMMIT TRAN
                                                    END
                                                    ";
    }
}
