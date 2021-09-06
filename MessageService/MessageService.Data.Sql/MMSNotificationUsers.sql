CREATE TABLE [dbo].[MMSNotificationUsers]
(
	[Id] NVARCHAR(256) NOT NULL PRIMARY KEY, 
    [SettingId] NVARCHAR(50) NULL references [MMSBalanceThreshold]([SettingId]), 
    [NotifyUserId] NVARCHAR(50) NULL, 
    [NotificationType] TINYINT NULL, 
    [DeliveryReportId] NVARCHAR(50) NULL references [MMSDeliveryReportNotification]([Id])
)
