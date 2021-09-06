using System;
using Autofac;
using MessageService.Service.Implementation;

namespace MessageService.Service
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            try
            {

                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                // Register your service here
                builder.RegisterType<TemplateService>().AsImplementedInterfaces();
                builder.RegisterType<SettingsService>().AsImplementedInterfaces();
                builder.RegisterType<LibraryService>().AsImplementedInterfaces();
                builder.RegisterType<MMSService>().AsImplementedInterfaces();
                builder.RegisterType<SMSService>().AsImplementedInterfaces();
                builder.RegisterType<WeChatifyService>().AsImplementedInterfaces();
                builder.RegisterType<MandrillService>().AsImplementedInterfaces();
                builder.RegisterType<EmailService>().AsImplementedInterfaces();
                builder.RegisterType<SMSDataExtensionService>().AsImplementedInterfaces();
                builder.RegisterType<MMSDataExtensionService>().AsImplementedInterfaces();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
