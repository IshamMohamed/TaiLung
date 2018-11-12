using System;
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
            Logger.LogInformation($"{nameof (OnTurnAsync)} started");

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var state = await MainBotAccessors.mainState.GetAsync(turnContext, () => new States.MainState());
                await turnContext.SendActivityAsync($". {state.GeneralProperty++} th of the exchange I was referred to as the {turnContext.Activity.Text}." );
                await MainBotAccessors.mainState.SetAsync(turnContext, state);
                await MainBotAccessors.SaveChangesAsync(turnContext);
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }

            Logger.LogInformation($"{nameof (OnTurnAsync)} ended");

        }
    }
}
