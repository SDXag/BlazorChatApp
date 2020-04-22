using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using BlazorChatApp.Shared.Models;
using System.Linq;

namespace BlazorChatApp.Functions
{
    public static class GetMessages
    {
        [FunctionName("GetMessages")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "GetMessages/{userName}")] HttpRequest req,
            [CosmosDB(databaseName: "Chat", collectionName: "Messages", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "SELECT * FROM Messages m WHERE m.SourceUserName = {userName} OR m.TargetUserName = {userName}")] IEnumerable<Message> messages,
            ILogger log)
        {
            log.LogInformation($"{messages.Count()} items loaded");
            return new OkObjectResult(messages);
        }
    }
}
