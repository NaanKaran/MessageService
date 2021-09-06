-- =======================================================
-- Create Stored Procedure Template for Azure SQL Database
-- =======================================================

-- =============================================
-- Author:      <Author, karunakaran>
-- Create Date: <Create Date, JUL-03-2019>
-- Description: <Description, Insert or Update into [mmstemplate]>
-- =============================================

CREATE PROCEDURE [dbo].[sp_MMSTemplate_Upsert] 

@Id				nvarchar(100),
@AccountId		bigint,
@TemplateName	nvarchar(400),
@Signature		nvarchar(100),
@Title			nvarchar(MAX),
@Content		nvarchar(MAX),
@Status			tinyint,
@Variables		nvarchar(1000),
@IsDeleted		bit,
@CreatedBy		nvarchar(100),
@CreatedOn		datetime2

AS
BEGIN

SET XACT_ABORT ON
--SET NOCOUNT ON
BEGIN TRAN

  IF NOT EXISTS(Select 1 from [dbo].[MMSTemplate] WHERE Id =@Id )
                        Begin
                        INSERT INTO [dbo].[MMSTemplate]
                                   ([Id]
                                   ,[AccountId]
                                   ,[TemplateName]
                                   ,[Signature]
                                   ,[Title]
                                   ,[Content]
                                   ,[Status]
                                   ,[Variables]
                                   ,[IsDeleted]
                                   ,[CreatedBy]
                                   ,[CreatedOn]
                                   ,[UpdatedBy]
                                   ,[UpdatedOn])
                             VALUES
                                   (@Id
                                   ,@AccountId
                                   ,@TemplateName
                                   ,@Signature
                                   ,@Title
                                   ,@Content
                                   ,@Status
                                   ,@Variables
                                   ,@IsDeleted
                                   ,@CreatedBy
                                   ,@CreatedOn
                                   ,null
                                   ,null)
                        End
                        ELSE
                        Begin
                        UPDATE [dbo].[MMSTemplate]
                           SET [TemplateName] =		@TemplateName
                              ,[Signature] =		@Signature
                              ,[Title] =			@Title
                              ,[Content] =			@Content
                              ,[Status] =			@Status
                              ,[Variables] =		@Variables
                              ,[UpdatedOn] =		@CreatedOn
                         WHERE Id = @id AND AccountId = @AccountId
                        End 

COMMIT TRAN
END