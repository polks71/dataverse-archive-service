using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Models
{
    public class ArchiveTableSettings
    {
        public string DisplayName { get; set; } = string.Empty;
	    public string LogicalName { get; set; } = string.Empty;
        public bool ArchiveEnabled { get; set; } = true;
        public string ArchiveEnabledSqlValue
        { get
            {
                return ArchiveEnabled ? "1" : "0";
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
	    public string LastChangeTrackingToken { get; set; } = string.Empty;
    }
}
