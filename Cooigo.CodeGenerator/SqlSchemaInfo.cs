using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Cooigo.CodeGenerator
{
    public class SqlSchemaInfo
    {
        public static string InformationSchema(IDbConnection connection)
        {
            return "INFORMATION_SCHEMA.";
        }


        public static List<Tuple<string, string>> GetTableNames(IDbConnection connection)
        {
            var tables = ((DbConnection)connection).GetSchema("Tables");

            var result = new List<Tuple<string, string>>();

            foreach (DataRow row in tables.Rows)
            {
                var tableType = row["TABLE_TYPE"] as string;

                //if (tableType != null && tableType.ToLowerInvariant() == "view")
                //    continue;

                var schema = row["TABLE_SCHEMA"] as string;
                var tableName = row["TABLE_NAME"] as string;

                result.Add(new Tuple<string, string>(schema, tableName));
            }

            result.Sort();

            return result;
        }

        public static List<string> GetTablePrimaryFields(IDbConnection connection, string schema, string tableName)
        {
            var inf = InformationSchema(connection);
            List<string> primaryFields = new List<string>();
            var columns = ((DbConnection)connection).GetSchema("Columns", new string[] { null, schema, tableName, null });
            if (columns.Columns.Contains("PRIMARY_KEY"))
            {
                foreach (DataRow row in columns.Rows)
                {
                    try
                    {
                        var isPrimaryKey = row["PRIMARY_KEY"] as Boolean?;
                        if (isPrimaryKey == true)
                            primaryFields.Add((string)row["COLUMN_NAME"]);
                    }
                    catch (Exception)
                    {

                    }
                }

                return primaryFields;
            }

            return primaryFields;
        }


        public static List<string> GetTableIdentityFields(IDbConnection connection, string schema, string tableName)
        {
            var columns = ((DbConnection)connection).GetSchema("Columns", new string[] { null, schema, tableName, null });
            List<string> identityFields = new List<string>();


            if (columns.Columns.Contains("AUTOINCREMENT"))
            {
                foreach (DataRow row in columns.Rows)
                {
                    var isIdentity = row["AUTOINCREMENT"] as Boolean?;
                    if (isIdentity == true)
                        identityFields.Add((string)row["COLUMN_NAME"]);
                }

                return identityFields;
            }

            identityFields = connection.Query<string>(@"
SELECT  c.name
FROM    syscolumns c
        LEFT OUTER JOIN sysobjects t ON t.id = c.id
WHERE   c.[status] & 128 = 128
        AND t.xtype = 'U'
        AND t.name = @name
", new { name = tableName }).ToList();

            return identityFields;
        }

        public static List<string> GetTableFieldNames(IDbConnection connection, string schema, string tableName)
        {
            var inf = InformationSchema(connection);
            var list = new List<string>();

            var columns = ((DbConnection)connection).GetSchema("Columns", new string[] { null, schema, tableName, null });
            var dict = new Dictionary<string, int>();
            foreach (DataRow row in columns.Rows)
            {
                var col = (string)row["COLUMN_NAME"];
                dict[col] = (int)row["ORDINAL_POSITION"];
                list.Add(col);
            }
            list.Sort((x, y) => dict[x].CompareTo(dict[y]));

            return list;
        }

        public static List<ForeignKeyInfo> GetTableSingleFieldForeignKeys(IDbConnection connection, string schema, string tableName)
        {
            var inf = InformationSchema(connection);
            List<ForeignKeyInfo> foreignKeyInfos = new List<ForeignKeyInfo>();

            var list = connection.Query<ForeignKeyInfo>(@"

SELECT  fk.CONSTRAINT_NAME AS FKName ,
        fk.TABLE_SCHEMA AS FKSchema ,
        fk.TABLE_NAME AS FKTable ,
        fk.COLUMN_NAME AS FKColumn ,
        pk.TABLE_SCHEMA AS PKSchema ,
        pk.TABLE_NAME AS PKTable ,
        pk.COLUMN_NAME AS PKColumn
FROM    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c ,
        INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE fk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk
WHERE   fk.TABLE_SCHEMA = @schema
        AND fk.TABLE_NAME = @tableName
        AND fk.CONSTRAINT_SCHEMA = c.CONSTRAINT_SCHEMA
        AND fk.CONSTRAINT_NAME = c.CONSTRAINT_NAME
        AND pk.CONSTRAINT_SCHEMA = c.UNIQUE_CONSTRAINT_SCHEMA
        AND pk.CONSTRAINT_NAME = c.UNIQUE_CONSTRAINT_NAME
ORDER BY fk.CONSTRAINT_NAME

", new { schema, tableName }).ToList();

            foreach (var foreignKeyInfo in list)
            {
                string priorName = "";
                bool priorDeleted = false;

                // eğer bir önceki ile aynıysa bunu listeye ekleme ve öncekini de sil
                if (priorName == foreignKeyInfo.FKName)
                {
                    if (!priorDeleted)
                    {
                        foreignKeyInfos.RemoveAt(foreignKeyInfos.Count - 1);
                        priorDeleted = true;
                    }
                    continue;
                }

                foreignKeyInfos.Add(foreignKeyInfo);
                priorDeleted = false;
                priorName = foreignKeyInfo.FKName;
            }


            return foreignKeyInfos;
        }


        public static List<FieldInfo> GetTableFieldInfos(IDbConnection connection, string schema, string tableName)
        {
            var inf = InformationSchema(connection);
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            List<ForeignKeyInfo> foreignKeys = GetTableSingleFieldForeignKeys(connection, schema, tableName);
            List<string> primaryFields = GetTablePrimaryFields(connection, schema, tableName);
            List<string> identityFields = GetTableIdentityFields(connection, schema, tableName);

            var columns = ((DbConnection)connection).GetSchema("Columns", new string[] { null, schema, tableName, null });
            var order = new Dictionary<string, int>();

            var ordinal = "ORDINAL_POSITION";
            var columnName = "COLUMN_NAME";
            var isNullable = "IS_NULLABLE";
            var charMax = "CHARACTER_MAXIMUM_LENGTH";
            var numPrec = "NUMERIC_PRECISION";
            var numScale = "NUMERIC_SCALE";
            var dataType = "DATA_TYPE";
            if (!columns.Columns.Contains(ordinal) &&
                columns.Columns.Contains(ordinal.ToLowerInvariant()))
            {
                ordinal = ordinal.ToLowerInvariant();
                columnName = columnName.ToLowerInvariant();
                dataType = dataType.ToLowerInvariant();
                isNullable = isNullable.ToLowerInvariant();
                charMax = charMax.ToLowerInvariant();
                numPrec = numPrec.ToLowerInvariant();
                numScale = numScale.ToLowerInvariant();
            }

            if (!columns.Columns.Contains(dataType) &&
                columns.Columns.Contains("COLUMN_DATA_TYPE"))
                dataType = "COLUMN_DATA_TYPE";

            if (!columns.Columns.Contains(charMax) &&
                columns.Columns.Contains("COLUMN_SIZE"))
                charMax = "COLUMN_SIZE";

            foreach (DataRow row in columns.Rows)
            {
                FieldInfo fieldInfo = new FieldInfo();
                fieldInfo.FieldName = (string)row[columnName];

                order[fieldInfo.FieldName] = Convert.ToInt32(row[ordinal]);

                fieldInfo.IsPrimaryKey =
                    primaryFields.IndexOf(fieldInfo.FieldName) >= 0;
                fieldInfo.IsIdentity =
                    identityFields.IndexOf(fieldInfo.FieldName) >= 0;
                fieldInfo.IsNullable = (row[isNullable] as string == "YES") || (row[isNullable] as Boolean? == true);
                fieldInfo.DataType = row[dataType] as string;
                fieldInfo.Size = 0;

                if (fieldInfo.DataType != SqlInt &&
                    fieldInfo.DataType != SqlInteger &&
                    fieldInfo.DataType != SqlReal &&
                    fieldInfo.DataType != SqlFloat &&
                    fieldInfo.DataType != SqlTinyInt &&
                    fieldInfo.DataType != SqlSmallInt &&
                    fieldInfo.DataType != SqlBigInt &&
                    fieldInfo.DataType != SqlInt8 &&
                    fieldInfo.DataType != SqlInt4)
                {
                    var val = row[charMax];
                    var size = (val == null || val == DBNull.Value) ? (Int64?)null : Convert.ToInt64(val);
                    if (size != null && size > 0 && size <= 1000000000)
                        fieldInfo.Size = (int)size.Value;

                    val = row[numPrec];
                    var prec = (val == null || val == DBNull.Value) ? (Int64?)null : Convert.ToInt64(val);

                    if (prec != null && (SqlTypeNameToFieldType(fieldInfo.DataType) != "String") &&
                        prec >= 0 && prec < 1000000000)
                    {
                        fieldInfo.Size = Convert.ToInt32(prec.Value);
                    }

                    val = row[numScale];
                    var scale = (val == null || val == DBNull.Value) ? (Int64?)null : Convert.ToInt64(val);
                    if (scale != null && scale >= 0 && scale < 1000000000)
                        fieldInfo.Scale = Convert.ToInt32(scale.Value);
                }

                fieldInfos.Add(fieldInfo);
            }

            fieldInfos.Sort((x, y) => order[x.FieldName].CompareTo(order[y.FieldName]));

            // şimdi ForeignKey tespiti yap, bunu önceki döngüde yapamayız reader'lar çakışır

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                ForeignKeyInfo foreignKey = foreignKeys.Find(
                    delegate (ForeignKeyInfo d) { return d.FKColumn == fieldInfo.FieldName; });

                if (foreignKey != null)
                {
                    fieldInfo.PKSchema = foreignKey.PKSchema;
                    fieldInfo.PKTable = foreignKey.PKTable;
                    fieldInfo.PKColumn = foreignKey.PKColumn;
                }
            }

            return fieldInfos;
        }

        public class FieldInfo
        {
            public string FieldName;
            public int Size;
            public int Scale;
            public bool IsPrimaryKey;
            public bool IsIdentity;
            public bool IsNullable;
            public string PKSchema;
            public string PKTable;
            public string PKColumn;
            public string DataType;
        }

        public class ForeignKeyInfo
        {
            public string FKName;
            public string FKSchema;
            public string FKTable;
            public string FKColumn;
            public string PKSchema;
            public string PKTable;
            public string PKColumn;
        }

        const string SqlBit = "bit";
        const string SqlDate = "date";
        const string SqlDateTime = "datetime";
        const string SqlDateTime2 = "datetime2";
        const string SqlSmallDateTime = "smalldatetime";
        const string SqlDecimal = "decimal";
        const string SqlNumeric = "numeric";
        const string SqlBigInt = "bigint";
        const string SqlInt = "int";
        const string SqlInteger = "integer";
        const string SqlDouble = "double";
        const string SqlDoublePrecision = "double precision";
        const string SqlMoney = "money";
        const string SqlNChar = "nchar";
        const string SqlNVarChar = "nvarchar";
        const string SqlNText = "ntext";
        const string SqlText = "text";
        const string SqlBlobSubType1 = "blob sub_type 1";
        const string SqlReal = "real";
        const string SqlFloat = "float";
        const string SqlSmallInt = "smallint";
        const string SqlVarChar = "varchar";
        const string SqlChar = "char";
        const string SqlUniqueIdentifier = "uniqueidentifier";
        const string SqlVarBinary = "varbinary";
        const string SqlTinyInt = "tinyint";
        const string SqlTime = "time";
        const string SqlTimestamp = "timestamp";
        const string SqlRowVersion = "rowversion";
        const string SqlInt8 = "int8";
        const string SqlInt4 = "int4";

        public static string SqlTypeNameToFieldType(string sqlTypeName)
        {
            if (sqlTypeName == SqlNVarChar || sqlTypeName == SqlNText || sqlTypeName == SqlText || sqlTypeName == SqlNChar ||
                sqlTypeName == SqlVarChar || sqlTypeName == SqlChar || sqlTypeName == SqlBlobSubType1)
                return "String";
            else if (sqlTypeName == SqlInt || sqlTypeName == SqlInteger || sqlTypeName == SqlInt4)
                return "Int32";
            else if (sqlTypeName == SqlBigInt || sqlTypeName == SqlInt8)
                return "Int64";
            else if (sqlTypeName == SqlMoney || sqlTypeName == SqlDecimal || sqlTypeName == SqlNumeric)
                return "Decimal";
            else if (sqlTypeName == SqlDateTime || sqlTypeName == SqlDateTime2 || sqlTypeName == SqlDate || sqlTypeName == SqlSmallDateTime)
                return "DateTime";
            else if (sqlTypeName == SqlTime)
                return "TimeSpan";
            else if (sqlTypeName == SqlBit)
                return "Boolean";
            else if (sqlTypeName == SqlReal)
                return "Single";
            else if (sqlTypeName == SqlFloat || sqlTypeName == SqlDouble || sqlTypeName == SqlDoublePrecision)
                return "Double";
            else if (sqlTypeName == SqlSmallInt || sqlTypeName == SqlTinyInt)
                return "Int16";
            else if (sqlTypeName == SqlUniqueIdentifier)
                return "Guid";
            else if (sqlTypeName == SqlVarBinary || sqlTypeName == SqlTimestamp || sqlTypeName == SqlRowVersion)
                return "Stream";
            else
                return "Stream";
        }
    }
}