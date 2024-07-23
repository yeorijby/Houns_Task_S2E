using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Threading;
using Microsoft.VisualBasic;
using NpgsqlTypes;
using Npgsql;

namespace SERIAL_CONVERTOR
{
    public class cDefApp
    {
       // public static string GM_WH_TYP = "CP";
        public static string GM_MC_TYP = "CV";
        public static string GM_CNF_USER_ID = "T-WC";

        public static OleDbConnection GM_DB_CN;            // @.DB���� ����[OracleConnection]
        public static NpgsqlConnection GM_PDB_CN;            // @.DB���� ����[PostgreSQLConnection]

        public const string GM_ENV_INI = "./WCS_DB.INI";   // @.INI ���ϰ��
        //public const string GM_ENV_INI = "C:/Users/JoHanSung/Desktop/TSK_COMM_BCR/bin/Debug/ENV_BCR.INI";   // @.INI ���ϰ��
        
        //public const string GM_ENV_INI = "D:/BSWCS_Task/TASK(����)/BCR/ENV_BCR.INI";   // @.INI ���ϰ��

        // @@.[DB_TYPE] ��������
        public static string DB_TYPE_INI = "0"; // 0:None, 1:Oracle, 2:PostgreSql, 3:MS_SQL, 4:MY_SQL

        // @@.[DB_1] Oracle ��������
        public static string GM_DB1_PROVIDER = "";
        public static string GM_DB1_ALIAS = "";
        public static string GM_DB1_USERID = "";
        public static string GM_DB1_PASSWORD = "";

        // @@.[DB_2] PostgreSql ��������
        public static string GM_DB2_IP = "";
        public static string GM_DB2_DATABASE = "";
        public static string GM_DB2_PORT = "";
        public static string GM_DB2_USER = "";
        public static string GM_DB2_USER_PW = "";

        // @@.������� ����
        // [CNF]
        public static string GM_WH_TYP = "";
        public static string GM_USERID = "";

        //LOG ����
        public static Queue<LogParam>[] m_LogQ = new Queue<LogParam>[200];
        public enum eLogWriteGbn { COMM1 = 0, COMM2 = 1, COMM3 = 2, COMM4 = 3, COMM5 = 4, COMM6 = 5, COMM7 = 6, COMM8 = 7, COMM9 = 8, COMM10 = 9 };

        // @@.W/C ��������
        public static string GM_DB_PROVIDER = "";
        public static string GM_DB_ALIAS = "";
        public static string GM_DB_USERID = "";
        public static string GM_DB_PASSWORD = "";

        // @@.W/C ��������(post)
        public static string GM_PDB_IP = "";
        public static string GM_PDB_DATABASE = "";
        public static string GM_PDB_PORT = "";
        public static string GM_PDB_USER = "";
        public static string GM_PDB_USER_PW = "";

        // @@.������� Ÿ�Ӿƿ� ����
        public static int GM_COMM_SND_TIME_OUT = 500;
        public static int GM_COMM_ACK_TIME_OUT = 500;
        public static int GM_COMM_RCV_TIME_OUT = 500;
        public static int GM_COMM_READ = 1000;

        public static int GM_PROCESS_CNT = 0;

        // @@.Application ���� ���� ���� ���� ����
        public static bool GM_STAT_MAIN = false;  // @.��ü �ý��� ���� ����[���� �ý����� ���� �Ǹ� ��ü ����!] 
        public static bool GM_RE_START = false;

        // @@.DB ����
        public cDbUse mDbWrk = new cDbUse("Multi", false);            // @.DB Class[Work]
        //public cDbPostUse m_pBdb = new cDbPostUse();

        // @@.������
        public const string cSPA = ";";

        // @@.���ó���۾�����ü
        public struct stutComProc
        {
            public bool bMakeTgmSnd;     // @.����Tgm�ۼ�
            public bool bSndTgm;          // @.Tgm����
            public bool bRcvAck;          // @.Ack����
            public int nChkAck;           // @.����Acküũ
            public bool bRcvTgm;          // @.Tgm����
            public int nChkTgm;           // @.����Tgmüũ
            public bool bDBProc;          // @.DBó��
            public string sProcMsg;       // @.ó���޼���

            public void init()
            {                               // @.����ü ���𺯼� �ʱ�ȭ
                this.bMakeTgmSnd = false;         // @.����Tgm�ۼ�
                this.bSndTgm = false;             // @.Tgm����
                this.bRcvAck = false;             // @.Ack����
                this.nChkAck = 99;                // @.����Acküũ
                this.bRcvTgm = false;             // @.Tgm����
                this.nChkTgm = 99;                // @.����Tgmüũ
                this.bDBProc = false;             // @.DBó��
                this.sProcMsg = "";               // @.ó���޼���
            }
        }

        // @@.DB Err ���
        public const int DB_ERR = -1;      // @.DB ����
        public const int DB_LOCK = -2;    // @.DB ������ DB Lock
        public const int DB_DUP = -3;      // @.DB ������ �ߺ� ����Ÿ

        // @@.enum ����
        public enum eComSts { ComNor = 0, ComErr = 1 };                     // @.��Ż���
        public enum eLogMsgType { MSG_NOR = 0, MSG_IMP = 1, MSG_ERR = 2 };   // @.eLogMsgType[0:����, 1:�߿�, 2:����]

        // @@.Structure ����
        public struct stutLogMsgInfo
        {
            public string Time;
            public string ID;
            public string MsgTyp;
            public string Com;
            public string Msg;
            public string Tgm;
            public void init()
            {
                this.Time = "";
                this.ID = "";
                this.MsgTyp = "";
                this.Com = "";
                this.Msg = "";
                this.Tgm = "";
            }
        }

        public const string CRLF = ControlChars.CrLf; // @.�����[vbCrLf]
        public const byte STX = 0x2;
        public const byte ETX = 0x3;
        public static char GM_STR_STX = Convert.ToChar(2);
        public static char GM_STR_ETX = Convert.ToChar(3);
        public static char GM_SLASH = Convert.ToChar(47);
        public static char GM_SPACE = Convert.ToChar(32);

    }
}
