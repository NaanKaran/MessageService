using System;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Repository.Implementation;

namespace MessageService.Repository
{
    public class RepositoryModule : Module
    {
        private readonly string _dbWeChatifyConnection = AppSettings.GetValue("ConnectionStrings:WechatifyConnection");
        private readonly string _dbMessageService = AppSettings.GetValue("ConnectionStrings:MessageServiceConnection");
        protected override void Load(ContainerBuilder builder)
        {
            try
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("Repository Builder is Null");
                }
           
                // Register here your Repository Service
                builder.RegisterType<TemplateRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection).WithParameter("messageService", _dbMessageService);
                builder.RegisterType<SettingsRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection).WithParameter("messageService", _dbMessageService);
                builder.RegisterType<AzureStorageRepository>().AsImplementedInterfaces();
                //builder.RegisterType<SMSRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection).WithParameter("messageService", _dbMessageService);
                builder.RegisterType<MMSRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection).WithParameter("messageService", _dbMessageService);
                builder.RegisterType<LibraryRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection).WithParameter("messageService", _dbMessageService);
                builder.RegisterType<WeChatifyRepository>().AsImplementedInterfaces().WithParameter("wechatify", _dbWeChatifyConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
