using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.DailyArchive
{
    public interface IDailyArchiveManager
    {
        public Task HandleDailyArchive(ILogger log);
    }
}
