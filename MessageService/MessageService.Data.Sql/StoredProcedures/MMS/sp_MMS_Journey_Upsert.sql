-- =======================================================
-- Create Stored Procedure Template for Azure SQL Database
-- =======================================================

-- =============================================
-- Author:      <Author, Aswin Perumal>
-- Create Date: <Create Date, JUN-28-2019>
-- Description: <Description, Insert or Update into [mmsjourneyinfo], [mmsinteractioninfo], [mmsactivityinfo]>
-- =============================================

CREATE PROCEDURE [dbo].[sp_MMS_Journey_Upsert] @ActivityId nvarchar(50),
@InteractionId nvarchar(50),
@Version nvarchar(50),
@JourneyId nvarchar(50),
@QuadrantInfo nvarchar(50),
@ActivityName nvarchar(250),
@AccountId bigint,
@JourneyKey nvarchar(50),
@JourneyName nvarchar(250),
@CreatedOn datetime
AS
BEGIN

SET XACT_ABORT ON
BEGIN TRAN

  IF NOT EXISTS (SELECT 1 FROM [dbo].[MMSJourneyInfo] WHERE JourneyId = @JourneyId)
  BEGIN
    INSERT INTO [dbo].[MMSJourneyInfo] 
	([JourneyId],
    [JourneyKey],
    [JourneyName],
    [InitiatedDate],
    [AccountId],
    [LastTriggeredOn])
      VALUES (@JourneyId, @JourneyKey, @JourneyName, @CreatedOn, @AccountId, @CreatedOn)
  END
  ELSE
  BEGIN
    UPDATE [dbo].[MMSJourneyInfo]
    SET [LastTriggeredOn] = @CreatedOn
    WHERE JourneyId = @JourneyId;
  END

  IF NOT EXISTS (SELECT 1 FROM [dbo].[MMSInteractionInfo] WHERE [InteractionId] = @InteractionId)
  BEGIN
    INSERT INTO [dbo].[MMSInteractionInfo] 
	([InteractionId],
    [JourneyId],
    [Version],
    [PublishedDate],
    [AccountId],
    [QuadrantInfo],
    [CreatedOn])
      VALUES (@InteractionId, @JourneyId, @Version, @CreatedOn, @AccountId, @QuadrantInfo, @CreatedOn)
  END

  INSERT INTO [dbo].[MMSActivityInfo] 
  ([ActivityId],
  [JourneyId],
  [InteractionId],
  [ActivityName],
  [AccountId],
  [QuadrantInfo],
  [CreatedOn])
    VALUES (@ActivityId, @JourneyId, @InteractionId, @ActivityName, @AccountId, @QuadrantInfo, @CreatedOn)

COMMIT TRAN
END