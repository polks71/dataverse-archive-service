using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Extensions.AttributeExentions
{
    internal static class AttributeList
    {
        /// <summary>
        /// Get the logical name of the attribute marked as IsPrimary
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static string GetPrimaryAttributeName(this List<AttributeMetadata> attributes)
        {
            return attributes.First(a => a.IsPrimaryId.HasValue && a.IsPrimaryId.Value).LogicalName;
        }
    }
}
