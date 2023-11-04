using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.TableDailyArchive
{
    public interface ITableDailyArchiveManager
    {
        public Task HandleTableDailyArchive(ILogger log, Models.TableDailyArchive tableDailyArchive);
    }
}
