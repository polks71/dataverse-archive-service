using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;

namespace ArchiveService.Shared.Managers.AlterTable
{
    public interface IAlterTableManager
    {
        public Task HandleAlterTable(EntityMetadata table, ILogger log);
    }
}
