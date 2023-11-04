using ArchiveService.Shared.Managers.DailyArchive;
using ArchiveService.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ArchiveService.Shared.Helpers;
using Microsoft.Xrm.Sdk.Metadata;
using ArchiveService.Shared.Extensions.AttributeExentions;
using ArchiveService.Shared.Extensions.SqlCommandExtensions;
using ArchiveService.Shared.Managers.AlterTable;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ArchiveService.Shared.Managers.TableDailyArchive
{
    public class TableDailyArchiveManager : ManagerBase, ITableDailyArchiveManager
    {
        private readonly IAlterTableManager _alterTableManager;
        public TableDailyArchiveManager(IOptions<SqlSettings> dataverseSettings, IConfiguration config, IAlterTableManager alterTableManager, IMemoryCache memoryCache) :
    base(dataverseSettings, config, memoryCache)
        {
            _alterTableManager = alterTableManager;
        }

        public async Task HandleTableDailyArchive(ILogger log, Models.TableDailyArchive tableDailyArchive)
        {
            string token;



            try
            {
                if (string.IsNullOrEmpty(tableDailyArchive.LastChangeTrackingToken))
                    log.LogInformation($"Initial Sync of {tableDailyArchive.LogicalName}");
                else
                    log.LogInformation($"Update of {tableDailyArchive.LogicalName}");

                // Retrieve records by using Change Tracking feature.
                RetrieveEntityChangesRequest request = new RetrieveEntityChangesRequest();
                request.EntityName = tableDailyArchive.LogicalName.ToLower();
                request.Columns = new ColumnSet(true);
                if (!string.IsNullOrEmpty(tableDailyArchive.LastChangeTrackingToken))
                    request.DataVersion = tableDailyArchive.LastChangeTrackingToken;
                request.PageInfo = new PagingInfo() { Count = 5000, PageNumber = 1, ReturnTotalRecordCount = false };                
                using (var conn = GetSqlConnection())
                {
                    var tableColumns = await GetTableColumnsHelper.GetTableColumnsAsync(tableDailyArchive.LogicalName, _archiveServiceSettings.SchemaName, conn);
                    var primaryKeyColumn = await GetTableColumnsHelper.GetPrimaryKeyColumn(tableDailyArchive.LogicalName, _archiveServiceSettings.SchemaName, conn);
                    var dataverseAttributes = DataverseAttributeHelper.GetDataverseAttributes(tableDailyArchive.LogicalName, log, GetServiceClient(log), true);
                    log.LogInformation($"{tableDailyArchive.LogicalName} retrieved table, primary key and dataverse attribues");
                    while (true)
                    {
                        RetrieveEntityChangesResponse response = (RetrieveEntityChangesResponse)await GetServiceClient(log).ExecuteAsync(request);
                        log.LogInformation($"{tableDailyArchive.LogicalName} executed retrive changes");
                        await NewOrUpdatedItems(log, tableDailyArchive, conn, tableColumns, primaryKeyColumn, dataverseAttributes, response);
                        log.LogInformation($"{tableDailyArchive.LogicalName} processed new items");
                        await DeletedOrRemovedItems(tableDailyArchive, conn, primaryKeyColumn, response);
                        log.LogInformation($"{tableDailyArchive.LogicalName} processed deleted items");

                        if (!response.EntityChanges.MoreRecords)
                        {
                            // Store token for later query
                            token = response.EntityChanges.DataToken;
                            break;

                        }
                        // Increment the page number to retrieve the next page.
                        request.PageInfo.PageNumber++;
                        // Set the paging cookie to the paging cookie returned from current results.
                        request.PageInfo.PagingCookie = response.EntityChanges.PagingCookie;
                    }
                    log.LogInformation($"{tableDailyArchive.LogicalName} updating change token");
                    var updateSql = @"  update dbo.ArchiveTableSettings set
  LastChangeTrackingToken = @changeToken
  where LogicalName = @logicalName";

                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = updateSql;
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@changeToken", token);
                        comm.Parameters.AddWithValue("@logicalName", tableDailyArchive.LogicalName);
                        await comm.ExecuteNonQueryAsync();
                    }
                    log.LogInformation($"{tableDailyArchive.LogicalName} updated change token");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"{tableDailyArchive} error");
                throw;
            }
        }

        private async Task DeletedOrRemovedItems(Models.TableDailyArchive tableDailyArchive, SqlConnection conn, string primaryKeyColumn, RetrieveEntityChangesResponse response)
        {
            var removedItems = response.EntityChanges.Changes
                .Where(c => c.Type == ChangeType.RemoveOrDeleted)
                .Select(x => (x as RemovedOrDeletedItem).RemovedItem)
                .ToList();

            foreach (var removedItem in removedItems)
            {
                var sql = @$"if ((select count(*) from {_archiveServiceSettings.SchemaName}.{tableDailyArchive.LogicalName} where {primaryKeyColumn} = @primarykeyValue) > 0)
begin
	update {_archiveServiceSettings.SchemaName}.{tableDailyArchive.LogicalName} set deleted = 1, deletedatetime = @deletedwhen where {primaryKeyColumn} = @primarykeyValue
end";
                using (var com = conn.CreateCommand())
                {
                    com.CommandText = sql;
                    com.Parameters.AddWithValue("@primarykeyValue", removedItem.Id);
                    if (removedItem.KeyAttributes.ContainsKey("deletetime"))
                    {
                        var deleteTimeValue = (DateTime)removedItem.KeyAttributes["deletetime"];
                        com.Parameters.AddWithValue("@deletedwhen", deleteTimeValue);
                    }
                    else
                    {
                        com.Parameters.Add("@deletedwhen", SqlDbType.DateTime2);
                        com.Parameters["@deletedwhen"].Value = DBNull.Value;
                    }
                    await com.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task NewOrUpdatedItems(ILogger log, Models.TableDailyArchive tableDailyArchive, SqlConnection conn, List<InformationSchemaColumns> tableColumns, string primaryKeyColumn, List<AttributeMetadata> dataverseAttributes, RetrieveEntityChangesResponse response)
        {
            var newordupdateItems = response.EntityChanges.Changes
                                    .Where(c => c.Type == ChangeType.NewOrUpdated)
                                    .Select(x => (x as NewOrUpdatedItem).NewOrUpdatedEntity)
                                    .ToList();

            foreach (var item in newordupdateItems)
            {
                //this line is to prevent columns we didn't capture at table create (see DataverseAttributeHelper.GetDataverseAttributes) from triggering the alter table
                var attributes = item.Attributes.ToList().Where(a => dataverseAttributes.Any(da => da.LogicalName == a.Key)).ToList();
                if (attributes.Any(a => !tableColumns.Any(c => c.ColumnName == a.Key)))
                {
                    log.LogWarning("Dataverse has attributes not in the table, updating table");
                    var metadata = GetServiceClient(log).GetEntityMetadata(item.LogicalName);
                    await _alterTableManager.HandleAlterTable(metadata, log);
                    //after rechcking the table refresh the list of columns
                    tableColumns = await GetTableColumnsHelper.GetTableColumnsAsync(tableDailyArchive.LogicalName, _archiveServiceSettings.SchemaName, conn);
                }
                using (var comm = conn.CreateCommand())
                {
                    //build an IF statement
                    //if a row exists for the primary key update it
                    //if not create a new one
                    //build the update statement and insert statment using the same parameter names, append parameters to the command object at the end
                    var mergestatement = new StringBuilder();
                    mergestatement.AppendLine($"if ((select count(*) from {_archiveServiceSettings.SchemaName}.{tableDailyArchive.LogicalName} where {primaryKeyColumn} = @primarykeyValue) > 0)");
                    mergestatement.AppendLine("begin");
                    mergestatement.AppendLine($"UPDATE {_archiveServiceSettings.SchemaName}.{tableDailyArchive.LogicalName} SET");
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        var metadata = dataverseAttributes.First(a => a.LogicalName == attr.Key);
                        var addTypeLine = !attributes.Any(a => a.Key.ToLower() == $"{attr.Key.ToLower()}type") &&
                                                ((metadata.AttributeType == AttributeTypeCode.Customer || metadata.AttributeType == AttributeTypeCode.Lookup));
                        if (addTypeLine)
                            mergestatement.AppendLine($"[{attr.Key}type] = @{attr.Key}typeValue,");

                        mergestatement.AppendLine($"{attr.MapUpdateLine()},");
                        mergestatement.AppendLine("jsondata = @jsondataValue");
                    }
                    mergestatement.AppendLine($"WHERE {primaryKeyColumn} = @primarykeyValue");
                    mergestatement.AppendLine("end");
                    mergestatement.AppendLine("else");
                    mergestatement.AppendLine("begin");
                    mergestatement.AppendLine($"INSERT INTO {_archiveServiceSettings.SchemaName}.{tableDailyArchive.LogicalName} (");
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        var metadata = dataverseAttributes.First(a => a.LogicalName == attr.Key);
                        var addTypeLine = !attributes.Any(a => a.Key.ToLower() == $"{attr.Key.ToLower()}type") &&
                                                ((metadata.AttributeType == AttributeTypeCode.Customer || metadata.AttributeType == AttributeTypeCode.Lookup));
                        if (addTypeLine)
                            mergestatement.AppendLine($"[{attr.Key}type],");
                        mergestatement.AppendLine($"[{attr.Key}],");
                        mergestatement.AppendLine("jsondata");
                    }
                    mergestatement.AppendLine(") VALUES (");
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        var metadata = dataverseAttributes.First(a => a.LogicalName == attr.Key);
                        var addTypeLine = !attributes.Any(a => a.Key.ToLower() == $"{attr.Key.ToLower()}type") &&
                                                ((metadata.AttributeType == AttributeTypeCode.Customer || metadata.AttributeType == AttributeTypeCode.Lookup));
                        if (addTypeLine)
                            mergestatement.AppendLine($"@{attr.Key}typeValue,");

                        
                        mergestatement.AppendLine($"{attr.GetParamName()},");
                        mergestatement.AppendLine("@jsondataValue");
                    }
                    mergestatement.AppendLine(")");
                    mergestatement.AppendLine("end");

                    if (!item.Attributes.ContainsKey(primaryKeyColumn))
                        throw new Exception($"Unable to find primary key column {primaryKeyColumn}");


                    var primaryKeyValue = item.GetAttributeValue<Guid>(primaryKeyColumn);
                    comm.CommandText = mergestatement.ToString();
                    comm.Parameters.AddWithValue("@primarykeyValue", primaryKeyValue);
                    //add params for each attribute
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        comm.AddParamFromXrmAttribute(attr, dataverseAttributes.First(a => a.LogicalName == attr.Key),
                            attributes.Any(a => a.Key.ToLower() == $"{attr.Key.ToLower()}type"));
                    }
                    comm.Parameters.AddWithValue("@jsondataValue", JsonConvert.SerializeObject(item.Attributes, Formatting.Indented));
                    await comm.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
