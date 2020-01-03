﻿// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(
    typeof(Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.Startup))]

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.UserData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.BotConnectorClient;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend;
    using Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.GetRecipientDataBatches;
    using Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.SendTriggersToAzureFunctions;

    /// <summary>
    /// Register services in DI container of the Azure functions system.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<BotConnectorClientFactory>();

            builder.Services.Configure<RepositoryOptions>(repositoryOptions =>
            {
                repositoryOptions.IsAzureFunction = true;
            });
            builder.Services.AddSingleton<NotificationDataRepository>();
            builder.Services.AddSingleton<SendingNotificationDataRepository>();
            builder.Services.AddSingleton<SentNotificationDataRepository>();
            builder.Services.AddSingleton<UserDataRepository>();
            builder.Services.AddSingleton<TeamDataRepository>();
            builder.Services.AddTransient<TableRowKeyGenerator>();

            builder.Services.AddSingleton<SendQueue>();
            builder.Services.AddSingleton<DataQueue>();

            builder.Services.AddTransient<AdaptiveCardCreator>();

            builder.Services.AddTransient<PreparingToSendOrchestration>();
            builder.Services.AddTransient<GetRecipientDataListForAllUsersActivity>();
            builder.Services.AddTransient<GetTeamDataEntitiesByIdsActivity>();
            builder.Services.AddTransient<GetRecipientDataListForRosterActivity>();
            builder.Services.AddTransient<GetRecipientDataListForTeamsActivity>();
            builder.Services.AddTransient<CreateSendingNotificationActivity>();
            builder.Services.AddTransient<SendTriggersToSendFunctionActivity>();
            builder.Services.AddTransient<SendTriggerToDataFunctionActivity>();
            builder.Services.AddTransient<ProcessRecipientDataListActivity>();
            builder.Services.AddTransient<HandleFailureActivity>();
        }
    }
}