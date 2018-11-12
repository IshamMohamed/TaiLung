using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using TaiLung.States;

namespace TaiLung
{
    public class MainBotAccessors
    {
        private BotState generalBotState { get; }
        public IStatePropertyAccessor<MainState> mainState { get; }
        public IStatePropertyAccessor<DialogState> dialogState { get; } //DialogState comes from using Microsoft.Bot.Builder.Dialogs;

        public MainBotAccessors(BotState botState)
        {
            generalBotState = botState ?? throw new ArgumentNullException(nameof(botState));
            mainState = generalBotState.CreateProperty<MainState>($"{nameof(MainBotAccessors)}.{nameof(MainState)}");
            dialogState = generalBotState.CreateProperty<DialogState>($"{nameof(MainBotAccessors)}.{nameof(DialogState)}");
        }

        public Task SaveChangesAsync(ITurnContext turnContext) => generalBotState.SaveChangesAsync(turnContext);
    }
}