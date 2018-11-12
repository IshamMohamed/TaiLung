using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaiLung
{
    public class Startup
    {
        private bool IsProduction { get; }
        private IConfiguration Configuration { get; }
        private ILoggerFactory LoggerFactory { get; set; }

        public Startup(IHostingEnvironment env)
        {
            IsProduction = env.IsProduction();
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // while ago appsettings.json that you create are reading here 
                .AddJsonFile($"Appsettings {env.EnvironmentName} .Json.", optional: true) // Load appsettings.production.json, if any , 
                .AddEnvironmentVariables() // Load contents in application settings when deploying to Azure App Services
                .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
