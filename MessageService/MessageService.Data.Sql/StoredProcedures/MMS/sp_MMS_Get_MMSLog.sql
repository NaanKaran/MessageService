-- =======================================================
-- Create Stored Procedure Template for Azure @query Database
-- =======================================================

 

-- =============================================
-- Author:      <Author, Karunakaran>
-- Create Date: <Create Date, JUN-28-2019>
-- Description: <Description, GetMMSLog>
-- =============================================

 

CREATE PROCEDURE [dbo].[sp_MMS_Get_MMSLog]

 

@JourneyId nvarchar(50),
@QuadrantTableName nvarchar(50),
@InteractionId nvarchar(50) = null,
@ActivityId nvarchar(50) = null,
@MobileNumber nvarchar(50) = null,
@SendStatus tinyint = null,
@DeliveryStatus tinyint = null,
@SendDateFrom DateTime = null,
@AccountId BigInt,
@SendDateTo DateTime = null,
@DeliveryDateFrom DateTime = null,
@DeliveryDateTo DateTime = null,
@PageNo Int,
@ItemsPerPage Int

 

AS
BEGIN
SET ANSI_NULLS OFF
SET XACT_ABORT ON
SET NOCOUNT ON

 


Declare @query nvarchar(Max) = ' SELECT 
                                I.Version,
                                A.ActivityName,
                                L.SendId,
                                L.MobileNumber,
                                L.MMSTemplateId,
                                L.DynamicParamsValue,
                                L.SentStatus,
                                L.DeliveryStatus,
                                L.SendDate,
                                L.DeliveryDate,
                                L.ErrorMessage,
                                L.DropErrorCode,
                                TotalCount = COUNT(1) OVER()
                            FROM 
                                [dbo].['+@QuadrantTableName +'] L
                            JOIN
                                [dbo].[MMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId = '+ Convert(nvarchar(10),@AccountId)+')
                            JOIN
                                [dbo].[MMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId = '+ Convert(nvarchar(10),@AccountId) + ')
                            WHERE
                            L.AccountId= '+ Convert(nvarchar(10),@AccountId) + '
                            AND
                            L.JourneyId = '''+ @JourneyId +'''  ';

 

IF (@DeliveryStatus  is not null)
BEGIN
SET @query = @query + ' AND L.DeliveryStatus = '+ Convert(nvarchar(10), @DeliveryStatus) +'';
END
IF (@SendStatus is not null)
BEGIN
SET  @query = @query + ' AND L.SentStatus ='+ Convert(nvarchar(10),@SendStatus)  +'';
END
IF (@MobileNumber is not null and @MobileNumber != '')
BEGIN
SET  @query = @query + ' AND L.MobileNumber ='''+ @MobileNumber +'''';
END
IF (@InteractionId is not null and @InteractionId != '')
BEGIN
SET  @query = @query + ' AND L.InteractionId ='''+@InteractionId+'''';
END
IF (@ActivityId is not null and @ActivityId != '')
BEGIN
SET  @query = @query + ' AND L.ActivityId ='''+ @ActivityId +'''';
END
IF (@SendDateFrom is not null and @SendDateTo is not null)
BEGIN
SET  @query = @query + ' AND L.SendDate Between '''+  CAST(@SendDateFrom AS nvarchar(100))  +''' and '''+  CAST(@SendDateTo AS nvarchar(100))  +'''';
END
IF (@DeliveryDateFrom  is not null and @DeliveryDateTo is not null)
BEGIN
SET   @query = @query + ' AND L.DeliveryDate Between '''+ CAST(@DeliveryDateFrom AS nvarchar(100)) +''' And '''+ CAST(@DeliveryDateTo AS nvarchar(100))  +'''';
END

 

SET @query = @query + ' ORDER BY L.SendDate DESC
                OFFSET (( '+ Convert (nvarchar(10),@PageNo) +'- 1) * '+ Convert (nvarchar(10),@ItemsPerPage)  +') ROWS
                FETCH NEXT '+ Convert (nvarchar(10),@ItemsPerPage) +' ROWS ONLY ';
EXEC (@query);

 

END