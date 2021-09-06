using System;
using Microsoft.Extensions.Configuration;

namespace MessageService.InfraStructure.Helpers
{
    public static class AppSettings
    {
        public static readonly IConfiguration Config;

        static AppSettings()
        {
            Config = new ConfigurationBuilder().SetBasePath(System.AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        }       

        public static string GetValue(string key)
        {            
            return Config[key] ?? Environment.GetEnvironmentVariable(key);
        }
    }
}
