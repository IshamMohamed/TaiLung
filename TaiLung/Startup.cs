using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            services.AddBot<MainBot>(options =>
            {
                var secretKey = Configuration.GetSection("botFileSecret")?.Value;
                var botFilePath = Configuration.GetSection("botFilePath")?.Value;
                // Read BotConfiguration.bot and register it in IServiceCollection 
                var botConfig = BotConfiguration.Load(botFilePath ?? @". \ BotConfiguration.bot", secretKey);
                services.AddSingleton(_ => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

                // Optional. Logging and error handling
                var logger = LoggerFactory.CreateLogger<MainBot>();
                options.OnTurnError = async (context, ex) =>
                {
                    logger.LogError($"Exception caught: {ex}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                // Obtain information on the endpoint of the current environment from BotConfiguration.bot. If not, it is an error. 
                var environment = IsProduction ? "production" : "development";
                var endpointService = botConfig
                    .Services
                    .FirstOrDefault(x => x.Type == "endpoint" && x.Name == environment) as EndpointService ??
                         throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");

                 // Add Bot's authentication information 
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // For development purposes only!In - memory store.The actual number seems to be Blob or something 
                var storage = new MemoryStorage();
                options.State.Add(new ConversationState(storage));
                options.State.Add(new UserState(storage));
            });

            // Add MainBotAccessor for state management
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value ??
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the state accessors");
                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault() ??
                    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                return new MainBotAccessors(conversationState);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory; // loggerFactory is not essential but logging is important

            // Activate Bot Framework
            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseBotFramework();
        }
    }
}
