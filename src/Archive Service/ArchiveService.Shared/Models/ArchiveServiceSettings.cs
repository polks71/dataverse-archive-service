using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Models
{
    public class ArchiveServiceSettings
    {
        public bool HistoricalArchive { get; set; } = true;
        public string HistoricalArchiveSqlValue
        {
            get
            {
                return HistoricalArchive ? "1" : "0";
            }
        }
        public bool ChangeLogEnabled { get; set; } = false;
        public string ChangeLogEnabledSqlValue
        {
            get
            {
                return ChangeLogEnabled ? "1" : "0";
            }
        }
        
        public string DataverseUrl { get; set; } = string.Empty;
        public string SchemaName { get; set; } = "dv";
        public Guid? ServiceEndPointId { get; set; }
    }
}
