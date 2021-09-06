
-- =======================================================
-- Create Stored Procedure Template for Azure SQL Database
-- =======================================================

-- =============================================
-- Author:      <Author, Karunakaran>
-- Create Date: <Create Date, JUN-28-2019>
-- Description: <Description, Insert sp_MMSLog_2019_Q3_Insert>
-- =============================================

CREATE PROCEDURE [dbo].[sp_MMSLog_2019_Q3_Insert]

@SendId					nvarchar(100),
@MobileNumber			nvarchar(40),
@DynamicParamsValue		nvarchar(1000),
@MMSTemplateId			nvarchar(40),
@AccountId				BIGINT,
@SentStatus				TINYINT,
@SendDate				datetime,
@DeliveryStatus			TINYINT,
@DropErrorCode			nvarchar(200),
@ErrorMessage			nvarchar(1000),
@InteractionId			nvarchar(100),
@JourneyId				nvarchar(100),
@ActivityId				nvarchar(100),
@ActivityInteractionId	nvarchar(100)
AS
BEGIN

SET XACT_ABORT ON
BEGIN TRAN

 INSERT INTO [dbo].[MMSLog_2019_Q3]
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
                    ,@MobileNumber
                    ,@DynamicParamsValue
                    ,@MMSTemplateId
                    ,@AccountId
                    ,@SentStatus
                    ,@SendDate
                    ,@DeliveryStatus
                    ,@DropErrorCode
                    ,@ErrorMessage
                    ,@InteractionId
                    ,@JourneyId
                    ,@ActivityId
                    ,@ActivityInteractionId)

COMMIT TRAN
END


