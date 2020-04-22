using BlazorChatApp.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatApp.Functions
{
    public static class SignalRFunctions
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "chat")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static async Task NewMessage(
            [CosmosDBTrigger(databaseName: "Chat", collectionName: "Messages", ConnectionStringSetting = "CosmosDBConnection", LeaseCollectionName = "leases", CreateLeaseCollectionIfNotExists = true, FeedPollDelay = 500, StartFromBeginning = true)] IReadOnlyList<Document> documents, 
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformation($"Received {documents.Count} new messages.");
            foreach(var document in documents)
            {
                var message = new Message
                {
                    DateTime = document.GetPropertyValue<DateTime>(nameof(Message.DateTime)),
                    Content = document.GetPropertyValue<string>(nameof(Message.Content)),
                    SourceUserName = document.GetPropertyValue<string>(nameof(Message.SourceUserName)),
                    TargetUserName = document.GetPropertyValue<string>(nameof(Message.TargetUserName)),
                };

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = $"newMessage-{message.TargetUserName}",
                        Arguments = new object[] { message }
                    });
            }
        }
    }
}
