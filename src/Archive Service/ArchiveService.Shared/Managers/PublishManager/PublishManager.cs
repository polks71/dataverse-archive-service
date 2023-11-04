using ArchiveService.Shared.Managers.AlterTable;
using ArchiveService.Shared.Managers.CreateTable;
using ArchiveService.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Data;
using System.Text;

namespace ArchiveService.Shared.Managers.PublishManager
{

    public class PublishManager : ManagerBase, IPublishManager
    {
        private readonly List<string> _tables = new List<string>();
        private readonly IAlterTableManager _alterTableHelper;
        private readonly ICreateTableManager _createTableHelper;
        public PublishManager(IOptions<SqlSettings> dataverseSettings, IConfiguration config, IAlterTableManager alterTableHelper, ICreateTableManager createTableHelper, IMemoryCache memoryCache) :
            base(dataverseSettings, config, memoryCache)
        {
            _alterTableHelper = alterTableHelper;
            _createTableHelper = createTableHelper;
        }

        public async Task RunAsync(RemoteExecutionContext context, ILogger log)
        {
            log.LogInformation("Start Publish");
            try
            {
                using (var conn = GetSqlConnection())
                using (var command = conn.CreateCommand())
                {
                    log.LogInformation("Sqlconnection and Command");
                    command.CommandText = $@"SELECT table_name FROM information_schema.tables where TABLE_SCHEMA = '{_archiveServiceSettings.SchemaName}';";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        log.LogInformation("Reading tables");
                        while (await reader.ReadAsync())
                        {
                            _tables.Add(await reader.GetFieldValueAsync<string>(0));
                        }
                    }
                }
                log.LogInformation($"Retrieve {_tables.Count} tables");
                //only archive tables with change tracking enabled
                var tables = GetServiceClient(log).GetAllEntityMetadata(true);
                foreach (var table in tables)
                {
                    if (_tables.Any(s => s == table.LogicalName))
                    {
                        log.LogInformation($"Table {table.LogicalName} already exists");
                        await _alterTableHelper.HandleAlterTable(table, log);
                    }
                    else
                    {
                        log.LogInformation($"Table {table.LogicalName} does not exist");
                        await _createTableHelper.HandleCreateTable(table, log);
                    }

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error publishing");
                throw;
            }
        }
    }
}
