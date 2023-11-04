using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ArchiveService.Dataverse.Api
{
    public class ChangeLog
    {
        [FunctionName("ChangeLog")]
        public void Run([ServiceBusTrigger("%changelog%", Connection = "changelogconnection")] string message, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
        }
    }
}
