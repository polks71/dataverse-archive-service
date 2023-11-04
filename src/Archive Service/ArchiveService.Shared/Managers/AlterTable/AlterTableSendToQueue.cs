using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.AlterTable
{
    public class AlterTableSendToQueue : SendToQueueBase, IAlterTableManager,IAsyncDisposable
    {
       
        public AlterTableSendToQueue(IConfiguration config):base(config) 
        {
            _client = new ServiceBusClient(_config.GetValue("altertableconnection", "archive-service-dev.servicebus.windows.net"), new DefaultAzureCredential());
            // create a sender for the queue 
            _sender = _client.CreateSender(_config.GetValue("altertablequeue", "altertable"));
        }

        public async Task HandleAlterTable(EntityMetadata table, ILogger log)
        {
            _config.GetValue("altertableconnection", "");
            var message = JsonConvert.SerializeObject(table);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);

            // send the message
            await _sender.SendMessageAsync(serviceBusMessage);
            log.LogInformation($"{table.LogicalName} sent");
        }
    }
}
