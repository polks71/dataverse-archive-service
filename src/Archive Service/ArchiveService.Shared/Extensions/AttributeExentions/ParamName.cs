using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Extensions.AttributeExentions
{
    internal static class ParamName
    {
        public static string GetParamName(this KeyValuePair<string, object> attr)
        {
            return $"@{attr.Key}Value";
        }
    }
}
