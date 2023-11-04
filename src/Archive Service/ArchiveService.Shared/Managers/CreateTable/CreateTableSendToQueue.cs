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

namespace ArchiveService.Shared.Managers.CreateTable
{
    public class CreateTableSendToQueue : SendToQueueBase, ICreateTableManager, IAsyncDisposable
    {
        
        //ServiceBusClient
        public CreateTableSendToQueue(IConfiguration config):base(config) 
        {
            _client = new ServiceBusClient(_config.GetValue("createtableconnection", "archive-service-dev.servicebus.windows.net"), new DefaultAzureCredential());
            _sender = _client.CreateSender(_config.GetValue("createtablequeue", "createtable"));
        }


        public async Task HandleCreateTable(EntityMetadata table, ILogger log)
        {
            var message = JsonConvert.SerializeObject(table);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);

            // send the message
            await _sender.SendMessageAsync(serviceBusMessage);
            log.LogInformation($"{table.LogicalName} sent");
        }
    }
}
