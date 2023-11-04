using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;

namespace ArchiveService.Shared.Managers.CreateTable
{
    public interface ICreateTableManager
    {
        public Task HandleCreateTable(EntityMetadata table, ILogger log);
    }
}
