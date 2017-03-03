﻿// -----------------------------------------------------------------------
// <copyright file="DistributedTokenCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Cache
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using IdentityModel.Clients.ActiveDirectory;
    using Logic;

    /// <summary>
    /// Custom implementation of the <see cref="TokenCache"/> class.
    /// </summary>
    /// <seealso cref="TokenCache" />
    public class DistributedTokenCache : TokenCache
    {
        /// <summary>
        /// Claim type for the object identifier claim.
        /// </summary>
        private const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// Resource that is being accessed.
        /// </summary>
        private readonly string resource;

        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedTokenCache"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application services.</param>
        /// <param name="resource">The resource being accessed.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null. 
        /// </exception>
        public DistributedTokenCache(IBotService service, string resource)
        {
            resource.AssertNotEmpty(nameof(resource));

            this.service = service;
            this.resource = resource;

            this.AfterAccess = this.AfterAccessNotification;
            this.BeforeAccess = this.BeforeAccessNotification;
        }

        /// <summary>
        /// Gets the key to be utilized for interfacing with the cache.
        /// </summary>
        private string Key => $"Resource::{this.resource}::Identifier::{ClaimsPrincipal.Current.Identities.First().FindFirst(ObjectIdClaimType).Value}";

        /// <summary>
        /// Notification method called after any library method accesses the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the ADAL call accessing the cache.</param>
        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!this.HasStateChanged)
            {
                return;
            }

            if (this.Count > 0)
            {
                this.service.Cache.Store(
                    CacheDatabaseType.Authentication, this.Key, Convert.ToBase64String(this.Serialize()));
            }
            else
            {
                this.service.Cache.Delete(CacheDatabaseType.Authentication, this.Key);
            }

            this.HasStateChanged = false;
        }

        /// <summary>
        /// Notification method called before any library method accesses the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the ADAL call accessing the cache.</param>
        public void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            string value = this.service.Cache.Fetch<string>(CacheDatabaseType.Authentication, this.Key);

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            this.Deserialize(Convert.FromBase64String(value));
        }

        /// <summary>
        /// Clears the cache by deleting all the items. Note that if the cache is the default shared cache, clearing it would
        /// impact all the instances of <see cref="T:Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" /> which share that cache.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            this.service.Cache.Clear(CacheDatabaseType.Authentication);
        }

        /// <summary>
        /// Deletes an item from the cache.
        /// </summary>
        /// <param name="item">The item to delete from the cache.</param>
        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            this.service.Cache.Delete(CacheDatabaseType.Authentication, this.Key);
        }
    }
}