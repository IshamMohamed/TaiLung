using Microsoft.Bot.Builder;
using System;
using System.Threading.Tasks;
using TaiLung.States;

namespace TaiLung
{
    public class MainBotAccessors
    {
        private BotState generalBotState { get; }
        public IStatePropertyAccessor<MainState> mainState { get; }

        public MainBotAccessors(BotState botState)
        {
            generalBotState = botState ?? throw new ArgumentNullException(nameof(botState));
            mainState = generalBotState.CreateProperty<MainState>($"{nameof(MainBotAccessors)}.{nameof(MainState)}");
        }

        public Task SaveChangesAsync(ITurnContext turnContext) => generalBotState.SaveChangesAsync(turnContext);
    }
}