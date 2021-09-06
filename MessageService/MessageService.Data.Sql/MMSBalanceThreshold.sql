CREATE TABLE [dbo].[MMSBalanceThreshold]
(
	[SettingId] NVARCHAR(50) NOT NULL PRIMARY KEY DEFAULT NEWID(), 
    [AccountId] BIGINT NULL, 
    [ThresholdCount] BIGINT NULL, 
    [CreatedBy] NVARCHAR(256) NULL, 
    [CreatedIP] NVARCHAR(50) NULL, 
    [CreatedOn] DATETIME NULL, 
    [ModifiedBy] NVARCHAR(256) NULL, 
    [ModifiedIP] NVARCHAR(50) NULL, 
    [ModifiedOn] DATETIME NULL
)
