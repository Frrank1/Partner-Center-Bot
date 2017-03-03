// -----------------------------------------------------------------------
// <copyright file="MessagesController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Autofac;
    using Dialogs;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Security;

    /// <summary>
    /// Provides the ability to handle messages.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [CustomBotAuthentication]
    [RoutePrefix("api/messages")]
    public class MessagesController : BaseApiController
    {
        /// <summary>
        /// Initializes static members of the <see cref="MessagesController"/> class.
        /// </summary>
        static MessagesController()
        {
            ContainerBuilder builder;

            try
            {
                builder = new ContainerBuilder();

                builder.Register(c =>
                {
                    using (ILifetimeScope scope = WebApiApplication.Container.BeginLifetimeScope())
                    {
                        IBotService service = scope.Resolve<IBotService>();
                        return new MicrosoftAppCredentials(
                            service.Configuration.MicrosoftAppId,
                            service.Configuration.MicrosoftAppPassword);
                    }
                }).SingleInstance();

#pragma warning disable 0618
                builder.Update(Conversation.Container);
#pragma warning restore 0618
            }
            finally
            {
                builder = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public MessagesController(IBotService service) : base(service)
        {
        }

        /// <summary>
        /// Processes messages received from a user. 
        /// </summary>
        /// <param name="activity">Represents the message received from a user.</param>
        /// <returns>A HTTP status code that reflects whether the request was successful or not.</returns>
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> PostAsync([FromBody]Activity activity)
        {
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;

            try
            {
                startTime = DateTime.Now;

                if (activity.GetActivityType() == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new ActionDialog(this.Service));
                }

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", activity.ChannelId },
                    { "Locale", activity.Locale },
                    { "ActivityType", activity.Type }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                this.Service.Telemetry.TrackEvent("api/messages", eventProperties, eventMeasurements);

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            finally
            {
                eventMeasurements = null;
                eventProperties = null;
            }
        }
    }
}