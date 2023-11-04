using ArchiveService.Shared.Managers.CreateTable;
using ArchiveService.Shared.Managers.TableDailyArchive;
using ArchiveService.Shared.Models;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.DailyArchive
{
    /// <summary>
    /// Retrive the list of tables from SQL and send a message to the queue for processing.
    /// Requires two Configuration keys
    /// archivetableconnection - fully qualified namespace of the Azure Service Bus
    /// archivetablequeue - queue name of the archive processing queue
    /// </summary>
    public class DailyArchiveManager : ManagerBase, IDailyArchiveManager
    {
        private readonly ITableDailyArchiveManager _tableDailyArchiveManager;
        public DailyArchiveManager(IOptions<SqlSettings> dataverseSettings, IConfiguration config, ITableDailyArchiveManager tableDailyArchiveManager, IMemoryCache memoryCache) :
            base(dataverseSettings, config, memoryCache)
        {
            _tableDailyArchiveManager = tableDailyArchiveManager;

        }

        public async Task HandleDailyArchive(ILogger log)
        {
            var getArchiveTableSettingsSql = @"SELECT [Id]
      ,[DisplayName]
      ,[LogicalName]
      ,[ArchiveEnabled]
      ,[LastChangeTrackingToken]
  FROM [dbo].[ArchiveTableSettings]
  where ArchiveEnabled = 1 and LogicalName is not null";


            using (var conn = GetSqlConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = getArchiveTableSettingsSql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableArchive = new Models.TableDailyArchive
                        {
                            ArchiveTableId = reader.GetFieldValue<int>("Id"),
                            LogicalName = reader.GetFieldValue<string>("LogicalName"),
                            LastChangeTrackingToken = reader.IsDBNull("LastChangeTrackingToken") ? string.Empty : reader.GetFieldValue<string>("LastChangeTrackingToken")
                        };
                        var displayName = reader.IsDBNull("DisplayName") ? string.Empty : reader.GetFieldValue<string>("DisplayName");

                        await _tableDailyArchiveManager.HandleTableDailyArchive(log, tableArchive);

                        log.LogInformation($"{displayName} Sent");
                    }
                }
            }
        }
    }
}
