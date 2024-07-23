using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SERIAL_CONVERTOR
{
    class cDefApi
    {
        // @@@.INI파일에서 정수형 데이터를 읽어옴.
        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);

        // @@@.INI파일에서 문자형 데이터를 읽어옴.
        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        public static void GsGetInitPorFilePostDB(ref string pIP,
                                                 ref string pPORT,
                                                 ref string pDatabase,
                                                 ref string pUserID,
                                                 ref string pPassword,
                                                 ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFilePostDB]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("P_DB", "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pIP = sb.ToString();

                GetPrivateProfileString("P_DB", "PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPORT = sb.ToString();

                GetPrivateProfileString("P_DB", "DATABASE", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pDatabase = sb.ToString();

                GetPrivateProfileString("P_DB", "USER", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUserID = sb.ToString();

                GetPrivateProfileString("P_DB", "USER_PW", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPassword = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB]Error::" + pRtnMsg;
        }

        // @@@.GsGetInitPorFileDB
        public static void GsGetStringInitPorFile(string strAppName,
                                                  string strKeyName,
                                                  ref string strValue,
                                                  ref string strRtnMsg)
        {
            string strTitle = "[GsGetStringInitPorFile] ";

            strValue = "";

            StringBuilder sb = new StringBuilder(1000);
            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                strRtnMsg = strTitle + "Not Found File";
                return;
            }

            try
            {
                strRtnMsg = "";
                GetPrivateProfileString(strAppName, strKeyName, null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                strValue = sb.ToString();
                strRtnMsg = strTitle + "Sucess";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.Message;
                return;
            }
        }

        // @@@.GsGetInitPorFileDB
        public static void GsGetIntInitPorFile(string strAppName,
                                              string strKeyName,
                                              ref int nValue,
                                              ref string strRtnMsg)
        {
            string strTitle = "[GsGetIntInitPorFile] ";

            nValue = 0;
            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                strRtnMsg = strTitle + "Not Found File";
                return;
            }

            try
            {
                strRtnMsg = "";
                nValue = GetPrivateProfileInt(strAppName, strKeyName, nValue, cDefApp.GM_ENV_INI);
                strRtnMsg = strTitle + "Sucess";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.Message;
                return;
            }
        }

        #region [DB_1]::Oracle 접속정보
        public static void GsGetInitPorFileDB_1(ref string pProvider,
                                              ref string pAlias,
                                              ref string pUserID,
                                              ref string pPassword,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB_1]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB_1", "PROVIDER", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pProvider = sb.ToString();

                GetPrivateProfileString("DB_1", "ALIAS", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pAlias = sb.ToString();

                GetPrivateProfileString("DB_1", "USERID", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUserID = sb.ToString();

                GetPrivateProfileString("DB_1", "PASSWORD", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPassword = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB_1]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB]Error::" + pRtnMsg;
        }
        #endregion

        #region [DB_2]::PostgreSql 접속정보
        public static void GsGetInitPorFileDB_2(ref string pIP,
                                              ref string pDATABASE,
                                              ref string pPORT,
                                              ref string pUSER,
                                              ref string pUSER_PW,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileDB_2]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("DB_2", "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pIP = sb.ToString();

                GetPrivateProfileString("DB_2", "DATABASE", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pDATABASE = sb.ToString();

                GetPrivateProfileString("DB_2", "PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pPORT = sb.ToString();

                GetPrivateProfileString("DB_2", "USER", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSER = sb.ToString();

                GetPrivateProfileString("DB_2", "USER_PW", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSER_PW = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileDB_2]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileDB_2]Error::" + pRtnMsg;
        }
        #endregion

        #region [CNF]::접속정보
        public static void GsGetInitPorFileCNF(ref string pGRP,
                                              ref string pUSERID,
                                              ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsGetInitPorFileCNF]::Not Found File";
                return;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString("CNF", "WH_TYP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pGRP = sb.ToString();

                GetPrivateProfileString("CNF", "USERID", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pUSERID = sb.ToString();

                pRtnMsg = "[GsGetInitPorFileCNF]::Sucess";
                return;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsGetInitPorFileCNF]Error::" + pRtnMsg;
        }
        #endregion

        #region [PROCESS]::CNT 가져오기
        public static bool GsReadInitProfileProcessCnt(string pAppNm, ref int pProcessCnt, ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileProcessCnt]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";

                pProcessCnt = GetPrivateProfileInt(pAppNm, "CNT", 1, cDefApp.GM_ENV_INI);

                pRtnMsg = "[GsReadInitProfileProcessCnt]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileProcessCnt]Error::" + pRtnMsg;
            return false;
        }
        #endregion

        #region [E-COMM]::설비통신 접속정보 - Server Socket임
        public static bool GsReadInitProfileEthernetCom(string pAppNm,
                                                    ref string pComPort,
                                                    ref string pSEC_No,
                                                    ref string pLogPath,
                                                    ref string pLogFileNm,
                                                    ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileEthernetCom]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";

                //GetPrivateProfileString(pAppNm, "PLC_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                //pPlcNo = sb.ToString();

                //GetPrivateProfileString(pAppNm, "EQMT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                //pEqmt = sb.ToString();

                //GetPrivateProfileString(pAppNm, "IP", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                //pCommIP = sb.ToString();

                GetPrivateProfileString(pAppNm, "PORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pComPort = sb.ToString();

                GetPrivateProfileString(pAppNm, "S2E_NO", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pSEC_No = sb.ToString();

                GetPrivateProfileString(pAppNm, "LOG_PATH", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogPath = sb.ToString();

                GetPrivateProfileString(pAppNm, "FILENAME", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogFileNm = sb.ToString();

                pRtnMsg = "[GsReadInitProfileEthernetCom]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileEthernetCom]Error::" + pRtnMsg;
            return false;
        }
        #endregion
        #region [S-COMM]::설비통신 접속정보
        public static bool GsReadInitProfileSerialCom(  string pAppNm,
                                                ref string pCommPort,
                                                ref string pBaudrate,
                                                ref string pStopBit,
                                                ref string pDataBit,
                                                ref string pParityBit,
                                                ref string pLogPath,
                                                ref string pLogFileNm,
                                                ref string pRtnMsg)
        {
            StringBuilder sb = new StringBuilder(1000);

            if (!System.IO.File.Exists(cDefApp.GM_ENV_INI))
            {
                pRtnMsg = "[GsReadInitProfileSerialCom]::Not Found File";
                return false;
            }

            try
            {
                pRtnMsg = "";

                GetPrivateProfileString(pAppNm, "COMMPORT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pCommPort = sb.ToString();

                GetPrivateProfileString(pAppNm, "BAUDRATE", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pBaudrate = sb.ToString();

                GetPrivateProfileString(pAppNm, "STOPBIT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pStopBit = sb.ToString();

                GetPrivateProfileString(pAppNm, "DATABIT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pDataBit = sb.ToString();

                GetPrivateProfileString(pAppNm, "PARITYBIT", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pParityBit = sb.ToString();


                GetPrivateProfileString(pAppNm, "LOG_PATH", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogPath = sb.ToString();

                GetPrivateProfileString(pAppNm, "FILENAME", null, sb, sb.Capacity, cDefApp.GM_ENV_INI);
                pLogFileNm = sb.ToString();

                pRtnMsg = "[GsReadInitProfileSerialCom]::Sucess";
                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
            }
            pRtnMsg = "[GsReadInitProfileSerialCom]Error::" + pRtnMsg;
            return false;
        }
        #endregion
    }
}
