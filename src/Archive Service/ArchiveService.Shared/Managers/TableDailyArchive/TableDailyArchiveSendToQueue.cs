using ArchiveService.Shared.Managers.AlterTable;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.TableDailyArchive
{
    public class TableDailyArchiveSendToQueue : SendToQueueBase, ITableDailyArchiveManager, IAsyncDisposable
    {
        
        public TableDailyArchiveSendToQueue(IConfiguration config):base(config)
        {
            
            _client = new ServiceBusClient(_config.GetValue("archivetableconnection", "archive-service-dev.servicebus.windows.net"), new DefaultAzureCredential());
            // create a sender for the queue 
            _sender = _client.CreateSender(_config.GetValue("archivetablequeue", "archivetable"));
        }

        public async Task HandleTableDailyArchive(ILogger log, Models.TableDailyArchive tableDailyArchive)
        {
            var message = JsonConvert.SerializeObject(tableDailyArchive);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);

            // send the message
            await _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
