using System;
using Autofac;
using MessageService.RedisRepository.Implementation;
using MessageService.RedisRepository.Interface;

namespace MessageService.RedisRepository
{
    public class RedisRepositoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("Redis Repository Builder is Null");
                }

                builder.RegisterType<RedisCache>().As<IRedisCache>();
                builder.RegisterType<Implementation.RedisRepository>().As<IRedisRespository>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
