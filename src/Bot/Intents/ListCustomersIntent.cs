// -----------------------------------------------------------------------
// <copyright file="ListCustomersIntent.cs" company="Microsoft">
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
    using RequestContext;
    using Security;

    /// <summary>
    /// Processes the request to list customers.
    /// </summary>
    /// <seealso cref="IIntent" />
    [Serializable]
    public class ListCustomersIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.ListCustomersHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.ListCustomers;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.AdminAgents | UserRoles.HelpdeskAgent;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">The message in the conversation.</param>
        /// <param name="service">Provides access to core services.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// or 
        /// <paramref name="service"/> is null.
        /// </exception>
        public async Task ExecuteAsync(IDialogContext context, LuisResult result, IBotService service)
        {
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IMessageActivity response;
            IPartner operations;
            SeekBasedResourceCollection<Customer> customers;

            context.AssertNotNull(nameof(context));
            result.AssertNotNull(nameof(result));
            service.AssertNotNull(nameof(service));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();

                operations = service.PartnerCenter.With(
                    RequestContextFactory.Instance.Create(correlationId));

                customers = await operations.Customers.GetAsync();

                response = context.MakeMessage();
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                response.Attachments = customers.Items.Select(c => new HeroCard(
                    null,
                    null,
                    null,
                    null,
                    new List<CardAction>
                    {
                            new CardAction
                            {
                                Title = c.CompanyProfile.CompanyName,
                                Type = ActionTypes.PostBack,
                                Value = $"select customer {c.Id}"
                            }
                    }).ToAttachment()).ToList();

                await context.PostAsync(response);

                principal = await context.GetCustomerPrincipalAsync(service);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "Locale", response.Locale },
                    { "PartnerCenterCorrelationId", correlationId.ToString() },
                    { "UserId", principal.ObjectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfCustomers", response.Attachments.Count }
                };

                service.Telemetry.TrackEvent("ListCustomers/Execute", eventProperties, eventMeasurements);
            }
            finally
            {
                customers = null;
                eventMeasurements = null;
                eventProperties = null;
                operations = null;
            }
        }
    }
}