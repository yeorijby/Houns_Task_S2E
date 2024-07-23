using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using NpgsqlTypes;

namespace SERIAL_CONVERTOR
{
    class DbLang
    {
#if ORACLE
        public const string NVL = "NVL";
        public const string SYSDATE = "SYSDATE";
        public const string II = "||";
        public const OleDbType VARCHAR = OleDbType.VarChar;
        public const OleDbType INT = OleDbType.Integer;
#endif
#if POSTGRESQL
        public const string NVL = "COALESCE";
        public const string SYSDATE = "NOW()";
        public const string II = "||";
        public const NpgsqlDbType VARCHAR = NpgsqlDbType.Varchar;
        public const NpgsqlDbType INT = NpgsqlDbType.Integer;
#endif
#if SQL
        public const string NVL = "ISNULL";
        public const string SYSDATE = "ISNULL";
        public const string II = "+";
#endif
    }
}
