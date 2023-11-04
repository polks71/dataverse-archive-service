using ArchiveService.Shared.Helpers;
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.AlterTable
{
    public class AlterTableProcessor : ManagerBase, IAlterTableManager
    {
        public AlterTableProcessor(IOptions<SqlSettings> dataverseSettings, IConfiguration config, IMemoryCache memoryCache) :
            base(dataverseSettings, config, memoryCache)
        {

        }

        public async Task HandleAlterTable(EntityMetadata table, ILogger log)
        {
            await UpdateTable(table, log);
        }

        private async Task UpdateTable(EntityMetadata table, ILogger log)
        {
            List<InformationSchemaColumns> columns = await GetTableColumnsAsync(table);
            var attributes = DataverseAttributeHelper.GetDataverseAttributes(table.LogicalName, log, GetServiceClient(log), true);
            var attributesToCreate = new List<AttributeMetadata>();
            foreach (var attribute in attributes)
            {
                //if the logical name is a column in the SQL table
                if (columns.Any(a => a.ColumnName == attribute.LogicalName))
                {
                    //any proved that one exists already
                    log.LogInformation($"Column {table.LogicalName}.{attribute.LogicalName} exists");
                    var column = columns.First(a => a.ColumnName == attribute.LogicalName);
                    var datatypestatus = ValidateDataType(attribute, column, log);
                    if (datatypestatus == DataTypeStatus.NotTracked)
                    {
                        log.LogInformation($"{attribute.LogicalName} {attribute.AttributeType} not tracked");
                        continue;
                    }
                    else
                    if (datatypestatus == DataTypeStatus.Same)
                    {
                        await ValidateColumnProperties(table, log, attribute, column);
                    }
                    else
                    {
                        //this will occur if someone drops a column in dataverse and re-adds it with the same name.
                        //in this case rename the existing column
                        //add the attribute to the list of attributes to create
                        //do not copy the data, can't guarantee new data type will allow it
                        var newcolumnName = $"{column.ColumnName}_old0";
                        if (columns.Any(c => newcolumnName == c.ColumnName))
                        {
                            //make sure we have a unique column name
                            var i = 1;
                            while (true)
                            {
                                newcolumnName = $"{column.ColumnName}_old{i}";
                                if (!columns.Any(c => newcolumnName == c.ColumnName))
                                    break;
                                i++;
                            }
                        }
                        var sqlcmd = $"EXEC sp_rename '{_archiveServiceSettings.SchemaName}.{table.LogicalName}.{column.ColumnName}', '{newcolumnName}', 'COLUMN'";
                        using (var sqlconn = GetSqlConnection())
                        using (var sqlCom = sqlconn.CreateCommand())
                        {
                            sqlCom.CommandText = sqlcmd;
                            await sqlCom.ExecuteNonQueryAsync();
                        }
                        attributesToCreate.Add(attribute);
                    }
                    continue;
                }
                else
                {
                    log.LogInformation($"Column {table.LogicalName}.{attribute.LogicalName} does not exist");
                    if (attribute.AttributeType.HasValue && attribute.AttributeType.Value != AttributeTypeCode.CalendarRules)
                        attributesToCreate.Add(attribute);
                    continue;

                }
            }
            if (attributesToCreate.Count > 0)
                await CreateNewColumns(table, log, attributesToCreate);
            else
                log.LogInformation("0 Attributes to create");
        }

        private async Task ValidateColumnProperties(EntityMetadata table, ILogger log, AttributeMetadata attribute, InformationSchemaColumns column)
        {
            //the rest of the data types are created with max values for properties. Decimal is the only one that is created with values from the dataverse attribute.
            switch (attribute.AttributeType)
            {
                case AttributeTypeCode.Decimal:
                    var decimalattr = (DecimalAttributeMetadata)attribute;
                    var length = decimalattr.MaxValue.HasValue ? decimalattr.MaxValue.ToString().Length : 12;
                    if (length < decimalattr.Precision)
                        length = decimalattr.Precision.Value;
                    if (length != column.NumericPrecisionInt || decimalattr.Precision.Value != column.NumericPrecisionRadixInt)
                    {
                        var attributeline = $"alter table {_archiveServiceSettings.SchemaName}{table.LogicalName} alter column {attribute.LogicalName} decimal({length},{decimalattr.Precision})";
                        using (var conn = GetSqlConnection())
                        using (var comm = conn.CreateCommand())
                        {
                            comm.CommandText = attributeline;
                            await comm.ExecuteNonQueryAsync();
                        }
                    }
                    break;
                default:
                    log.LogTrace("No update necessary");
                    break;
            }
        }

        private DataTypeStatus ValidateDataType(AttributeMetadata attribute, InformationSchemaColumns column, ILogger log)
        {
            switch (attribute.AttributeType)
            {

                case AttributeTypeCode.Boolean:
                    return column.DataType == "bit" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Virtual:
                case AttributeTypeCode.String:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.EntityName:
                    return column.DataType == "nvarchar" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.DateTime:
                    return column.DataType == "datetime" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                    return column.DataType == "decimal" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.Double:
                    return column.DataType == "float" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return column.DataType == "int" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Uniqueidentifier:
                    return column.DataType == "uniqueidentifier" ? DataTypeStatus.Same : DataTypeStatus.Different;
                case AttributeTypeCode.PartyList:
                    //todo: handle party list
                    return DataTypeStatus.NotTracked;
                case AttributeTypeCode.BigInt:
                    return column.DataType == "bigint" ? DataTypeStatus.Same : DataTypeStatus.Different;
                default:
                    return DataTypeStatus.NotTracked;
            }
        }

        private async Task CreateNewColumns(EntityMetadata table, ILogger log, List<AttributeMetadata> attributesToCreate)
        {
            log.LogInformation($"{attributesToCreate.Count} Attributes To Create");
            using (var conn = GetSqlConnection())
            using (var comm = conn.CreateCommand())
            {
                foreach (var attr in attributesToCreate)
                {

                    var tempattrList = new List<AttributeMetadata>() { attr };
                    var fieldToCreate = Helpers.FieldDefinitionHelper.GenerateFieldDefinitions(table, tempattrList, log).Replace(",", "");
                    if (string.IsNullOrEmpty(fieldToCreate))
                    {
                        //this prevent attempting to execute an alter statemnt with no fields.
                        log.LogInformation($"{table.LogicalName} No fields to create");
                        continue;
                    }
                    var alterTableStatement = $"alter table {_archiveServiceSettings.SchemaName}.{table.LogicalName} add {fieldToCreate}";

                    comm.CommandText = alterTableStatement;
                    await comm.ExecuteNonQueryAsync();
                    log.LogInformation($"Create column {attr.LogicalName}");

                }
            }
        }

        private async Task<List<InformationSchemaColumns>> GetTableColumnsAsync(EntityMetadata table)
        {
            using (var conn = GetSqlConnection())            
            {
                return await GetTableColumnsHelper.GetTableColumnsAsync(table.LogicalName, _archiveServiceSettings.SchemaName, conn);
            }
        }
    }
}
