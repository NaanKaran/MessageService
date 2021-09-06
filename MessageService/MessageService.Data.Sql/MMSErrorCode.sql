CREATE TABLE [dbo].[MMSErrorCode]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[EnglishDescription] NVARCHAR(MAX),
	[ChineseDescription] NVARCHAR(MAX),
	[ErrorCode] NVARCHAR(100),
	[VendorId] INT,
	[CreatedOn] datetime2 DEFAULT GETUTCDATE()
)
go
CREATE NONCLUSTERED INDEX IX_MMSErrorCode_ErrorCode
ON MMSErrorCode (ErrorCode ASC)