using ArchiveService.Shared.Extensions.AttributeExentions;
using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;

namespace ArchiveService.Shared.Extensions.SqlCommandExtensions
{
    internal static class AddAttributeParam
    {
        public static void AddParamFromXrmAttribute(this SqlCommand sqlCommand, KeyValuePair<string, object> attr, AttributeMetadata attributeMetadata, bool typeExists)
        {

            switch (attributeMetadata.AttributeType)
            {
                case AttributeTypeCode.PartyList:
                    var s = JsonConvert.SerializeObject(((EntityCollection)attr.Value).Entities);
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), s);
                    break;
                case AttributeTypeCode.Boolean:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (bool)attr.Value);
                    break;
                case AttributeTypeCode.Virtual:
                case AttributeTypeCode.String:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.EntityName:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (string)attr.Value);
                    break;
                case AttributeTypeCode.DateTime:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (DateTime)attr.Value);
                    break;
                case AttributeTypeCode.Decimal:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (decimal)attr.Value);
                    break;
                case AttributeTypeCode.Money:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), ((Money)attr.Value).Value);
                    break;
                case AttributeTypeCode.Double:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), attr.Value);
                    break;
                case AttributeTypeCode.Integer:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (int)attr.Value);
                    break;
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), ((OptionSetValue)attr.Value).Value);
                    break;
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), ((EntityReference)attr.Value).Id);
                    if (!typeExists)
                        sqlCommand.Parameters.AddWithValue($"@{attr.Key}typeValue", ((EntityReference)attr.Value).LogicalName);
                    break;
                case AttributeTypeCode.Owner:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), ((EntityReference)attr.Value).Id);
                    break;
                case AttributeTypeCode.Uniqueidentifier:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (Guid)attr.Value);
                    break;

                case AttributeTypeCode.BigInt:
                    sqlCommand.Parameters.AddWithValue(attr.GetParamName(), (long)attr.Value);
                    break;
                default:
                    throw new Exception($"Unknown Attribute DataType {attributeMetadata.AttributeType} for {attributeMetadata.LogicalName}");
            }
        }
    }
}
