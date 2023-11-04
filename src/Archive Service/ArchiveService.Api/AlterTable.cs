using System.Threading.Tasks;
using ArchiveService.Shared.Managers.AlterTable;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;

namespace ArchiveService.Dataverse.Api
{
    public class AlterTable
    {
        private readonly IAlterTableManager _alterTableManager;
        public AlterTable(IAlterTableManager alterTableManager)
        {
            _alterTableManager = alterTableManager;
        }
        [FunctionName("AlterTable")]
        public async Task Run([ServiceBusTrigger("%altertable%", Connection = "altertableconnection")] string message, ILogger log)
        {
            log.LogInformation($"MessageReceived: {message}");
            var context = JsonConvert.DeserializeObject<EntityMetadata>(message);
            await _alterTableManager.HandleAlterTable(context, log);
        }
    }
}
