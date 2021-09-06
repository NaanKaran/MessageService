using System;
using Autofac;
using MessageService.RedisRepository;
using MessageService.Repository;
using MessageService.Service;

namespace MMS.HttpTriggerFunction
{
    public class ContainerResolver : IDisposable
    {
        public IContainer Container
        {
            get; set;
        }

        public ContainerResolver()
        {
            try
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterModule<RepositoryModule>();
                containerBuilder.RegisterModule<ServiceModule>();
                containerBuilder.RegisterModule<RedisRepositoryModule>();

                Container = containerBuilder.Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
