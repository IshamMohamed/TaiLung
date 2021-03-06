﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using TaiLung.States;

namespace TaiLung
{
    public class MainBotAccessors
    {
        private ConversationState ConversationState { get; }
        private UserState UserState { get; }
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; }
        public IStatePropertyAccessor<UserProfile> UserProfile { get; }
        public IStatePropertyAccessor<string> LanguagePreference { get; set; }
        public IStatePropertyAccessor<bool> IsLoginPrompted { get; set; }
        public IStatePropertyAccessor<bool> IsAuthenticated { get; set; }

        public MainBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationDialogState = ConversationState.CreateProperty<DialogState>($"{nameof(MainBotAccessors)}.{nameof(ConversationDialogState)}");
            UserProfile = UserState.CreateProperty<UserProfile>($"{nameof(MainBotAccessors)}.{nameof(UserProfile)}");
            LanguagePreference = UserState.CreateProperty<string>("LanguagePreference");
            IsLoginPrompted = UserState.CreateProperty<bool>("IsLoginPrompted");
            IsAuthenticated = UserState.CreateProperty<bool>("IsAuthenticated");
        }

        public Task SaveConversationStateChangesAsync(ITurnContext turnContext) => ConversationState.SaveChangesAsync(turnContext);
        public Task SaveUserStateChangesAsync(ITurnContext turnContext) => UserState.SaveChangesAsync(turnContext);

        public async Task SaveChangesAsync(ITurnContext turnContext)
        {
            await SaveConversationStateChangesAsync(turnContext);
            await SaveUserStateChangesAsync(turnContext);
        }

    }
}