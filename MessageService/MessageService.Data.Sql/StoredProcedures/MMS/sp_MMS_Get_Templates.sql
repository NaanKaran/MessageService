-- =======================================================
-- Create Stored Procedure Template for Azure SQL Database
-- =======================================================

-- =============================================
-- Author:      <Author, Karunakaran>
-- Create Date: <Create Date, JUN-28-2019>
-- Description: <Description, Get MMS Templates>
-- =============================================

CREATE PROCEDURE [dbo].[sp_MMS_Get_Templates]

@AccountId bigint,
@TemplateName nvarchar(250),
@Status StatusUserDefineType ReadOnly,
@PageNo int,
@ItemsPerPage int


AS
BEGIN

SET XACT_ABORT ON
SET NOCOUNT ON

Declare @statusCount int = (Select count(Id) from @Status);


IF(@statusCount != 0 AND (@TemplateName IS NULL OR @TemplateName = ''))
Begin
SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSTemplate] WHERE AccountId = @AccountId AND IsDeleted = 0 AND [Status] in (Select Id from @Status)
	  ORDER BY CreatedOn DESC
			OFFSET ((@PageNo - 1) * @ItemsPerPage) ROWS
			FETCH NEXT @ItemsPerPage ROWS ONLY
END
Else IF(@statusCount !=0 AND (@TemplateName IS NOT NULL OR @TemplateName != ''))
Begin
  SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSTemplate] WHERE AccountId = @AccountId AND IsDeleted = 0 AND [Status] in (Select Id from @Status) 
  AND [TemplateName] LIKE CONCAT('%',@TemplateName,'%')
  ORDER BY CreatedOn DESC
        OFFSET ((@PageNo - 1) * @ItemsPerPage) ROWS
        FETCH NEXT @ItemsPerPage ROWS ONLY
End
ELSE IF ((@TemplateName IS NOT NULL OR @TemplateName != '') AND @statusCount =0)
BEGIN
  SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSTemplate] WHERE AccountId = @AccountId AND IsDeleted = 0 
  AND [TemplateName] LIKE CONCAT('%',@TemplateName,'%')
  ORDER BY CreatedOn DESC
        OFFSET ((@PageNo - 1) * @ItemsPerPage) ROWS
        FETCH NEXT @ItemsPerPage ROWS ONLY
END
ELSE 
BEGIN
  SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSTemplate] WHERE AccountId = @AccountId AND IsDeleted = 0 
															  ORDER BY CreatedOn DESC
																	OFFSET ((@PageNo - 1) * @ItemsPerPage) ROWS
																	FETCH NEXT @ItemsPerPage ROWS ONLY
END

END