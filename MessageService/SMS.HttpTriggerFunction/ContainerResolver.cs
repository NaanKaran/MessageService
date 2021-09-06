using System;
using Autofac;
using MessageService.CosmosRepository;
using MessageService.CosmosRepository.Utility;
using MessageService.RedisRepository;
using MessageService.Repository;
using MessageService.Service;

namespace SMS.HttpTriggerFunction
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
                containerBuilder.RegisterModule<CosmosRepositoryModule>();

                Container = containerBuilder.Build();
                //Initialize the Cosmos connection and Metadata of Scale up
                CosmosBaseRepository.InitializeAsync().Wait();
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
