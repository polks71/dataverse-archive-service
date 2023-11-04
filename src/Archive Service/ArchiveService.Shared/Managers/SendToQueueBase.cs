using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers
{
    public abstract class SendToQueueBase
    {
        protected readonly IConfiguration _config;
        protected ServiceBusClient _client;
        protected ServiceBusSender _sender;

        protected SendToQueueBase(IConfiguration config) 
        {
            _config = config;
        }

        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                await _client.DisposeAsync();
            }
            if (_sender != null)
            {
                await _sender.DisposeAsync();
            }
        }
    }
}
