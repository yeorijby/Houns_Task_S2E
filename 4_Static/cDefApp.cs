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

        public static OleDbConnection GM_DB_CN;            // @.DB연결 정의[OracleConnection]
        public static NpgsqlConnection GM_PDB_CN;            // @.DB연결 정의[PostgreSQLConnection]

        public const string GM_ENV_INI = "./WCS_DB.INI";   // @.INI 파일경로
        //public const string GM_ENV_INI = "C:/Users/JoHanSung/Desktop/TSK_COMM_BCR/bin/Debug/ENV_BCR.INI";   // @.INI 파일경로
        
        //public const string GM_ENV_INI = "D:/BSWCS_Task/TASK(서비스)/BCR/ENV_BCR.INI";   // @.INI 파일경로

        // @@.[DB_TYPE] 접속정보
        public static string DB_TYPE_INI = "0"; // 0:None, 1:Oracle, 2:PostgreSql, 3:MS_SQL, 4:MY_SQL

        // @@.[DB_1] Oracle 접속정보
        public static string GM_DB1_PROVIDER = "";
        public static string GM_DB1_ALIAS = "";
        public static string GM_DB1_USERID = "";
        public static string GM_DB1_PASSWORD = "";

        // @@.[DB_2] PostgreSql 접속정보
        public static string GM_DB2_IP = "";
        public static string GM_DB2_DATABASE = "";
        public static string GM_DB2_PORT = "";
        public static string GM_DB2_USER = "";
        public static string GM_DB2_USER_PW = "";

        // @@.설비통신 정보
        // [CNF]
        public static string GM_WH_TYP = "";
        public static string GM_USERID = "";

        //LOG 관련
        public static Queue<LogParam>[] m_LogQ = new Queue<LogParam>[200];
        public enum eLogWriteGbn { COMM1 = 0, COMM2 = 1, COMM3 = 2, COMM4 = 3, COMM5 = 4, COMM6 = 5, COMM7 = 6, COMM8 = 7, COMM9 = 8, COMM10 = 9 };

        // @@.W/C 접속정보
        public static string GM_DB_PROVIDER = "";
        public static string GM_DB_ALIAS = "";
        public static string GM_DB_USERID = "";
        public static string GM_DB_PASSWORD = "";

        // @@.W/C 접속정보(post)
        public static string GM_PDB_IP = "";
        public static string GM_PDB_DATABASE = "";
        public static string GM_PDB_PORT = "";
        public static string GM_PDB_USER = "";
        public static string GM_PDB_USER_PW = "";

        // @@.소켓통신 타임아웃 정의
        public static int GM_COMM_SND_TIME_OUT = 500;
        public static int GM_COMM_ACK_TIME_OUT = 500;
        public static int GM_COMM_RCV_TIME_OUT = 500;
        public static int GM_COMM_READ = 1000;

        public static int GM_PROCESS_CNT = 0;

        // @@.Application 종료 여부 설정 변수 선언
        public static bool GM_STAT_MAIN = false;  // @.전체 시스템 종료 여부[메인 시스템이 종료 되면 전체 종료!] 
        public static bool GM_RE_START = false;

        // @@.DB 선언
        public cDbUse mDbWrk = new cDbUse("Multi", false);            // @.DB Class[Work]
        //public cDbPostUse m_pBdb = new cDbPostUse();

        // @@.구분자
        public const string cSPA = ";";

        // @@.통신처리작업구조체
        public struct stutComProc
        {
            public bool bMakeTgmSnd;     // @.전송Tgm작성
            public bool bSndTgm;          // @.Tgm전송
            public bool bRcvAck;          // @.Ack수신
            public int nChkAck;           // @.수신Ack체크
            public bool bRcvTgm;          // @.Tgm수신
            public int nChkTgm;           // @.수신Tgm체크
            public bool bDBProc;          // @.DB처리
            public string sProcMsg;       // @.처리메세지

            public void init()
            {                               // @.구조체 선언변수 초기화
                this.bMakeTgmSnd = false;         // @.전송Tgm작성
                this.bSndTgm = false;             // @.Tgm전송
                this.bRcvAck = false;             // @.Ack수신
                this.nChkAck = 99;                // @.수신Ack체크
                this.bRcvTgm = false;             // @.Tgm수신
                this.nChkTgm = 99;                // @.수신Tgm체크
                this.bDBProc = false;             // @.DB처리
                this.sProcMsg = "";               // @.처리메세지
            }
        }

        // @@.DB Err 상수
        public const int DB_ERR = -1;      // @.DB 에러
        public const int DB_LOCK = -2;    // @.DB 에러중 DB Lock
        public const int DB_DUP = -3;      // @.DB 에러중 중복 데이타

        // @@.enum 정의
        public enum eComSts { ComNor = 0, ComErr = 1 };                     // @.통신상태
        public enum eLogMsgType { MSG_NOR = 0, MSG_IMP = 1, MSG_ERR = 2 };   // @.eLogMsgType[0:보통, 1:중요, 2:에러]

        // @@.Structure 정의
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

        public const string CRLF = ControlChars.CrLf; // @.제어문자[vbCrLf]
        public const byte STX = 0x2;
        public const byte ETX = 0x3;
        public static char GM_STR_STX = Convert.ToChar(2);
        public static char GM_STR_ETX = Convert.ToChar(3);
        public static char GM_SLASH = Convert.ToChar(47);
        public static char GM_SPACE = Convert.ToChar(32);

    }
}
