using Autofac;
using Autofac.Extensions.DependencyInjection;
using MessageService.CosmosRepository;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.RedisRepository;
using MessageService.Repository;
using MessageService.Service;
using MessageService.Service.Implementation;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Net.Http;
using MessageService.CosmosRepository.Utility;
using MessageService.Models.CosmosModel.ScaleModels;

namespace MessageService
{
    public class Startup
    {
        private readonly string CorsPolicy = "MessageServiceAllowedOrigins";
        private readonly string[] AllowedOrigins = AppSettings.GetValue("AllowedOrigins").Split(',');
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors
            (
                options =>
                {
                    options.AddPolicy
                    (
                        CorsPolicy,
                        corsPolicyBuilder =>
                        {
                            corsPolicyBuilder
                                 .SetIsOriginAllowedToAllowWildcardSubdomains()
                                 .WithOrigins(AllowedOrigins)
                                 .AllowAnyMethod()
                                 .AllowCredentials()
                                 .AllowAnyHeader()
                                 .Build();
                        }
                    );
                }
            );

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpClient();

            //services.AddTransient<ISubMailApiClientService, SubMailApiClientService>();
            services.AddHttpClient("SubMailClient", client =>
            {
                client.BaseAddress = new Uri(SubmailAPIUrls.SubMailBaseDomainUrl);
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>

            {
                c.SwaggerDoc("v1", new Info { Title = "MMS APIs", Version = "v1" });
            });

            //Now register our services with Autofac container

            var builder = new ContainerBuilder();

            // exclicit resolving client for constructor
            builder.RegisterType<SubMailApiClientService>().As<ISubMailApiClientService>().WithParameter(
                (p, ctx) => p.ParameterType == typeof(HttpClient),
                (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient());


            builder.RegisterModule<RedisRepositoryModule>();

            builder.RegisterModule<CosmosRepositoryModule>();

            builder.RegisterModule<RepositoryModule>();

            builder.RegisterModule<ServiceModule>();

            builder.Populate(services);

            var container = builder.Build();

            //Initialize the Cosmos connection and Metadata of Scale up
            CosmosBaseRepository.InitializeCosmosCollectionAsync().Wait();
            CosmosBaseRepository.InitializeAsync().Wait();
            //SaveAsync the IServiceProvider based on the container
            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(CorsPolicy);
            //LogManager.Configuration.Variables["AzureTableStorageConnectionString"] = Configuration.GetValue<string>("AzureTableStorageConnectionString");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>

            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MMS V1");
                //c.RoutePrefix = string.Empty;

            });

            app.UseMvc();
        }
    }
}
