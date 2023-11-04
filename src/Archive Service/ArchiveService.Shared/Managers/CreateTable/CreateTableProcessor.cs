using ArchiveService.Shared.Extensions.AttributeExentions;
using ArchiveService.Shared.Extensions.AttributeListExtensions;
using ArchiveService.Shared.Helpers;
using ArchiveService.Shared.Managers.ServiceEndpointSteps;
using ArchiveService.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.CreateTable
{
    public class CreateTableProcessor : ManagerBase, ICreateTableManager
    {
        public CreateTableProcessor(IOptions<SqlSettings> dataverseSettings, IConfiguration config, IMemoryCache memoryCache) :
    base(dataverseSettings, config, memoryCache)
        {

        }
        public async Task HandleCreateTable(EntityMetadata table, ILogger log)
        {
            await CreateTable(table, log);
        }

        private async Task CreateTable(EntityMetadata table, ILogger log)
        {
            StringBuilder createTableStatement = GenerateTableAndLogTableStatement(table, log);
            

            using (var conn = GetSqlConnection())
            using (var comm = conn.CreateCommand())
            {
                comm.CommandText = createTableStatement.ToString();
                await comm.ExecuteNonQueryAsync();
            }

            if (_archiveServiceSettings.ChangeLogEnabled && _archiveServiceSettings.ServiceEndPointId.HasValue)
            {
                log.LogInformation("Setting up ServiceEndpointSteps");
                var stepmanager = new ServiceEndPointStepManager(GetServiceClient(log), log);
                stepmanager.AddOrRemoveServiceEndPointStep(table.LogicalName, _archiveServiceSettings.ServiceEndPointId.Value, _archiveServiceSettings.ChangeLogEnabled);
            }

            using (var conn = GetSqlConnection())
            using (var comm = conn.CreateCommand())
            {
                var displayname = table.DisplayName.LocalizedLabels.Count > 0 ? table.DisplayName.UserLocalizedLabel.Label.Replace("'", "").Replace("&", "") : table.LogicalName;
                //for daily archive to be enabled change tracking has to be turned on
                var dvchangeTrackingEnabled = table.ChangeTrackingEnabled.HasValue && table.ChangeTrackingEnabled.Value;
                var archiveEnabled = _archiveServiceSettings.HistoricalArchive && dvchangeTrackingEnabled;
                comm.CommandText =
$@"
declare @tablecount int
select @tablecount = count(*)   from dbo.ArchiveTableSettings where LogicalName = @tableName
if (@tablecount = 0)
begin
INSERT INTO [dbo].[ArchiveTableSettings]
           ([DisplayName]
           ,[LogicalName]
           ,[ArchiveEnabled]
           ,[ChangeLogEnabled])
     VALUES
           (@displayName
           ,@tableName
           ,@archiveEnabled
           ,@changeLogEnabled)
end
else
begin
    update dbo.ArchiveTableSettings 
    set ArchiveEnabled = @archiveEnabled
    where LogicalName = @tableName

end
";
                
                comm.Parameters.AddWithValue("@displayName", displayname);
                comm.Parameters.AddWithValue("@tableName", table.LogicalName);
                comm.Parameters.AddWithValue("@archiveEnabled", archiveEnabled);
                comm.Parameters.AddWithValue("@changeLogEnabled", _archiveServiceSettings.ChangeLogEnabled);
                await comm.ExecuteNonQueryAsync();
            }
            log.LogInformation($"Created Table {table.LogicalName}");
 
        }

        private StringBuilder GenerateTableAndLogTableStatement(EntityMetadata table, ILogger log)
        {
            log.LogInformation("Start GenerateTableAndLogTableStatement");
            List<AttributeMetadata> attributes = DataverseAttributeHelper.GetDataverseAttributes(table.LogicalName, log, GetServiceClient(log), true);
            log.LogInformation("Retrieved Attributes");
            var createTableStatement = new StringBuilder();
            createTableStatement.AppendLine($"create table {_archiveServiceSettings.SchemaName}.{table.LogicalName} (");
            createTableStatement.AppendLine($"rowid bigint identity(1,1),");
            createTableStatement.AppendLine($"deleted bit default 0,");
            createTableStatement.AppendLine($"deletedatetime datetime2 null,");
            createTableStatement.AppendLine($"jsondata nvarchar(max) null,");
            createTableStatement.AppendLine(Helpers.FieldDefinitionHelper.GenerateFieldDefinitions(table, attributes, log));
            createTableStatement.AppendLine($@")");//end create table
            createTableStatement.AppendLine($@"
CREATE UNIQUE CLUSTERED INDEX UCIX_{table.LogicalName}_id   
    ON {_archiveServiceSettings.SchemaName}.{table.LogicalName} (rowid);");

            if (attributes.Any(a => a.IsPrimaryId.HasValue && a.IsPrimaryId.Value))
            {
                var primaryField = attributes.GetPrimaryAttributeName();
                //create pk on source history table
                createTableStatement.AppendLine($@"ALTER TABLE {_archiveServiceSettings.SchemaName}.{table.LogicalName}
ADD CONSTRAINT PK_{table.LogicalName} PRIMARY KEY NONCLUSTERED ({primaryField}) WITH (FILLFACTOR = 70, PAD_INDEX = ON)");

                //create log table and indexes
                createTableStatement.AppendLine($"create table {_archiveServiceSettings.SchemaName}Log.{table.LogicalName} (");
                createTableStatement.AppendLine($"rowid bigint not null identity(1,1),");
                createTableStatement.AppendLine($"{primaryField} uniqueidentifier not null,");
                createTableStatement.AppendLine($"messagename varchar(100) null,");
                createTableStatement.AppendLine($"ChangeDateTime datetime,");
                createTableStatement.AppendLine($"UserId uniqueidentifier,");
                createTableStatement.AppendLine("jsondata nvarchar(max),");
                createTableStatement.AppendLine($@"CONSTRAINT [{table.LogicalName}LogFormattedAsJSON] CHECK (ISJSON(jsondata)=1))");
                createTableStatement.AppendLine($@"ALTER TABLE {_archiveServiceSettings.SchemaName}Log.{table.LogicalName}
ADD CONSTRAINT PK_{table.LogicalName} PRIMARY KEY (rowid) WITH (FILLFACTOR = 80, PAD_INDEX = ON)");
                createTableStatement.AppendLine($@"
CREATE INDEX IX_{table.LogicalName}Log_{primaryField}   
    ON {_archiveServiceSettings.SchemaName}Log.{table.LogicalName} ({primaryField}) WITH (FILLFACTOR = 70, PAD_INDEX = ON);");
            }

            return createTableStatement;
        }        
    }
}
