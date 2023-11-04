using ArchiveService.Shared.Managers.AlterTable;
using ArchiveService.Shared.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Extensions.AttributeExentions
{
    internal static class UpdateLine
    {
        public static string MapUpdateLine(this KeyValuePair<string, object> attr)
        {

            return $"[{attr.Key}] = {attr.GetParamName()}";
        }


    }
}
