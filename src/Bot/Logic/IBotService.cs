﻿// -----------------------------------------------------------------------
// <copyright file="IBotService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Threading.Tasks;
    using Cache;
    using Configuration;
    using Intents;
    using Security;
    using Telemetry;

    /// <summary>
    /// Represents the core service that powers the application.
    /// </summary>
    public interface IBotService
    {
        /// <summary>
        /// Gets the service that provides caching functionality.
        /// </summary>
        ICacheService Cache { get; }

        /// <summary>
        /// Gets a reference to the available configurations.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the service that provides access to the supported intents.
        /// </summary>
        IIntentService Intent { get; }

        /// <summary>
        /// Gets the service that provides localization functionality.
        /// </summary>
        ILocalizationService Localization { get; }

        /// <summary>
        /// Gets the Partner Center service reference.
        /// </summary>
        IAggregatePartner PartnerCenter { get; }

        /// <summary>
        /// Gets the telemetry service reference.
        /// </summary>
        ITelemetryProvider Telemetry { get; }

        /// <summary>
        /// Gets the a reference to the token management service.
        /// </summary>
        ITokenManagement TokenManagement { get; }

        /// <summary>
        /// Gets a reference to the vault service. 
        /// </summary>
        IVaultService Vault { get; }

        /// <summary>
        /// Initializes the bot service and all the dependent services.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task InitializeAsync();
    }
}