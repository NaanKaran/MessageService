CREATE TABLE [dbo].[MMSVendorSettings]
(
	[AccountId] BIGINT NOT NULL PRIMARY KEY, 
    [VendorId] SMALLINT NULL, 
    [AppId] NVARCHAR(250) NULL, 
    [AppKey] NVARCHAR(250) NULL, 
    [CreatedOn] DATETIME NULL, 
    [UpdatedOn] DATETIME NULL
)
