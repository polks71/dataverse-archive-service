using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Models
{
    internal class InformationSchemaColumns
    {
        public string TableSchema { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int? CharacterMaxLength { get; set; }
        public byte? NumericPrecision { get; set; }
        public Int16? NumericPrecisionRadix { get; set; }

        public int NumericPrecisionInt
        {
            get => Convert.ToInt32(NumericPrecision);
        }

        public int NumericPrecisionRadixInt
        {
            get => Convert.ToInt32(NumericPrecisionRadix);
        }
    }
}
