using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ArchiveService.Shared.Extensions.AttributeExentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Extensions.AttributeListExtensions
{
    internal static class ChangeLogJObject
    {
        public static JObject GetJObject(this AttributeCollection values, List<AttributeMetadata> attributes)
        {
            var jobject = new JObject();
            foreach (var attr in values)
            {
                var attributeMetadata = attributes.First(a => a.LogicalName == attr.Key);
                switch (attributeMetadata.AttributeType)
                {
                    case AttributeTypeCode.PartyList:
                        var s = JsonConvert.SerializeObject(((EntityCollection)attr.Value).Entities);
                        jobject.Add(attr.Key, s);
                        break;
                    case AttributeTypeCode.Boolean:
                        jobject.Add(attr.Key, (bool)attr.Value);
                        break;
                    case AttributeTypeCode.Customer:
                        jobject.Add(attr.Key, (string)attr.Value);
                        break;
                    case AttributeTypeCode.Virtual:
                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.EntityName:
                        jobject.Add(attr.Key, (string)attr.Value);
                        break;
                    case AttributeTypeCode.DateTime:
                        jobject.Add(attr.Key, (DateTime)attr.Value);
                        break;
                    case AttributeTypeCode.Decimal:
                        jobject.Add(attr.Key, (decimal)attr.Value);
                        break;
                    case AttributeTypeCode.Money:
                        jobject.Add(attr.Key, ((Money)attr.Value).Value);
                        break;
                    case AttributeTypeCode.Double:
                        jobject.Add(attr.Key, (float)attr.Value);
                        break;
                    case AttributeTypeCode.Integer:
                        jobject.Add(attr.Key, (int)attr.Value);
                        break;
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        jobject.Add(attr.Key, ((OptionSetValue)attr.Value).Value);
                        break;
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        jobject.Add(attr.Key, ((EntityReference)attr.Value).Id);
                        break;
                    case AttributeTypeCode.Uniqueidentifier:
                        jobject.Add(attr.Key, (Guid)attr.Value);
                        break;

                    case AttributeTypeCode.BigInt:
                        jobject.Add(attr.Key, (long)attr.Value);
                        break;
                    default:
                        throw new Exception($"Unknown Attribute DataType {attributeMetadata.AttributeType} for {attributeMetadata.LogicalName}");
                } 
            }
            return jobject;
        }
    }
}
