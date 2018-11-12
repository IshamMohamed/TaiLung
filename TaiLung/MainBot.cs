﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace TaiLung
{
    public class MainBot : IBot
    {
        private ILogger Logger { get; }
        private MainBotAccessors MainBotAccessors { get; }

        private DialogSet Dialogs { get; }

        public MainBot(MainBotAccessors mainBotAccessors, ILoggerFactory loggerFactory)
        {
            MainBotAccessors = mainBotAccessors ?? throw new ArgumentNullException(nameof(mainBotAccessors));
            Logger = loggerFactory?.CreateLogger<MainBot>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                await turnContext.SendActivityAsync($"{ turnContext.Activity.Text}");
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
