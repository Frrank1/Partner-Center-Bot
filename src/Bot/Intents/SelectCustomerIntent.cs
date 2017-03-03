// -----------------------------------------------------------------------
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
        /// <param name="result">The message in the conversation.</param>
        /// <param name="service">Provides access to core services.</param>
        /// <returns>
        /// An instance of <see cref="Task" /> that represents the asynchronous operation.
        /// </returns>
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
            Customer customer;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            EntityRecommendation customerEntity;
            IMessageActivity message;
            string customerId = string.Empty;

            context.AssertNotNull(nameof(context));
            result.AssertNotNull(nameof(result));
            service.AssertNotNull(nameof(service));

            try
            {
                startTime = DateTime.Now;

                message = context.MakeMessage();
                principal = await context.GetCustomerPrincipalAsync(service);

                if (result.TryFindEntity("customer", out customerEntity))
                {
                    customerId = customerEntity.Entity.Replace(" ", string.Empty);
                    principal.Operation.CustomerId = customerId;
                    context.StoreCustomerPrincipal(principal);
                }

                if (string.IsNullOrEmpty(customerId))
                {
                    message.Text = Resources.UnableToLocateCustomer;
                }
                else
                {
                    customer = await service.PartnerCenter.Customers.ById(customerId).GetAsync();
                    message.Text = $"Customer context is now configured for {customer.CompanyProfile.CompanyName}";
                }

                await context.PostAsync(message);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", customerId },
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
                customerEntity = null;
                eventMeasurements = null;
                eventProperties = null;
                message = null;
            }
        }
    }
}