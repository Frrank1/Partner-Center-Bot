// -----------------------------------------------------------------------
// <copyright file="ListSubscriptionsIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Subscriptions;
    using RequestContext;
    using Security;

    /// <summary>
    /// Processes the request to list subscriptions.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class ListSubscriptionsIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.ListSubscriptionssHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.ListSubscriptions;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.AdminAgents | UserRoles.HelpdeskAgent | UserRoles.GlobalAdmin;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="service">Provides access to core services;.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="message"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// or 
        /// <paramref name="service"/> is null.
        /// </exception>
        public async Task ExecuteAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result, IBotService service)
        {
            Customer customer = null;
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IMessageActivity response;
            IPartner operations;
            ResourceCollection<Subscription> subscriptions;
            string name;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            service.AssertNotNull(nameof(principal));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                response = context.MakeMessage();
                operations = service.PartnerCenter.With(RequestContextFactory.Instance.Create(correlationId));
                principal = await context.GetCustomerPrincipalAsync(service);

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    customer = await operations.Customers.ById(principal.Operation.CustomerId).GetAsync();
                    subscriptions = await operations.Customers.ById(principal.Operation.CustomerId).Subscriptions.GetAsync();
                }
                else
                {
                    subscriptions = await operations.Customers.ById(principal.CustomerId).Subscriptions.GetAsync();
                }

                name = (customer == null) ? "the partner" : customer.CompanyProfile.CompanyName;
                response.Text = $"Here are the subscriptions for {name}";
                await context.PostAsync(response);

                response = context.MakeMessage();

                response.Attachments = subscriptions.Items.Select(s => new HeroCard(
                    null,
                    null,
                    null,
                    null,
                    new List<CardAction>
                    {
                        new CardAction
                        {
                            Title = s.FriendlyName,
                            Type = ActionTypes.PostBack,
                            Value = $"select subscription {s.Id}"
                        }
                    }).ToAttachment()).ToList();

                await context.PostAsync(response);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "PartnerCenterCorrelationId", correlationId.ToString() },
                    { "UserId", principal.ObjectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfSubscriptions", response.Attachments.Count }
                };

                service.Telemetry.TrackEvent("ListCustomers/Execute", eventProperties, eventMeasurements);
            }
            finally
            {
                customer = null;
                eventMeasurements = null;
                eventProperties = null;
                operations = null;
                principal = null;
                response = null;
                subscriptions = null;
            }
        }
    }
}