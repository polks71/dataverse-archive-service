using ArchiveService.Shared.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArchiveService.Shared.Helpers
{
    internal static class GetTableColumnsHelper
    {
        public static async Task<List<InformationSchemaColumns>> GetTableColumnsAsync(string tablename, string schemaname, SqlConnection conn)
        {
            var columns = new List<InformationSchemaColumns>();
            using (var comm = conn.CreateCommand())
            {
                comm.CommandText = @"SELECT
        table_schema, table_name, column_name, data_type, character_maximum_length, numeric_precision, numeric_precision_radix
    FROM
        INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = @tablename and TABLE_SCHEMA = @schemaname
    ORDER BY 3";
                comm.Parameters.AddWithValue("@tablename", tablename);
                comm.Parameters.AddWithValue("@schemaname", schemaname);
                using (var reader = await comm.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var column = new InformationSchemaColumns();
                        if (reader.IsDBNull("column_name"))
                            throw new Exception("ColumnName is null, but shouldn't be");
                        column.ColumnName = reader.GetFieldValue<string>("column_name");
                        if (reader.IsDBNull("data_type"))
                            throw new Exception("DataType is null, but shouldn't be");
                        column.DataType = reader.GetFieldValue<string>("data_type");
                        if (reader.IsDBNull("table_schema"))
                            throw new Exception("TableSchema is null, but shouldn't be");
                        column.TableSchema = reader.GetFieldValue<string>("table_schema");
                        if (reader.IsDBNull("table_name"))
                            throw new Exception("TableName is null, but shouldn't be");
                        column.TableName = reader.GetFieldValue<string>("table_name");
                        column.CharacterMaxLength = reader.IsDBNull("character_maximum_length") ? null : reader.GetFieldValue<int>("character_maximum_length");
                        column.NumericPrecision = reader.IsDBNull("numeric_precision") ? null : reader.GetFieldValue<byte>("numeric_precision");
                        column.NumericPrecisionRadix = reader.IsDBNull("numeric_precision_radix") ? null : reader.GetFieldValue<short>("numeric_precision_radix");
                        columns.Add(column);
                    }
                }
            }

            return columns;
        }

        public static async Task<string> GetPrimaryKeyColumn(string tablename, string schemaname, SqlConnection conn)
        {
            using (var comm = conn.CreateCommand())
            {
                comm.CommandText = @"select top 1 C.COLUMN_NAME FROM  
INFORMATION_SCHEMA.TABLE_CONSTRAINTS T  
JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE C  
ON C.CONSTRAINT_NAME=T.CONSTRAINT_NAME  
WHERE  
C.TABLE_NAME = @tablename and C.TABLE_SCHEMA = @schemaname 
and T.CONSTRAINT_TYPE='PRIMARY KEY'";
                comm.Parameters.AddWithValue("@tablename", tablename);
                comm.Parameters.AddWithValue("@schemaname", schemaname);
                return (string) await comm.ExecuteScalarAsync();
            }
        }
    }
}
