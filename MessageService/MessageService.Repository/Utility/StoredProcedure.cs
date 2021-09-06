
using MessageService.InfraStructure.Helpers;
using System;

namespace MessageService.Repository.Utility
{
    public static class StoredProcedure
    {
        public const string SMSJourneyUpsert = "sp_SMS_Journey_Upsert";

        public const string MMSJourneyUpsert = "sp_MMS_Journey_Upsert";

        public const string MMSTemplateUpsert = "sp_MMSTemplate_Upsert";

        public const string MMSGetTemplates = "sp_MMS_Get_Templates";

        public const string GetMMSLog = "sp_MMS_Get_MMSLog";

        public const string SMSIncomingMessageInsert = "sp_SMS_IncomingMessage_Insert";        
       
    }   
}
