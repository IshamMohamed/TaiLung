using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace TaiLung
{
    public class MainBot : IBot
    {
        private ILogger Logger { get; }
        private MainBotAccessors MyBotAccessors { get; }

        private DialogSet Dialogs { get; }

        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }
    }
}
