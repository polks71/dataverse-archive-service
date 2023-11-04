using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.ChangeLog
{
    public interface IChangeLogManager
    {
        Task RunAsync(RemoteExecutionContext context, ILogger log);
    }
}
