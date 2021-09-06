using System;
using MessageService.InfraStructure.Helpers;
using StackExchange.Redis;

namespace MessageService.RedisRepository.Utility
{
    public class RedisConnector
    {
        static RedisConnector()
        {
            LazyRedisConnection = new Lazy
               <ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(AppSettings.GetValue("ConnectionStrings:RedisConnection")));
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyRedisConnection;

        public static ConnectionMultiplexer RedisConnection => LazyRedisConnection.Value;
    }
}
