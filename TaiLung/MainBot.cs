using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaiLung;
using TaiLung.Translation;

namespace TaiLung
{
    public class MainBot : IBot
    {
        private const string English = "en";
        private const string Japanese = "ja";
        private const string Tamil = "ta";
        private const string Chinese = "zh";

        private ILogger Logger { get; }
        private MainBotAccessors MainBotAccessors { get; }
        private DialogSet Dialogs { get; }

        public MainBot(MainBotAccessors mainBotAccessors, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger<MainBot>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            MainBotAccessors = mainBotAccessors ?? throw new ArgumentNullException(nameof(MainBotAccessors));
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
                string userLanguage = await MainBotAccessors.LanguagePreference.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage) ?? TranslationSettings.DefaultLanguage;
                bool translate = userLanguage != TranslationSettings.DefaultLanguage;
                bool isLoginPrompted = await MainBotAccessors.IsLoginPrompted.GetAsync(turnContext, () => false);
                bool isAuthenticated = await MainBotAccessors.IsAuthenticated.GetAsync(turnContext, () => false);

                if (IsLanguageChangeRequested(turnContext.Activity.Text))
                {
                    await MainBotAccessors.LanguagePreference.SetAsync(turnContext, turnContext.Activity.Text);
                    var reply = turnContext.Activity.CreateReply($"Your current language code is: {turnContext.Activity.Text}");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else if (isAuthenticated)
                {
                    var dialogContext = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                    if (results.Status == DialogTurnStatus.Empty)
                    {
                        await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                    }
                }
                else if (!isLoginPrompted)
                {
                    var reply = turnContext.Activity.CreateReply();

                    var card = new HeroCard
                    {
                        Buttons = new List<CardAction>()
                        {
                            //new CardAction(title: "Sign In", type: ActionTypes.PostBack, value: "Please sign in to your Azure account"),
                            new CardAction(ActionTypes.OpenUrl, title: "Click here to sign in", value: "https://azure.microsoft.com/en-us/services/bot-service/"),
                        },
                    };

                    reply.Attachments = new List<Attachment>() { card.ToAttachment() };
                    //IsLoginPrompted = true;
                    await MainBotAccessors.IsLoginPrompted.SetAsync(turnContext, true);
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else if (isLoginPrompted && !isAuthenticated)
                {
                    var reply = turnContext.Activity.Text;
                    if (reply.Equals("3344"))
                        //IsAuthenticated = true;
                        await MainBotAccessors.IsAuthenticated.SetAsync(turnContext, true);
                }
                await MainBotAccessors.SaveChangesAsync(turnContext);
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }

            Logger.LogInformation($"{nameof(OnTurnAsync)} ended");
        }

        private static bool IsLanguageChangeRequested(string utterance)
        {
            if (string.IsNullOrEmpty(utterance))
            {
                return false;
            }

            utterance = utterance.ToLower().Trim();
            return utterance == English || utterance == Japanese || utterance == Tamil || utterance == Chinese;
        }
    }
}