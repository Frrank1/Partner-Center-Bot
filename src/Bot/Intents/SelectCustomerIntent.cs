﻿// -----------------------------------------------------------------------
// <copyright file="SelectCustomerIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Customers;
    using RequestContext;
    using Security;

    /// <summary>
    /// Processes the request to select a specific customer.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class SelectCustomerIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => string.Empty;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.SelectCustomer;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.AdminAgents | UserRoles.HelpdeskAgent;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="service">Provides access to core services.</param>
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
            CustomerPrincipal principal;
            Customer customer;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            EntityRecommendation indentifierEntity;
            Guid correlationId;
            IMessageActivity response;
            IPartner operations;
            string customerId = string.Empty;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            service.AssertNotNull(nameof(service));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                response = context.MakeMessage();

                operations = service.PartnerCenter.With(RequestContextFactory.Instance.Create(correlationId));

                principal = await context.GetCustomerPrincipalAsync(service);

                if (result.TryFindEntity("identifier", out indentifierEntity))
                {
                    customerId = indentifierEntity.Entity.Replace(" ", string.Empty);
                    principal.Operation.CustomerId = customerId;
                    context.StoreCustomerPrincipal(principal);
                }

                if (string.IsNullOrEmpty(customerId))
                {
                    response.Text = Resources.UnableToLocateCustomer;
                }
                else
                {
                    customer = await operations.Customers.ById(customerId).GetAsync();
                    response.Text = $"Customer context is now configured for {customer.CompanyProfile.CompanyName}";
                }

                await context.PostAsync(response);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", customerId },
                    { "PartnerCenterCorrelationId", correlationId.ToString() },
                    { "PrincipalCustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "UserId", principal.ObjectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                service.Telemetry.TrackEvent("SelectCustomer/Execute", eventProperties, eventMeasurements);
            }
            finally
            {
                customer = null;
                indentifierEntity = null;
                eventMeasurements = null;
                eventProperties = null;
                message = null;
                operations = null;
            }
        }
    }
}