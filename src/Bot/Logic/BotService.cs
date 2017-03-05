// -----------------------------------------------------------------------
// <copyright file="BotService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System;
    using System.Threading.Tasks;
    using Cache;
    using Configuration;
    using Intents;
    using Security;
    using Telemetry;

    /// <summary>
    /// Provides access to core services.
    /// </summary>
    /// <seealso cref="IBotService" />
    [Serializable]
    public class BotService : IBotService
    {
        /// <summary>
        /// Provides the ability to cache often used objects. 
        /// </summary>
        private static ICacheService cache;

        /// <summary>
        /// Provides the ability to access various configurations.
        /// </summary>
        private static IConfiguration configuration;

        /// <summary>
        /// Provides access to the supported intents.
        /// </summary>
        private static IIntentService intent;

        /// <summary>
        /// Provides localization functionality for the bot.
        /// </summary>
        private static ILocalizationService localization;

        /// <summary>
        /// Provides the ability to track telemetry data.
        /// </summary>
        private static ITelemetryProvider telemetry;

        /// <summary>
        /// Provides the ability to manage access tokens.
        /// </summary>
        private static ITokenManagement tokenManagement;

        /// <summary>
        /// Provides the ability to retrieve and store data in a secure vault.
        /// </summary>
        private static IVaultService vault;

        /// <summary>
        /// Gets the service that provides caching functionality.
        /// </summary>
        public ICacheService Cache => cache ?? (cache = new CacheService(this));

        /// <summary>
        /// Gets a reference to the available configurations.
        /// </summary>
        public IConfiguration Configuration => configuration ?? (configuration = new Configuration(this));

        /// <summary>
        /// Gets the service that provides access to the supported intents.
        /// </summary>
        public IIntentService Intent => intent;

        /// <summary>
        /// Gets the service that provide localization functionality.
        /// </summary>
        public ILocalizationService Localization => localization;

        /// <summary>
        /// Gets the Partner Center service reference.
        /// </summary>
        public IAggregatePartner PartnerCenter => PartnerService.Instance.CreatePartnerOperations(
            this.TokenManagement.GetPartnerCenterAppOnlyCredentials(
                $"{this.Configuration.ActiveDirectoryEndpoint}/{this.Configuration.PartnerCenterApplicationTenantId}"));

        /// <summary>
        /// Gets the telemetry service reference.
        /// </summary>
        public ITelemetryProvider Telemetry
        {
            get
            {
                if (telemetry != null)
                {
                    return telemetry;
                }

                if (string.IsNullOrEmpty(this.Configuration.InstrumentationKey))
                {
                    telemetry = new EmptyTelemetryProvider();
                }
                else
                {
                    telemetry = new ApplicationInsightsTelemetryProvider();
                }

                return telemetry;
            }
        }

        /// <summary>
        /// Gets the a reference to the token management service.
        /// </summary>
        public ITokenManagement TokenManagement => tokenManagement ?? (tokenManagement = new TokenManagement(this));

        /// <summary>
        /// Gets a reference to the vault service.
        /// </summary>
        public IVaultService Vault => vault ?? (vault = new VaultService(this));

        /// <summary>
        /// Initializes the application core services.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        public async Task InitializeAsync()
        {
            intent = new IntentService(this);
            localization = new LocalizationService(this);

            intent.Initialize();
            await localization.InitializeAsync();
        }
    }
}