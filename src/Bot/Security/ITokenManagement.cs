// -----------------------------------------------------------------------
// <copyright file="ITokenManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Represents a management interface for retrieving access tokens.
    /// </summary>
    public interface ITokenManagement
    {
        /// <summary>
        /// Acquires an access token without asking for user credential.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the client requesting the token.</param>
        /// <param name="userId">Identifier of the user that is requesting the token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represents the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="userId"/> is null.
        /// </exception>
        AuthenticationResult AcquireTokenSilent(string authority, string resource, UserIdentifier userId);

        /// <summary>
        /// Acquires an access token without asking for user credential.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the client requesting the token.</param>
        /// <param name="userId">Identifier of the user that is requesting the token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represents the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="userId"/> is null.
        /// </exception>
        Task<AuthenticationResult> AcquireTokenSilentAsync(string authority, string resource, UserIdentifier userId);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        AuthenticationToken GetAppOnlyToken(string authority, string resource);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        Task<AuthenticationToken> GetAppOnlyTokenAsync(string authority, string resource);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="scope">Permissions the requested token will need.</param>
        /// <returns>A string that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        string GetAppOnlyToken(string authority, string resource, string scope);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="scope">Permissions the requested token will need.</param>
        /// <returns>A string that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        Task<string> GetAppOnlyTokenAsync(string authority, string resource, string scope);

        /// <summary>
        /// Gets an access token from the authority using app + user authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="token">Access token for the user requesting the resource.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// or 
        /// <paramref name="token"/> is empty or null.
        /// </exception>
        AuthenticationToken GetAppPlusUserToken(string authority, string resource, string token);

        /// <summary>
        /// Gets an access token from the authority using app + user authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="token">Access token for the user requesting the resource.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// or
        /// <paramref name="token"/> is empty or null.
        /// </exception>
        Task<AuthenticationToken> GetAppPlusUserTokenAsync(string authority, string resource, string token);

        /// <summary>
        /// Gets the URL of the authorization endpoint including the query parameters.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="extraQueryParameters">Data that will be appended as is to the query string in the HTTP authentication request to the authority.</param>
        /// <returns>URL of the authorization endpoint including the query parameters.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or 
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        string GetAuthorizationRequestUrl(string authority, Uri redirectUri, string resource, string extraQueryParameters);

        /// <summary>
        /// Gets the URL of the authorization endpoint including the query parameters.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="extraQueryParameters">Data that will be appended as is to the query string in the HTTP authentication request to the authority.</param>
        /// <returns>URL of the authorization endpoint including the query parameters.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or 
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        Task<string> GetAuthorizationRequestUrlAsync(string authority, Uri redirectUri, string resource, string extraQueryParameters);

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        /// <remarks>
        /// This function will use app only authentication to obtain the credentials.
        /// </remarks>
        IPartnerCredentials GetPartnerCenterAppOnlyCredentials(string authority);

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        /// <remarks>
        /// This function will use app only authentication to obtain the credentials.
        /// </remarks>
        Task<IPartnerCredentials> GetPartnerCenterAppOnlyCredentialsAsync(string authority);

        /// <summary>
        /// Gets an access token utilizing an authorization code. 
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="code">Authorization code received from the service authorization endpoint.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="redirectUri">Redirect URI used for obtain the authorization code.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="code"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        AuthenticationResult GetTokenByAuthorizationCode(string authority, string code, string resource, Uri redirectUri);

        /// <summary>
        /// Gets an access token utilizing an authorization code. 
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="code">Authorization code received from the service authorization endpoint.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="redirectUri">Redirect URI used for obtain the authorization code.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="code"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// redirectUri
        /// </exception>
        Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string authority, string code, string resource, Uri redirectUri);
    }
}