﻿// <copyright file="SendTriggersToSendFunctionActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.SendTriggersToAzureFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.UserData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueue;
    using Newtonsoft.Json;

    /// <summary>
    /// This class contains the following durable components:
    /// 1). The durable sub-orchestration ProcessRecipientBatchSubOrchestration.
    /// 2). And two durable activities, SendTriggersToSendFunctionAsync and SetRecipientBatchSatusAsync.
    /// The components work together to send triggers to the Azure send function.
    /// </summary>
    public class SendTriggersToSendFunctionActivity
    {
        private readonly SendQueue sendMessageQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTriggersToSendFunctionActivity"/> class.
        /// </summary>
        /// <param name="sendMessageQueue">Send message queue service.</param>
        public SendTriggersToSendFunctionActivity(SendQueue sendMessageQueue)
        {
            this.sendMessageQueue = sendMessageQueue;
        }

        /// <summary>
        /// Run the sub-orchestration.
        /// </summary>
        /// <param name="context">Durable orchestration context.</param>
        /// <param name="notificationDataEntityId">New sent notification id.</param>
        /// <param name="recipientDataBatch">A recipient data batch.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAsync(
            DurableOrchestrationContext context,
            string notificationDataEntityId,
            IEnumerable<UserDataEntity> recipientDataBatch)
        {
            await context.CallActivityWithRetryAsync(
                nameof(this.SendTriggersToSendFunctionAsync),
                new RetryOptions(TimeSpan.FromSeconds(5), 3),
                new SendTriggersToSendFunctionActivityDTO
                {
                    NotificationDataEntityId = notificationDataEntityId,
                    RecipientDataBatch = recipientDataBatch,
                });
        }

        /// <summary>
        /// This method represents the "send triggers to Azure service bus" activity.
        /// It sends triggers to the Azure send function.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [FunctionName(nameof(SendTriggersToSendFunctionAsync))]
        public async Task SendTriggersToSendFunctionAsync(
            [ActivityTrigger] SendTriggersToSendFunctionActivityDTO input)
        {
            var recipientDataBatch = input.RecipientDataBatch;
            var notificationDataEntityId = input.NotificationDataEntityId;

            var sendQueueMessageContentBatch = recipientDataBatch
                .Select(recipientData =>
                    new SendQueueMessageContent
                    {
                        NotificationId = notificationDataEntityId,
                        UserDataEntity = recipientData,
                    })
                .Where(sendQueueMessageContent => sendQueueMessageContent != null);

            await this.sendMessageQueue.SendAsync(sendQueueMessageContentBatch);
        }
    }
}
