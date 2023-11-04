using System;
using ArchiveService.Shared.Managers.DailyArchive;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ArchiveService.Publish
{
    public class DailyArchive
    {
        private readonly IDailyArchiveManager _dailyArchiveManager;
        public DailyArchive(IDailyArchiveManager dailyArchiveManager)
        {
            _dailyArchiveManager = dailyArchiveManager;
        }
        [FunctionName("DailyArchive")]
        public void Run([TimerTrigger("%dailyArchiveTimerTrigger%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Daily Archive started at: {DateTime.Now}");
            _dailyArchiveManager.HandleDailyArchive(log);
        }
    }
}

