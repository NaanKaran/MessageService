using Autofac;
using MessageService.CosmosRepository.Implementation;
using MessageService.CosmosRepository.Interface;
using System;
using MessageService.CosmosRepository.MetaDataOperator;

namespace MessageService.CosmosRepository
{
    public class CosmosRepositoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("Cosmos Repository Builder is Null");
                }

                builder.RegisterType<SMSCosmosRepository>().As<ISMSCosmosRepository>();
                builder.RegisterType<SettingsCosmosRepository>().As<ISettingsCosmosRepository>();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
