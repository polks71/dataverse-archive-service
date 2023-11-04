using ArchiveService.Shared.Managers;
using ArchiveService.Shared.Managers.PublishManager;
using ArchiveService.Shared.Managers.TableDailyArchive;
using ArchiveService.Shared.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ArchiveService.Dataverse.Api
{
    public class DailyArchiveTable
    {

        private readonly ITableDailyArchiveManager _publishManager;
        public DailyArchiveTable(ITableDailyArchiveManager dailyArchiveManager)
        {
            _publishManager = dailyArchiveManager;
        }
        [FunctionName("TableDailyArchive")]
        public async Task Run([ServiceBusTrigger("%archivetablequeue%", Connection = "archivetableconnection")] string message, ILogger log)
        {
            log.LogInformation($"MessageReceived: {message}");
            var tableDailyArchive = JsonConvert.DeserializeObject<TableDailyArchive>(message);
            await _publishManager.HandleTableDailyArchive(log, tableDailyArchive);
        }
    }
}
