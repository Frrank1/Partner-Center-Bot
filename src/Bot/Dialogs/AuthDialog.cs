// -----------------------------------------------------------------------
// <copyright file="AuthDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Security;

    /// <summary>
    /// Dialog that handles authentication requests.
    /// </summary>
    [Serializable]
    public class AuthDialog : IDialog<string>
    {
        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// The object that relates to a particular point in the conversation.
        /// </summary>
        private readonly ResumptionCookie resumptionCookie;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDialog"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application services.</param>
        /// <param name="message">Message received by the bot from the end user.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// or
        /// <paramref name="message"/> is null.
        /// </exception>
        public AuthDialog(IBotService service, IMessageActivity message)
        {
            service.AssertNotNull(nameof(service));
            message.AssertNotNull(nameof(message));

            this.service = service;
            this.resumptionCookie = new ResumptionCookie(message);

            // this.cookie = message.CreateConversationReference();
        }

        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task StartAsync(IDialogContext context)
        {
            await this.AuthenticateAsync(context);
        }

        /// <summary>
        /// Processes a message in a conversation between the bot and a user.
        /// </summary>
        /// <param name="context">The context for the execution of a dialogs conversational process.</param>
        /// <param name="argument">The message in a conversation between the bot and a user.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            CustomerPrincipal principal;
            IMessageActivity message;

            try
            {
                message = await argument;

                if (context.PrivateConversationData.TryGetValue(BotConstants.CustomerPrincipalKey, out principal))
                {
                    context.Done(string.Format(Resources.AuthenticationSuccessDoneMessage, principal.Name));
                }
                else
                {
                    context.Wait(this.MessageReceivedAsync);
                }
            }
            finally
            {
                message = null;
                principal = null;
            }
        }

        /// <summary>
        /// Processes the authentication request from the end user.
        /// </summary>
        /// <param name="context">The dialog context</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        private async Task AuthenticateAsync(IDialogContext context)
        {
            IMessageActivity message;
            Uri redirectUri;
            string authUrl;
            string state;

            try
            {
                redirectUri = new Uri($"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/{BotConstants.CallbackPath}");

                state = $"&state={this.GenerateState(context, this.resumptionCookie)}";

                authUrl = await this.service.TokenManagement.GetAuthorizationRequestUrlAsync(
                    $"{this.service.Configuration.ActiveDirectoryEndpoint}/{BotConstants.AuthorityEndpoint}",
                    redirectUri,
                    this.service.Configuration.GraphEndpoint,
                    state);

                message = context.MakeMessage();

                message.Attachments.Add(SigninCard.Create(
                    Resources.SigninCardText, Resources.LoginCaptial, authUrl).ToAttachment());

                await context.PostAsync(message);
                context.Wait(this.MessageReceivedAsync);
            }
            finally
            {
                message = null;
            }
        }

        /// <summary>
        /// Generates the state to be utilized with the authentication request.
        /// </summary>
        /// <param name="context">Context for the dialog.</param>
        /// <param name="cookie">Used to resume the conversation with the user.</param>
        /// <returns>A string that represents the current state for the user.</returns>
        private string GenerateState(IBotData context, ResumptionCookie cookie)
        {
            Dictionary<string, string> state;
            Guid uniqueId = Guid.NewGuid();

            cookie.AssertNotNull(nameof(cookie));

            try
            {
                state = new Dictionary<string, string>
                {
                    { BotConstants.BotIdKey, cookie.Address.BotId },
                    { BotConstants.ChannelIdKey, cookie.Address.ChannelId },
                    { BotConstants.ConversationIdKey, cookie.Address.ConversationId },
                    { BotConstants.UniqueIdentifierKey, uniqueId.ToString() },
                    { BotConstants.LocaleKey, cookie.Locale },
                    { BotConstants.ServiceUrlKey, cookie.Address.ServiceUrl },
                    { BotConstants.UserIdKey, cookie.Address.UserId }
                };

                // Save the unique identifier in the user's private conversation store. This value will be 
                // utilized to verify the authentication request. 
                context.PrivateConversationData.SetValue(BotConstants.UniqueIdentifierKey, uniqueId);

                return UrlToken.Encode(state);
            }
            finally
            {
                state = null;
            }
        }
    }
}