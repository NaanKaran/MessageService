
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.InfraStructure.Helpers
{
    public static class EventGridHelper
    {
        private static readonly EventGridClient Client;
        private static readonly string TopicKey = AppSettings.GetValue("SMSTopicKey") ;
        private static readonly string TopicEndpoint = AppSettings.GetValue("SMSTopicEndpoint");
        private static readonly string TopicHostname = new Uri(TopicEndpoint).Host;
        static EventGridHelper()
        {
            TopicCredentials credentials = new TopicCredentials(TopicKey);
            Client = new EventGridClient(credentials);
        }
        public static async Task PublishSMSEventsAsync(IList<EventGridEvent> events)
        {
            await Client.PublishEventsAsync(TopicHostname, events);
        }
    }

}
