// -----------------------------------------------------------------------
// <copyright file="ActionDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Dialogs
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Intents;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Security;

    /// <summary>
    /// Dialog that handles communication with the user.
    /// </summary>
    /// <seealso>
    ///     <cref>Microsoft.Bot.Builder.Dialogs.LuisDialog{string}</cref>
    /// </seealso>
    [Serializable]
    public class ActionDialog : LuisDialog<string>
    {
        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDialog"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public ActionDialog(IBotService service) :
            base(new LuisService(new LuisModelAttribute(service.Configuration.LuisAppId, service.Configuration.LuisApiKey)))
        {
            service.AssertNotNull(nameof(service));

            this.service = service;
        }

        /// <summary>
        /// Processes the request for help.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service (LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent("Help")]
        public async Task HelpAsync(IDialogContext context, LuisResult result)
        {
            CustomerPrincipal principal;
            IMessageActivity message;
            StringBuilder builder;

            try
            {
                message = context.MakeMessage();

                principal = await context.GetCustomerPrincipalAsync(this.service);

                if (principal == null)
                {
                    message.Text = Resources.NotAuthenticatedHelpMessage;
                }
                else
                {
                    builder = new StringBuilder();
                    builder.AppendLine($"{Resources.HelpMessage}\n\n");

                    principal.AvailableIntents.Aggregate(
                        builder, (sb, pair) => sb.AppendLine($"* {pair.Value.HelpMessage}\n"));

                    message.Text = builder.ToString();
                }

                await context.PostAsync(message);
                context.Wait(this.MessageReceived);
            }
            finally
            {
                builder = null;
                message = null;
                principal = null;
            }
        }

        /// <summary>
        /// Processes the request to list customers.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service (LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent(IntentConstants.ListCustomers)]
        public async Task ListCustomersAsync(IDialogContext context, LuisResult result)
        {
            await this.InvokeIntentAsync(context, result, IntentConstants.ListCustomers);
        }

        /// <summary>
        /// Processes the request to list subscriptions.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service (LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent(IntentConstants.ListSubscriptions)]
        public async Task ListSubscriptionsAsync(IDialogContext context, LuisResult result)
        {
            await this.InvokeIntentAsync(context, result, IntentConstants.ListSubscriptions);
        }

        /// <summary>
        /// Processes the request to select a customer.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service (LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent(IntentConstants.SelectCustomer)]
        public async Task SelectCustomerAsync(IDialogContext context, LuisResult result)
        {
            await this.InvokeIntentAsync(context, result, IntentConstants.SelectCustomer);
        }

        /// <summary>
        /// Processes any message received by the bot from the user that cannot be mapped to be an intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service(LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task NoMatchAsync(IDialogContext context, LuisResult result)
        {
            await this.HelpAsync(context, result);
        }

        /// <summary>
        /// Processes messages received by the bot from the user.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="item">The message from the conversation.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            IMessageActivity message;

            try
            {
                message = await item;

                if (message.Text.Equals(Resources.Login, StringComparison.CurrentCultureIgnoreCase))
                {
                    await context.Forward(
                        new AuthDialog(this.service, message),
                        this.ResumeAfterAuth,
                        message,
                        CancellationToken.None);
                }
                else
                {
                    await base.MessageReceived(context, item);
                }
            }
            finally
            {
                item = null;
            }
        }

        /// <summary>
        /// Invokes the execute function of the intent.
        /// </summary>
        /// <param name="context">The of the conversational process.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service (LUIS).</param>
        /// <param name="key">Key of the intent being invoked..</param>
        /// <returns>AN instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// </exception>
        private async Task InvokeIntentAsync(IDialogContext context, LuisResult result, string key)
        {
            CustomerPrincipal principal;

            context.AssertNotNull(nameof(context));
            result.AssertNotNull(nameof(result));
            key.AssertNotEmpty(nameof(key));

            try
            {
                principal = await context.GetCustomerPrincipalAsync(this.service);

                if (principal == null)
                {
                    return;
                }

                if (principal.AvailableIntents.ContainsKey(key.ToCamelCase()))
                {
                    await principal.AvailableIntents[key.ToCamelCase()]
                        .ExecuteAsync(context, result, this.service);
                }
                else
                {
                    await this.NoMatchAsync(context, result);
                }
            }
            finally
            {
                principal = null;
            }
        }

        /// <summary>
        /// Resumes the conversation once the authentication process has completed.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">The result returned from </param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            string message = await result;
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }
    }
}