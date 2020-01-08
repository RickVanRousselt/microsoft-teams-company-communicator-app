﻿// <copyright file="GetBotAccessTokenService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Send.Func.Services.BotAccessTokenServices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// Service to fetch access tokens for the bot.
    /// </summary>
    public class GetBotAccessTokenService
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBotAccessTokenService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpClient">The http client.</param>
        public GetBotAccessTokenService(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Fetches an access token for the bot.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<GetBotAccessTokenServiceResponse> GetTokenAsync()
        {
            var values = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", this.configuration["MicrosoftAppId"] },
                    { "client_secret", this.configuration["MicrosoftAppPassword"] },
                    { "scope", "https://api.botframework.com/.default" },
                };
            var content = new FormUrlEncodedContent(values);

            using (var tokenResponse = await this.httpClient.PostAsync("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", content))
            {
                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var accessTokenContent = await tokenResponse.Content.ReadAsAsync<AccessTokenResponse>();

                    var expiresInSeconds = 121;

                    // If parsing fails, out variable is set to 0, so need to set the default
                    if (!int.TryParse(accessTokenContent.ExpiresIn, out expiresInSeconds))
                    {
                        expiresInSeconds = 121;
                    }

                    return new GetBotAccessTokenServiceResponse
                    {
                        BotAccessToken = accessTokenContent.AccessToken,

                        // Remove two minutes in order to have a buffer amount of time.
                        BotAccessTokenExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(expiresInSeconds - 120),
                    };
                }
                else
                {
                    throw new Exception("Error fetching bot access token.");
                }
            }
        }

        private class AccessTokenResponse
        {
            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public string ExpiresIn { get; set; }

            [JsonProperty("ext_expires_in")]
            public string ExtExpiresIn { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }
    }
}
