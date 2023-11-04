using ArchiveService.Shared.Extensions.AttributeExentions;
using ArchiveService.Shared.Extensions.AttributeListExtensions;
using ArchiveService.Shared.Helpers;
using ArchiveService.Shared.Managers.AlterTable;
using ArchiveService.Shared.Managers.CreateTable;
using ArchiveService.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.ChangeLog
{
    public class ChangeLogManager : ManagerBase, IChangeLogManager
    {
        public ChangeLogManager(IOptions<SqlSettings> dataverseSettings, IConfiguration config, IMemoryCache memoryCache) :
            base(dataverseSettings, config, memoryCache)
        { }
        public async Task RunAsync(RemoteExecutionContext context, ILogger log)
        {
            if (!context.InputParameters.ContainsKey("Target"))
            {
                //without a Target there we must question if it is a Create, Update, or Delete. Target is included with create, update, delete messages
                log.LogCritical("Context does not contain Target");
                return;
            }
            List<AttributeMetadata> attributes = DataverseAttributeHelper.GetDataverseAttributes(context.PrimaryEntityName, log, GetServiceClient(log));
            var sqlCommand = @$"insert into {_archiveServiceSettings.SchemaName}log.{context.PrimaryEntityName} 
(
	[{attributes.GetPrimaryAttributeName()}] = @primaryAttrValue,
	[messagename] = @messagenamevalue,
	[ChangeDatetime] = @changedatetimevalue,
	[UserId] = @useridvalue,
	[jsondata] = @jsondata

)";
            var jsondata = "";
            if (context.MessageName.ToLower() != "delete")
            {
                var target = context.InputParameters["Target"];
                //confirm the target is an Entity
                if (target.GetType() == typeof(Entity))
                {
                    var entity = (Entity)target;
                    var jobject = entity.Attributes.GetJObject(attributes);
                    jsondata = jobject.ToString(Formatting.Indented);
                }
            }
           
            //var attributesJson = JsonConvert.SerializeObject()
            using (var conn = GetSqlConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlCommand;
                cmd.Parameters.AddWithValue("@primaryAttrValue", context.PrimaryEntityId);
                cmd.Parameters.AddWithValue("@messagenamevalue", context.MessageName);
                cmd.Parameters.AddWithValue("@changedatetimevalue", context.OperationCreatedOn);
                cmd.Parameters.AddWithValue("@useridvalue", context.UserId);
                cmd.Parameters.AddWithValue("@jsondata", jsondata);

                await cmd.ExecuteNonQueryAsync();
            }

            /*
             * insert into dvlog.aaduser 
(
	[aaduserid] = @aaduseridvalue,
	[messagename] = @messagenamevalue,
	[ChangeDatetime] = @changedatetimevalue,
	[UserId] = @useridvalue,
	[jsondata] = @jsaondata

)
             */

        }
    }
}
