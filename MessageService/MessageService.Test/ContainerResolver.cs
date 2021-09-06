using System;
using System.Net.Http;
using Autofac;
using MessageService.CosmosRepository;
using MessageService.CosmosRepository.Utility;
using MessageService.RedisRepository;
using MessageService.Repository;
using MessageService.Service;
using MessageService.Service.Implementation;
using MessageService.Service.Interface;

namespace MessageService.Test
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

                containerBuilder.RegisterType<SubMailApiClientService>().As<ISubMailApiClientService>().WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient());
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
