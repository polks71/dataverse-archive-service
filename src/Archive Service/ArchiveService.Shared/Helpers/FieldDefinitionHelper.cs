using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Helpers
{
    internal static class FieldDefinitionHelper
    {
        public static string GenerateFieldDefinitions(EntityMetadata table, List<AttributeMetadata> attributes, ILogger log)
        {
            /*
             * create table dv.logicalname (
             * )
             */
            var fieldDefinitions = new StringBuilder();
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeType.HasValue)
                {
                    string attributeline = string.Empty;
                    var attributeName = $"[{attribute.LogicalName}]";
                    var attributeTypeLine = string.Empty;
                    switch (attribute.AttributeType)
                    {
                        case AttributeTypeCode.Boolean:
                            attributeline = $"{attributeName} bit null,";
                            break;
                        case AttributeTypeCode.Customer:
                            attributeline = $"{attributeName} uniqueidentifier null,";
                            //ensure there isn't a type attribute already, owner and parent columns have a type field
                            if (!attributes.Any(a => a.LogicalName.ToLower() == $"{attribute.LogicalName}type"))
                                attributeTypeLine = $"[{attribute.LogicalName}type] nvarchar(255) null,";
                            break;
                        case AttributeTypeCode.DateTime:
                            var dateattr = (DateTimeAttributeMetadata)attribute;
                            attributeline = $"{attributeName} datetime null,";
                            break;
                        case AttributeTypeCode.Decimal:
                            var decimalattr = (DecimalAttributeMetadata)attribute;
                            var length = decimalattr.MaxValue.HasValue ? decimalattr.MaxValue.ToString().Length : 12;
                            if (length < decimalattr.Precision)
                                length = decimalattr.Precision.Value;
                            attributeline = $"{attributeName} decimal({length+decimalattr.Precision},{decimalattr.Precision}) null,";
                            break;
                        case AttributeTypeCode.Double:
                            var dattr = (DoubleAttributeMetadata)attribute;
                            attributeline = $"{attributeName} float null,";
                            break;
                        case AttributeTypeCode.Integer:
                            attributeline = $"{attributeName} int null,";
                            break;
                        case AttributeTypeCode.Lookup:
                            attributeline = $"{attributeName} uniqueidentifier,";
                            //ensure there isn't a type attribute already, owner and parent columns have a type field
                            if (!attributes.Any(a => a.LogicalName.ToLower() == $"{attribute.LogicalName}type"))
                                attributeTypeLine = $"[{attribute.LogicalName}type] nvarchar(255) null,";
                            break;
                        case AttributeTypeCode.Memo:
                            attributeline = $"{attributeName} nvarchar(max) null,";
                            break;
                        case AttributeTypeCode.Money:
                            //money has a max value 15 numbers long and a max precision of 10 after currency conversion
                            attributeline = $"{attributeName} decimal(25,10),";
                            break;
                        case AttributeTypeCode.Owner:
                            attributeline = $"{attributeName} uniqueidentifier null,";
                            break;
                        case AttributeTypeCode.PartyList:
                            attributeline = $"{attributeName} varchar(max) null,";
                            break;
                        case AttributeTypeCode.Picklist:
                            attributeline = $"{attributeName} int null,";
                            break;
                        case AttributeTypeCode.State:
                            attributeline = $"{attributeName} int null,";
                            break;
                        case AttributeTypeCode.Status:
                            attributeline = $"{attributeName} int null,";
                            break;
                        case AttributeTypeCode.Virtual:
                        case AttributeTypeCode.String:
                            attributeline = $"{attributeName} nvarchar(max) null,";
                            break;
                        case AttributeTypeCode.Uniqueidentifier:
                            if (attribute.IsPrimaryId.HasValue && attribute.IsPrimaryId.Value)
                                attributeline = $"{attributeName} uniqueidentifier not null,";
                            else
                                attributeline = $"{attributeName} uniqueidentifier,";
                            break;
                        case AttributeTypeCode.CalendarRules:
                            log.LogWarning("Calendar Rules not handled");
                            break;
                        case AttributeTypeCode.BigInt:
                            attributeline = $"{attributeName} bigint null,";
                            break;
                        case AttributeTypeCode.EntityName:
                            attributeline = $"{attributeName} nvarchar(255) null,";
                            break;
                        default:
                            log.LogWarning($"unknown datatype {attribute.AttributeType}");
                            break;
                    }

                    //some of the built in look up tyes (i.e. owner and parent have a Type field already) so the tracker list prevents duplicate columns
                    if (attributeline != string.Empty)
                    {
                        fieldDefinitions.AppendLine(attributeline);
                    }

                    
                    if (attributeTypeLine != string.Empty)
                    {
                        fieldDefinitions.AppendLine(attributeTypeLine);
                    }
                }
            }
            return fieldDefinitions.ToString();
        }
    }
}
