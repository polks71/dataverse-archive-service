using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Rest;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Helpers
{
    public static class DataverseAttributeHelper
    {
        private static object _lockObject = new object();
        private static ConcurrentDictionary<string, List<AttributeMetadata>> _tableAttributes = new ConcurrentDictionary<string, List<AttributeMetadata>>();
        public static List<AttributeMetadata> GetDataverseAttributes(string table, ILogger log, ServiceClient sericeClient, bool forceupdate = false)
        {
            if (!forceupdate && _tableAttributes.ContainsKey(table))
            {
                log.LogInformation("Retrieving Attributes from Cache");
                return _tableAttributes[table];
            }
            else
            {
                lock(_lockObject)
                {
                    //check again in case another thread created it
                    if (!forceupdate && _tableAttributes.ContainsKey(table))
                    {
                        return _tableAttributes[table];
                    }
                    else
                    {
                        log.LogInformation("Retrieving Attributes for Cache");
                        var attributes = sericeClient.GetAllAttributesForEntity(table).OrderBy(a => a.LogicalName).ToList()
                            .Where(a => a.AttributeType.HasValue && a.AttributeType.Value != AttributeTypeCode.CalendarRules
                                    && a.AttributeType != AttributeTypeCode.ManagedProperty).ToList();
                        _tableAttributes[table] = attributes;
                        log.LogInformation("Retrieved Attributes and Cached");
                        return attributes;
                    }

                }
            }
        }

        public static bool TableTrackChangesEnabled(ServiceClient serviceClient, string logicalName)
        {
            var metadata = serviceClient.GetEntityMetadata(logicalName, Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity);
            if (metadata == null)
            {
                throw new Exception($"Table {logicalName} not found");
            }
            return metadata.ChangeTrackingEnabled.HasValue ? metadata.ChangeTrackingEnabled.Value : false;
        }
    }
}
