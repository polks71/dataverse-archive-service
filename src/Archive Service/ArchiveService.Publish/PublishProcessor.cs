using System;
using System.Threading.Tasks;
using ArchiveService.Shared.Managers.PublishManager;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;

namespace ArchiveService.Publish
{
    public class PublishProcessor
    {
        
        private readonly IPublishManager _publishManager;
        public PublishProcessor(IPublishManager publishManager)
        {
            _publishManager= publishManager;
        }
        [FunctionName("Publish")]
        public async Task Run([ServiceBusTrigger("%publish%", Connection = "publishqueueconnection")]string message, ILogger log)
        {
            log.LogInformation($"MessageReceived: {message}");
            var context = ArchiveService.Shared.RemoteContextDeserializer.DeserializeJsonString<RemoteExecutionContext>(message);
            await _publishManager.RunAsync(context, log);
        }
    }
}
