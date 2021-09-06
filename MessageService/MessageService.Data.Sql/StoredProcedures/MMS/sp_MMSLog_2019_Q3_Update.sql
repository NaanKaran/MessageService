
-- =======================================================
-- Create Stored Procedure Template for Azure SQL Database
-- =======================================================

-- =============================================
-- Author:      <Author, Karunakaran>
-- Create Date: <Create Date, JUN-28-2019>
-- Description: <Description,  Update MMSLog_2019_Q3>
-- =============================================

CREATE PROCEDURE [dbo].[sp_MMSLog_2019_Q3_Update]

@SendId					nvarchar(100),
@DeliveryDate			datetime2(7),
@DeliveryStatus			TINYINT,
@DropErrorCode			nvarchar(200),
@ErrorMessage			nvarchar(1000)
AS
BEGIN

SET XACT_ABORT ON
--SET NOCOUNT ON
BEGIN TRAN

 UPDATE [dbo].[MMSLog_2019_Q3]
                SET [DeliveryStatus] = @DeliveryStatus
                    ,[DeliveryDate] = @DeliveryDate
                    ,[DropErrorCode] = @DropErrorCode
	                ,[ErrorMessage]  = @ErrorMessage
	                ,[IsUpdatedIntoDE]  = 0
                WHERE SendId = @SendId

COMMIT TRAN
END


