using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaiLung;

namespace TaiLung
{
    public class MainBot : IBot
    {
        private ILogger Logger { get; }
        private MainBotAccessors MainBotAccessors { get; }

        private DialogSet Dialogs { get; }

        public MainBot(MainBotAccessors mainBotAccessors, ILoggerFactory loggerFactory)
        {
            MainBotAccessors = mainBotAccessors ?? throw new ArgumentNullException(nameof(MainBotAccessors));
            Logger = loggerFactory?.CreateLogger<MainBot>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            Dialogs = new DialogSet(MainBotAccessors.ConversationDialogState);

            // Register WaterfallDialog. Pass an array of Task <DialogTurnResult> Xxx (WaterfallStepContext, CancellationToken) as an argument. 
            // This time I wrote in lambda to put it in one place, but I think that it is usually better to pass some class method. 
            Dialogs.Add(new WaterfallDialog("details", new WaterfallStep[]
            {
                (stepContext, cancellationToken) => stepContext.PromptAsync("name", new PromptOptions { Prompt = MessageFactory.Text("What is your name?") }, cancellationToken),
                async (stepContext, cancellationToken) =>
                {
                    var userProfile = await MainBotAccessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                    userProfile.Name = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync($"Thanks {userProfile.Name} !!", cancellationToken: cancellationToken);
                    return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Can you tell me your age?") }, cancellationToken);
                },
                async (stepContext, cancellationToken) =>
                {
                    if ((bool)stepContext.Result)
                    {
                        return await stepContext.PromptAsync("age", new PromptOptions {Prompt = MessageFactory.Text("So, how old are you?")}, cancellationToken);
                    }
                    else
                    {
                        // Let's say that the age is -1 and next
                        return await stepContext.NextAsync(-1, cancellationToken);
                    }
                },
                async (stepContext, cancellationToken) =>
                {
                    var userProfile = await MainBotAccessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                    userProfile.Age = (int)stepContext.Result;
                    if (userProfile.Age == -1)
                    {
                        // Age was canceled
                        await stepContext.Context.SendActivityAsync($"It's mysterious!!", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // I got my age
                        await stepContext.Context.SendActivityAsync($"{userProfile.Age} years old!!", cancellationToken: cancellationToken);
                   }

                    return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Is it correct?") }, cancellationToken);
                },
                async (stepContext, cancellationToken) =>
                {
                    if ((bool)stepContext.Result)
                    {
                        var userProfile = await MainBotAccessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                        if (userProfile.Age == -1)
                        {
                            await stepContext.Context.SendActivityAsync($"Mysterious {userProfile.Name}", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync($"{userProfile.Name} is {userProfile.Age} years old!", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Well then, do not forget to forget about you!", cancellationToken: cancellationToken);
                        await MainBotAccessors.UserProfile.DeleteAsync(stepContext.Context, cancellationToken);
                    }

                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }));

            // We also add a dialog calling with PromptAsync in WaterfallDialog. 
            Dialogs.Add(new TextPrompt("name"));
            Dialogs.Add(new NumberPrompt<int>("age"));
            Dialogs.Add(new ConfirmPrompt("confirm"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.LogInformation($"{nameof(OnTurnAsync)} started");
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                }

                await MainBotAccessors.SaveChangesAsync(turnContext);
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }

            Logger.LogInformation($"{nameof(OnTurnAsync)} ended");
        }
    }
}