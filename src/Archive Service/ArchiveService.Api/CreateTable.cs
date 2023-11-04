
using ArchiveService.Shared.Managers.CreateTable;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ArchiveService.Dataverse.Api
{
    public class CreateTable
    {

        private readonly ICreateTableManager _createTableManager;
        public CreateTable(ICreateTableManager createTableManager)
        {
            _createTableManager = createTableManager;
        }
        [FunctionName("CreateTable")]
        public async Task Run([ServiceBusTrigger("%createtable%", Connection = "createtableconnection")] string message, ILogger log)
        {
            log.LogInformation($"MessageReceived: {message}");
            var context = JsonConvert.DeserializeObject<EntityMetadata>(message);
            await _createTableManager.HandleCreateTable(context, log);
        }
    }
}
