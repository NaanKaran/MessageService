CREATE TABLE [dbo].[MMSTemplate]
(
	[Id] NVARCHAR(50) NOT NULL PRIMARY KEY,
	[AccountId] BIGINT NOT NULL,
    [TemplateName] NVARCHAR(200) NOT NULL, 
    [Signature] NVARCHAR(50) NOT NULL, 
    [Title] NVARCHAR(MAX) NULL, 
    [Content] NVARCHAR(MAX) NULL,
	[Status] TINYINT NULL,
	[Variables] NVARCHAR(500) NULL,
	[IsDeleted] bit NULL,
	[Comments] NVARCHAR(MAX) NULL, 
    [CreatedBy] NVARCHAR(50) NULL, 
    [CreatedOn] DATETIME2 NOT NULL, 
    [UpdatedBy] NVARCHAR(50) NULL, 
    [UpdatedOn] DATETIME2 NULL 
    
)
GO
CREATE NONCLUSTERED INDEX IX_MMSTemplate_Status
ON MMSTemplate (Status ASC)
GO
CREATE NONCLUSTERED INDEX IX_MMSTemplate_CreatedOn
ON MMSTemplate (Status ASC)
