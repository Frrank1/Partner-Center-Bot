// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Bot.Builder.Dialogs;
    using Security;

    /// <summary>
    /// Provides useful methods used for validation.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the customer principal from the private bot data associated with the user.
        /// </summary>
        /// <param name="context">The context for the bot.</param>
        /// <param name="service">Provides access to core application services.</param>
        /// <returns>An instance of <see cref="CustomerPrincipal"/> that represents the authenticated user.</returns>
        public static async Task<CustomerPrincipal> GetCustomerPrincipalAsync(this IBotContext context, IBotService service)
        {
            CustomerPrincipal principal = null;
            AuthenticationResult authResult;

            context.AssertNotNull(nameof(context));
            service.AssertNotNull(nameof(service));

            try
            {
                if (!context.PrivateConversationData.TryGetValue(BotConstants.CustomerPrincipalKey, out principal))
                {
                    return principal;
                }

                authResult = await service.TokenManagement.AcquireTokenSilentAsync(
                    $"{service.Configuration.ActiveDirectoryEndpoint}/{principal.CustomerId}",
                    service.Configuration.GraphEndpoint,
                    new UserIdentifier(principal.ObjectId, UserIdentifierType.UniqueId));

                principal.AccessToken = authResult.AccessToken;
                principal.ExpiresOn = authResult.ExpiresOn;

                context.StoreCustomerPrincipal(principal);

                return principal;
            }
            finally
            {
                authResult = null;
            }
        }

        /// <summary>
        /// Gets the text from the description attribute.
        /// </summary>
        /// <param name="value">The enumeration value associated with the attribute.</param>
        /// <returns>A <see cref="string"/> containing the text from the description attribute.</returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attribute;

            try
            {
                attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;

                return attribute == null ? value.ToString() : attribute.Description;
            }
            finally
            {
                attribute = null;
            }
        }

        /// <summary>
        /// Converts the value to camel case. 
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>A string in camel case notation.</returns>
        public static string ToCamelCase(this string value)
        {
            return value.Substring(0, 1).ToLower().Insert(1, value.Substring(1));
        }

        /// <summary>
        /// Ensures that a string is not empty.
        /// </summary>
        /// <param name="nonEmptyString">The string to validate.</param>
        /// <param name="caption">The name to report in the exception.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="nonEmptyString"/> is empty or null.
        /// </exception>
        public static void AssertNotEmpty(this string nonEmptyString, string caption)
        {
            if (string.IsNullOrWhiteSpace(nonEmptyString))
            {
                throw new ArgumentException($"{caption ?? "string"} is not set");
            }
        }

        /// <summary>
        /// Ensures that a given object is not null. Throws an exception otherwise.
        /// </summary>
        /// <param name="objectToValidate">The object we are validating.</param>
        /// <param name="caption">The name to report in the exception.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="objectToValidate"/> is null.
        /// </exception>
        public static void AssertNotNull(this object objectToValidate, string caption)
        {
            if (objectToValidate == null)
            {
                throw new ArgumentNullException(caption);
            }
        }

        /// <summary>
        /// Stores an instance of <see cref="CustomerPrincipal"/> in the private bot data associated with the user.
        /// </summary>
        /// <param name="context">The context for the bot.</param>
        /// <param name="principal">An instance of <see cref="CustomerPrincipal"/> associated with the authenticated user.</param>
        public static void StoreCustomerPrincipal(this IBotContext context, CustomerPrincipal principal)
        {
            context.AssertNotNull(nameof(context));
            principal.AssertNotNull(nameof(principal));

            context.PrivateConversationData.SetValue(BotConstants.CustomerPrincipalKey, principal);
        }
    }
}