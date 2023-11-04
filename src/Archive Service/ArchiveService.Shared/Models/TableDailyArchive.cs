using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Models
{
    public class TableDailyArchive
    {
        public string LogicalName { get; set; } = string.Empty;
        public string LastChangeTrackingToken { get; set; } = string.Empty;
        public int ArchiveTableId { get; set; } = 0;
    }
}
