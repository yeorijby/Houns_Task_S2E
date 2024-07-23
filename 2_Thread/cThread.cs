using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using NpgsqlTypes;
using Npgsql;
using log4net;
using log4net.Config;

namespace SERIAL_CONVERTOR
{
    //2014 조형준 메모리에 Cv 상태를 저장한다.
    public class WCData
    {
        private string Wcno;
        public string WCNO
        {
            get { return Wcno; }
            set { Wcno = value; }
        }
        private string COMMMode;
        public string COMMMODE
        {
            get { return COMMMode; }
            set { COMMMode = value; }
        }
        public WCData()
        {
            WCNO = "";
            COMMMODE = "";
        }
    }

    public class cThread
    {
        public PsMsgView callPsMsgView = null;
        public PfSetStatImgViewDB callPicDb = null;
        public PfSetStatImgViewSOCKET callPicSocket = null;

        public int m_nThNo = 0;
        public string m_strWhTyp = "";
        public string m_strEqmtTyp = "";
        public string m_strPlcNo = "";
        public string m_strIp = "";
        public string m_strPort = "";
        public int m_nPort = 0;
        public string m_strConnectionString = "";
        public string m_strS2E_No = "";
        public string m_strWcMcNo = "";
        public string m_strLogFileNm = "";
        public string m_strSpName = "";
        private string _strErrorMsg = "";
        public int m_nCmdCnt = 0;

        private bool m_bOpen;
        public bool IsOpen { get { return m_bOpen; } set { m_bOpen = value; } } //프로그램 화면표시용.
        private cWrkBcrSkt Wrk;
        public byte[] m_ByTxBuff = new byte[6];
        public int m_nByTxBuffLen;


        string strSql = "";
        string CRLF = "\r\n";
        int ReqCnt = 0;
        int nSelCnt = 0;
        public string m_strLogMsg = "";

        public string m_strStrgTyp = "";
        public string m_strGrpNo = "";


        public Thread m_thThread;
        public SYS_MAIN m_frmMain;
        public DateTime dtTmFlag;
        public double nRcvSec = 120;
        Dictionary<string, WCData> WcDic = new Dictionary<string, WCData>();

        public PictureBox picStatOp = null;
        public PictureBox picStatDbCn = null;

        cDefApp.stutComProc log = new cDefApp.stutComProc(); //Log 구조체.

        private string m_strRtnMsg = "";
        private string m_strRSTMsg = "";

        public string[] gStringArray = new string[100];
        public int gStringArrayCnt = 0;


        public cThread(int nProcessID,
                       string strWh_Typ,
                       //string strEqmtTyp,
                       //string strPlcNo,
                       string strS2E_No,
                       //string strWc_Mc_No,
                       //string strIp,
                       string strPort,
                       string strConnectString,
                       string strLogFileNm)
        {
            m_nThNo = nProcessID;
            m_strWhTyp = strWh_Typ;
            //m_strPlcNo = strPlcNo;
            //m_strEqmtTyp = strEqmtTyp;
            m_strIp = "127.0.0.1";//strIp;
            m_strPort = strPort;
            m_nPort = Convert.ToInt32(0 + strPort);
            m_strConnectionString = strConnectString;
            m_strS2E_No = strS2E_No;
            //m_strWcMcNo = strWc_Mc_No;
            m_strLogFileNm = strLogFileNm;
            m_strSpName = "COMM" + nProcessID;

            IsOpen = false;
            Wrk = new cWrkBcrSkt(m_strWhTyp, m_strEqmtTyp, m_strS2E_No, m_strSpName, m_strConnectionString);
        }


        public void TokenFrame(string strRecv)
        {
            string strBuff = "";
            //리스트 객체 생성
            //List<string> StringList = new List<string>(new string[] {"","","","" }); 
            //string[] stringArray = new string[100];
            int j = 0;

            for (int i = 0; i < strRecv.Length; i++)
            {
                if ((strRecv[i] == 'S') && (strRecv[i + 1] == 'T'))
                {
                    strBuff = "";
                    strBuff += strRecv[i];
                }
                else if (strRecv[i] == '\n')
                {
                    strBuff += strRecv[i];
                    //생성된 리스트 객체에 AddTail 하면됨
                    gStringArray[j++] = strBuff;
                }
                else
                {
                    strBuff += strRecv[i];
                }

            }
            gStringArrayCnt = j;
        }

        public void Thread_Doing()
        {
            int nCount = 0;
            try
            {
                if (cDefApp.GM_STAT_MAIN == false)
                {
                    throw new Exception("서비스 종료됨");
                }
                MakeMsg_Imp("Serial/Socket Connectting", m_nThNo);

                //시리얼 및 소켓 접속 체크
                if (Wrk.SktConnected == false || Wrk.m_bSerialCon == false)
                {
                    MakeMsg_Imp(string.Format("IP [{0}] PORT [{1}] 접속시도", m_strIp, m_strPort), m_nThNo);
                    Wrk.SetConfig(m_strIp, m_nPort, 2);

                    if (!m_frmMain.SerialPort_Open(ref m_strRtnMsg))
                    {
                        Wrk.m_bSerialCon = false;

                        SetErrorMsg("Comm" + m_nThNo + " :" + m_strRtnMsg);
                        MakeMsg_Error(m_strRtnMsg, m_nThNo);
                        throw new Exception(m_strRtnMsg);
                    }
                    Wrk.m_bSerialCon = true;

                    if (Wrk.mSkt == null)
                    {
                        Wrk.SktListen(m_nPort, 10000, ref m_strRtnMsg);
                    }
                    else if (!Wrk.mSkt.Connected)
                            Wrk.SktListen(m_nPort, 10000, ref m_strRtnMsg);
                    if (Wrk.mSkt == null || !Wrk.mSkt.Connected)
                    {
                        m_strRtnMsg = "Socket Listen 하였으나 Client가 접속하지 않음";
                        SetErrorMsg("Comm" + m_nThNo + " :" + m_strRtnMsg);
                        MakeMsg_Error(m_strRtnMsg, m_nThNo);

                        throw new Exception(m_strRtnMsg);
                    }

                }

                //소켓연결 + 시리얼 연결 성공
                if (Wrk.mSkt.Connected == true && Wrk.m_bSerialCon == true)
                {
                    IsOpen = true;
                    MakeMsg_Imp("Serial & Socket Connected Ok!", m_nThNo);

                    dtTmFlag = DateTime.Now.AddSeconds(nRcvSec); //data수신 없을시 재접속 시간 초기화

                    while (true)
                    {
                        #region [시리얼에 들어온 값이 있으면]
                        // 만약 시리얼에 들어온 내용이 있다면... 
                        if (m_frmMain.m_bSerialDataReceived == true)
                        {
                            nCount = 0;
                            string strTemp = "";
                            for (int iiii = 0; iiii < m_frmMain.m_nSerialMsgCnt; iiii++)
                            {
                                strTemp = m_frmMain.m_strSerialMsg[iiii];

                                MakeMsg("Serial Received ... [" + strTemp + "]", m_nThNo);

                                // 받은 내용을 그대로 소켓으로 던지기 
                                log.bSndTgm = Wrk.SndReqBCD(strTemp, ref m_strRtnMsg);

                                if (log.bSndTgm == true)
                                {
                                    ++nCount;
                                }
                            }
                            m_frmMain.m_nSerialMsgCnt = m_frmMain.m_nSerialMsgCnt - nCount;
                            m_frmMain.m_bSerialDataReceived = false;
                        }
                        #endregion


                        #region [이더넷에 들어온 값이 있으면 ]
                        //WC 값 리시브 하는 부분 시작
                        Wrk.mRcvTgm = "";

                        int nReadCnt = 0;
                        //WC 값 리시브 하기 (다시 탔을 때 리시브 되는지 확인)
                        log.bRcvTgm = Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 1, ref nReadCnt, ref m_strRSTMsg, ref m_strRtnMsg);

                        if (m_strRSTMsg == "2") //에러 종류
                        {
                            //MakeMsg_Error("WC 번호 [" + m_strWcNo + "]소켓 연결 끊김", m_nThNo);
                            throw new Exception("S2E 번호 [" + m_strS2E_No + "]소켓 연결 끊김 / " + m_strRtnMsg);
                        }

                        // 소켓에 리시브 한 값이 없으면 컨티뉴
                        if (Wrk.mRcvTgm.Length > 0)
                        {
                            #region [실제구동용]
                            // 소켓에 리시브 한 값이 있는경우
                            if (log.bRcvTgm == true)
                            {
                                // 소켓에 리시브한거를 화면에 출력하고
                                MakeMsg("Socket Received ... [" + Wrk.mRcvTgm.Trim() + "]", m_nThNo);

                                // 시리얼로 보내기 
                                m_frmMain.SendSerialData(Wrk.mRcvTgm.Trim());
                            }
                            #endregion
                        }
                        else
                        {
                            Thread.Sleep(500);
                            continue;
                        }

                        #endregion

                        dtTmFlag = dtTmFlag.AddSeconds(nRcvSec);
                        Thread.Sleep(500);
                    }
                }

            }
            catch (Exception ex)
            {
                m_frmMain.m_nSerialMsgCnt = m_frmMain.m_nSerialMsgCnt - nCount;
                MakeMsg_Error(ex.Message, m_nThNo);
                //InsertWcsLogPgr(m_strS2E_No, m_strRtnMsg);
            }

            //설비 통신상태 업데이트
            //Communication("N", m_strWhTyp, m_strEqmtTyp, m_strS2E_No, ref m_strRtnMsg);

            IsOpen = false;
            Wrk.Close(ref m_strRtnMsg);
            MakeMsg_Imp(m_strRtnMsg, m_nThNo);
            m_thThread = null;
        }
        #region 화면 표시용.
        private void MakeMsg(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView(msg, m_strS2E_No.ToString(), nThGbn);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void MakeMsg_Error(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_Error(msg, m_strS2E_No.ToString(), nThGbn);

                // 파일에 남길필요는 없을거 같아서 그냥 주석 처리함
                //bool bTemp1 = cDefApp.m_LogQ[m_nThNo] != null;

                //if (bTemp1)
                //    cDefApp.m_LogQ[m_nThNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }
        private void MakeMsg_Imp(string msg, int nThGbn)
        {
            try
            {
                m_frmMain.PsMsgView_IMP(msg, m_strS2E_No.ToString(), nThGbn);

                // 파일에 남길필요는 없을거 같아서 그냥 주석 처리함
                //bool bTemp1 = cDefApp.m_LogQ[m_nThNo] != null;

                //if (bTemp1)
                //    cDefApp.m_LogQ[m_nThNo].Enqueue(new LogParam(DateTime.Now, msg));
            }
            catch (Exception ex)
            {
                return;
            }

        }
        public void SetErrorMsg(string strMsg)
        {
            _strErrorMsg = strMsg;
            Log.Error(_strErrorMsg);
        }

        #endregion

        #region [Communication] :: EQP_MST의 CONNECT 여부 설정
        public bool Communication(string CONNECTED_YN, string WH_TYP, string EQP_TYP, string PLC_NO, ref string pRtnMsg)
        {
            string strTitle = "[Communication]";

            try
            {
                Wrk._pBdb.BeginTrans();

                string strSql = "";
                string CRLF = "\r\n";
                int nSelCnt;

                //MakeMsg("PLC 통신 OK", m_nThNo);

                strSql = "";
                strSql += CRLF + "UPDATE EQP_MST                                    ";
                strSql += CRLF + "   SET CONNECTED_YN      = :CONNECTED_YN          ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    EQP_TYP           = :EQP_TYP               ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";

                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();
                Wrk._pBdb.mComMain.Parameters.Add("CONNECTED_YN", DbLang.VARCHAR).Value = CONNECTED_YN;
                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = WH_TYP;
                Wrk._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = EQP_TYP;
                Wrk._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = PLC_NO;
                nSelCnt = Wrk._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "PLC정보 변경중 ERROR. ErrorMsg [" + Wrk._pBdb.ErrMsg + "] WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "]  PLC_NO [" + PLC_NO + "]", m_nThNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "PLC정보 변경중 Data가 없습니다.WH_TYP [" + WH_TYP + "] EQP_TYP [" + EQP_TYP + "] PLC_NO [" + PLC_NO + "] CONNECTED_YN [" + CONNECTED_YN + "]", m_nThNo);
                    return false;
                }

                Wrk._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Wrk._pBdb.Rollback();
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                pRtnMsg = strTitle + "Exception Error" + ex.Message;
                return false;
            }
        }
        #endregion

        #region [InsertWcsLogPgr] :: WCS_LOG_PGR에 LOG 남기기
        public bool InsertWcsLogPgr(string strTRACK_NO, string strLOG_MSG)
        {
            try
            {
                Wrk._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "INSERT INTO WCS_LOG_PGR (WH_TYP                 ";
                strSql += CRLF + "						  ,INS_DT                 ";
                strSql += CRLF + "						  ,LOG_SEQ                ";
                strSql += CRLF + "						  ,LUGG_NO                ";
                strSql += CRLF + "						  ,BCR_BOTTOM             ";
                strSql += CRLF + "						  ,BCR_TOP                ";
                strSql += CRLF + "						  ,PGR_NM                 ";
                strSql += CRLF + "						  ,LOG_KOR                ";
                strSql += CRLF + "						  ,TRACK_FROM             ";
                strSql += CRLF + "						  ,TRACK_TO               ";
                strSql += CRLF + "						  ,JOB_STA                ";
                strSql += CRLF + "						  ,RQ_INS_ID              ";
                strSql += CRLF + "						  ,RQ_INS_DT              ";
                strSql += CRLF + "						  ,EQP_TYP )              ";
                strSql += CRLF + "				VALUES    (:WH_TYP                ";
                strSql += CRLF + "						  ," + DbLang.SYSDATE + " ";
                strSql += CRLF + "						  ,NEXTVAL('LOG_SEQ')     ";
                strSql += CRLF + "						  ,NULL                   ";
                strSql += CRLF + "						  ,NULL                   ";
                strSql += CRLF + "						  ,NULL                   ";
                strSql += CRLF + "						  ,:PGR_NM                ";
                strSql += CRLF + "						  ,:LOG_KOR               ";
                strSql += CRLF + "						  ,NULL                   ";
                strSql += CRLF + "						  ,NULL                   ";
                strSql += CRLF + "						  ,:JOB_STA               ";
                strSql += CRLF + "						  ,:RQ_INS_ID             ";
                strSql += CRLF + "						  ," + DbLang.SYSDATE + " ";
                strSql += CRLF + "						  ,:EQP_TYP )             ";


                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();

                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWhTyp;
                Wrk._pBdb.mComMain.Parameters.Add("PGR_NM", DbLang.VARCHAR, 255).Value = m_strLogFileNm;
                Wrk._pBdb.mComMain.Parameters.Add("LOG_KOR", DbLang.VARCHAR, 255).Value = strLOG_MSG;
                Wrk._pBdb.mComMain.Parameters.Add("JOB_STA", DbLang.VARCHAR, 255).Value = "999";
                Wrk._pBdb.mComMain.Parameters.Add("RQ_INS_ID", DbLang.VARCHAR, 255).Value = m_strS2E_No;
                Wrk._pBdb.mComMain.Parameters.Add("EQP_TYP", DbLang.VARCHAR, 255).Value = m_strEqmtTyp;
                nSelCnt = Wrk._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    Wrk._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + Wrk._pBdb.ErrMsg + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + Wrk._pBdb.ErrMsg + "]", m_nThNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Wrk._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr]쓰기지시 후 상태값 변경중 DATA가 없습니다., S2E_NO [" + m_strS2E_No + "]");
                    MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 DATA가 없습니다.,S2E_NO [" + m_strS2E_No + "]", m_nThNo);
                    return false;

                }

                Wrk._pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Wrk._pBdb.Rollback();
                SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + ex.ToString() + "]");
                MakeMsg_Error("[InsertWcsLogPgr] 쓰기지시 후 상태값 변경중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + ex.ToString() + "]", m_nThNo);
                return false;
            }
        }
        #endregion

        #region [selectWc_Data_Cmd] :: WC의 CMD = 'Y' 인거 있는지 확인
        public bool selectWcDataCmd(string pWH_TYP, string pWC_NO, ref int pCmdCnt, ref string pRtnMsg)
        {
            string strTitle = "selectWcDataCmd";

            try
            {

                strSql = "";
                strSql += CRLF + "SELECT *                       ";
                strSql += CRLF + "  FROM WC_DATA                 ";
                strSql += CRLF + " WHERE WH_TYP = :WH_TYP        ";
                strSql += CRLF + "   AND WC_NO = :WC_NO          ";
                strSql += CRLF + "   AND CMD_RQ_ID = 'RQ'        ";
                strSql += CRLF + "   AND CMD_RQ_YN = 'Y'         ";
                strSql += CRLF + "   AND SUSPEND = '0'           ";

                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();

                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = pWH_TYP;
                Wrk._pBdb.mComMain.Parameters.Add("WC_NO", DbLang.VARCHAR, 255).Value = pWC_NO;
                nSelCnt = Wrk._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    SetErrorMsg("Comm" + m_nThNo + " : " + strTitle + " WC 읽기지시 조회 중 ERROR., WC_NO [" + pWC_NO + "]  MSG [" + Wrk._pBdb.ErrMsg + "]");
                    MakeMsg_Error(strTitle + "WC 읽기지시 조회 중 ERROR., WC_NO [" + pWC_NO + "]  MSG [" + Wrk._pBdb.ErrMsg + "]", m_nThNo);
                    return false;
                }

                pCmdCnt = nSelCnt;
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                pRtnMsg = strTitle + "Exception Error" + ex.Message;
                return false;
            }
        }
        #endregion

        #region [updateWcData] :: WC_DATA 무게값 업데이트
        public bool updateWcData(string WH_TYP, string PLC_NO, string WC_NO, string WEIGHT_READ_STA, string WEIGHT_RCV_VAL, ref string pRtnMsg, ref int nUpdCnt)
        {
            string strTitle = "[updateWcData]";

            try
            {
                Wrk._pBdb.BeginTrans();

                string strSql = "";
                string CRLF = "\r\n";
                int nSelCnt;

                // MakeMsg("PLC 통신 OK", m_nThNo);

                strSql = "";
                strSql += CRLF + "UPDATE WC_DATA                                    ";
                strSql += CRLF + "   SET WEIGHT_READ_STA   = :WEIGHT_READ_STA       "; //무게값 읽으면 RD,  WMS에 보고하고 나면 OK, 그외에는 ER
                strSql += CRLF + "      ,WEIGHT_RCV_VAL    = :WEIGHT_RCV_VAL        ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      ,UPD_USER_ID       = 'SERIAL_CONVERTOR'          ";
                strSql += CRLF + "      ,OD_RQ_ID          = 'RQ'                   "; //무게 지시 상태 '요청'
                strSql += CRLF + "      ,CMD_RQ_ID         = 'OK'                   "; //읽기 요청 상태 '완료'
                strSql += CRLF + "      ,CMD_RQ_YN         = 'N'                    "; //읽기 요청 여부 '대기'
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";
                strSql += CRLF + "AND    WC_NO             = :WC_NO                 ";
                strSql += CRLF + "AND    OD_RQ_ID          = 'OK'                   "; // 무게 지시 상태 '완료'

                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();
                Wrk._pBdb.mComMain.Parameters.Add("WEIGHT_READ_STA", DbLang.VARCHAR).Value = WEIGHT_READ_STA;
                Wrk._pBdb.mComMain.Parameters.Add("WEIGHT_RCV_VAL", DbLang.VARCHAR).Value = WEIGHT_RCV_VAL;
                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = WH_TYP;
                Wrk._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = PLC_NO;
                Wrk._pBdb.mComMain.Parameters.Add("WC_NO", DbLang.VARCHAR, 255).Value = WC_NO;
                nSelCnt = Wrk._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "WC 무게값 변경중 ERROR. ErrorMsg [" + Wrk._pBdb.ErrMsg + "] WH_TYP [" + WH_TYP + "] PLC_NO [" + PLC_NO + "] WC_NO [" + WC_NO + "] 무게값 [" + WEIGHT_RCV_VAL + "]", m_nThNo);
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "WC 무게값 변경중 Data가 없습니다. WH_TYP [" + WH_TYP + "] PLC_NO [" + PLC_NO + "] WC_NO [" + WC_NO + "] 무게값 [" + WEIGHT_RCV_VAL + "]", m_nThNo);
                    nUpdCnt = 0;
                    return true; //FALSE
                }

                Wrk._pBdb.Commit();
                nUpdCnt = 1;
                return true;
            }
            catch (Exception ex)
            {
                Wrk._pBdb.Rollback();
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                pRtnMsg = strTitle + "Exception Error" + ex.Message;
                return false;
            }
        }
        #endregion

        #region [InsertWcHis] :: WC 이력 남기기
        public bool InsertWcHis(string pWEIGHT_READ_STA, string pWEIGHT_RCV_VAL, string pREMARKS, ref string pRtnMsg)
        {
            try
            {

                Wrk._pBdb.BeginTrans();

                strSql = "";
                strSql += CRLF + "INSERT INTO WC_HIS (WH_TYP                 ";
                strSql += CRLF + "				     ,PLC_NO                 ";
                strSql += CRLF + "				     ,WC_NO                  ";
                strSql += CRLF + "				     ,WC_MC_NO               ";
                strSql += CRLF + "				     ,WEIGHT_READ_STA        ";
                strSql += CRLF + "				     ,WEIGHT_RCV_VAL         ";
                strSql += CRLF + "				     ,REMARKS                ";
                strSql += CRLF + "				     ,INS_DT                 ";
                strSql += CRLF + "				     ,INS_USER_ID)           ";
                strSql += CRLF + "		VALUES       (:WH_TYP                ";
                strSql += CRLF + "				     ,:PLC_NO                ";
                strSql += CRLF + "				     ,:WC_NO                 ";
                strSql += CRLF + "				     ,:WC_MC_NO              ";
                strSql += CRLF + "				     ,:WEIGHT_READ_STA       ";
                strSql += CRLF + "				     ,:WEIGHT_RCV_VAL        ";
                strSql += CRLF + "				     ,:REMARKS               ";
                strSql += CRLF + "				     ," + DbLang.SYSDATE + " ";
                strSql += CRLF + "				     ,'SERIAL_CONVERTOR')         ";


                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();

                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = m_strWhTyp;
                Wrk._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlcNo;
                Wrk._pBdb.mComMain.Parameters.Add("WC_NO", DbLang.VARCHAR, 255).Value = m_strS2E_No;
                Wrk._pBdb.mComMain.Parameters.Add("WC_MC_NO", DbLang.VARCHAR, 255).Value = m_strWcMcNo;
                Wrk._pBdb.mComMain.Parameters.Add("WEIGHT_READ_STA", DbLang.VARCHAR, 255).Value = pWEIGHT_READ_STA;
                Wrk._pBdb.mComMain.Parameters.Add("WEIGHT_RCV_VAL", DbLang.VARCHAR, 255).Value = pWEIGHT_RCV_VAL;
                Wrk._pBdb.mComMain.Parameters.Add("REMARKS", DbLang.VARCHAR, 255).Value = pREMARKS;
                nSelCnt = Wrk._pBdb.ExcuteNonQry(strSql);

                if (nSelCnt < 0)
                {
                    Wrk._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcHis] S2E 이력 입력중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + Wrk._pBdb.ErrMsg + "]");
                    MakeMsg_Error("[InsertWcHis] S2E 이력 입력중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + Wrk._pBdb.ErrMsg + "]", m_nThNo);
                    pRtnMsg = "[InsertWcHis] S2E 이력 입력중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + Wrk._pBdb.ErrMsg + "]";
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Wrk._pBdb.Rollback();
                    SetErrorMsg("Comm" + m_nThNo + " :[InsertWcHis] S2E 이력 입력중 DATA가 없습니다., S2E_NO [" + m_strS2E_No + "]");
                    MakeMsg_Error("[InsertWcHis] S2E 이력 입력중 DATA가 없습니다.,S2E_NO [" + m_strS2E_No + "]", m_nThNo);
                    pRtnMsg = "[InsertWcHis] S2E 이력 입력중 DATA가 없습니다.,S2E_NO [" + m_strS2E_No + "]";
                    return false;

                }

                Wrk._pBdb.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Wrk._pBdb.Rollback();
                SetErrorMsg("Comm" + m_nThNo + " :[InsertWcsLogPgr] S2E 이력 입력중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + ex.ToString() + "]");
                MakeMsg_Error("[InsertWcsLogPgr] S2E 이력 입력중 ERROR., S2E_NO [" + m_strS2E_No + "] MSG [" + ex.ToString() + "]", m_nThNo);
                pRtnMsg = ex.Message;
                return false;
            }
        }
        #endregion

        #region [updateWcDataCmd] :: WC_DATA의 CMD 업데이트
        public bool updateWcDataCmd(ref string pRtnMsg)
        {
            string strTitle = "[updateWcDataCmd]";

            try
            {
                Wrk._pBdb.BeginTrans();

                string strSql = "";
                string CRLF = "\r\n";
                int nSelCnt;

                strSql = "";
                strSql += CRLF + "UPDATE WC_DATA                                    ";
                strSql += CRLF + "   SET CMD_RQ_ID         = 'OK'                   ";
                strSql += CRLF + "      ,CMD_RQ_YN         = 'N'                    ";
                strSql += CRLF + "      ,UPD_DT            = " + DbLang.SYSDATE + " ";
                strSql += CRLF + "      ,UPD_USER_ID       = 'SERIAL_CONVERTOR'          ";
                strSql += CRLF + "WHERE  WH_TYP            = :WH_TYP                ";
                strSql += CRLF + "AND    PLC_NO            = :PLC_NO                ";
                strSql += CRLF + "AND    WC_NO             = :WC_NO                 ";

                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();
                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR).Value = m_strWhTyp;
                Wrk._pBdb.mComMain.Parameters.Add("PLC_NO", DbLang.VARCHAR, 255).Value = m_strPlcNo;
                Wrk._pBdb.mComMain.Parameters.Add("WC_NO", DbLang.VARCHAR, 255).Value = m_strS2E_No;
                nSelCnt = Wrk._pBdb.ExcuteNonQry(strSql);
                if (nSelCnt < 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "S2E CMD 변경중 ERROR. ErrorMsg [" + Wrk._pBdb.ErrMsg + "] S2E_NO [" + m_strS2E_No + "]", m_nThNo);
                    pRtnMsg = "S2E CMD 변경중 ERROR. ErrorMsg [" + Wrk._pBdb.ErrMsg + "] S2E_NO [" + m_strS2E_No + "]";
                    return false;
                }

                if (nSelCnt == 0)
                {
                    Wrk._pBdb.Rollback();
                    MakeMsg_Error(strTitle + "S2E CMD 변경중 Data가 없습니다.S2E_NO [" + m_strS2E_No + "]", m_nThNo);
                    pRtnMsg = "S2E CMD 변경중 Data가 없습니다.S2E_NO [" + m_strS2E_No + "]";
                    return false;
                }

                Wrk._pBdb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Wrk._pBdb.Rollback();
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                pRtnMsg = strTitle + "Exception Error" + ex.Message;
                return false;
            }
        }
        #endregion

        #region
        public bool ReadRequest(ref string pRtnMsg)
        {
            try
            {

                m_ByTxBuff[0] = 0x30; // 0
                m_ByTxBuff[1] = 0x31; // 1
                m_ByTxBuff[2] = 0x52; // R 52  M 4D, 
                m_ByTxBuff[3] = 0x57; // W     Z 5A
                m_ByTxBuff[4] = 0x0D; // CR  0x0D
                m_ByTxBuff[5] = 0x0A; // LF  0x0A

                //m_ByTxBuff[0] = 0x30; // 0
                //m_ByTxBuff[1] = 0x31; // 1
                //m_ByTxBuff[2] = 0x20; // SP
                //m_ByTxBuff[3] = 0x52; // R 52
                //m_ByTxBuff[4] = 0x57; // W
                //m_ByTxBuff[5] = 0x20; // SP
                //m_ByTxBuff[6] = 0x0D; // CR  0x0D
                //m_ByTxBuff[7] = 0x20; // SP
                //m_ByTxBuff[8] = 0x0A; // LF  0x0A
                char c0 = Convert.ToChar(48);
                char c1 = Convert.ToChar(49);
                char cR = Convert.ToChar(82);
                char cW = Convert.ToChar(87);
                char c0D = Convert.ToChar(13);
                char c0A = Convert.ToChar(10);

                Wrk.mstrSndTgm = c0 + "" + c1 + "" + cR + "" + cW + "" + c0D + "" + c0A + "";

                log.bSndTgm = Wrk.SndReqBCD(Wrk.mstrSndTgm, ref m_strRtnMsg);

                return true;
            }
            catch (Exception ex)
            {
                pRtnMsg = ex.Message;
                return false;
            }
        }
        #endregion

        #region [selectWc_Data_Suspend] :: WC의 SUSPEND 확인
        public bool selectWcDataSuspend(string pWH_TYP, string pWC_NO, ref int pCmdCnt, ref string pRtnMsg)
        {
            string strTitle = "selectWcDataCmd";

            try
            {

                strSql = "";
                strSql += CRLF + "SELECT *                       ";
                strSql += CRLF + "  FROM WC_DATA                 ";
                strSql += CRLF + " WHERE WH_TYP = :WH_TYP        ";
                strSql += CRLF + "   AND WC_NO = :WC_NO          ";
                strSql += CRLF + "   AND SUSPEND = '1'           ";

                Wrk._pBdb.mComMain.CommandType = CommandType.Text;
                Wrk._pBdb.mComMain.Parameters.Clear();

                Wrk._pBdb.mComMain.Parameters.Add("WH_TYP", DbLang.VARCHAR, 255).Value = pWH_TYP;
                Wrk._pBdb.mComMain.Parameters.Add("WC_NO", DbLang.VARCHAR, 255).Value = pWC_NO;
                nSelCnt = Wrk._pBdb.ExcuteQry(strSql);

                if (nSelCnt < 0)
                {
                    SetErrorMsg("Comm" + m_nThNo + " : " + strTitle + " WC 읽기지시 조회 중 ERROR., WC_NO [" + pWC_NO + "]  MSG [" + Wrk._pBdb.ErrMsg + "]");
                    MakeMsg_Error(strTitle + "WC 읽기지시 조회 중 ERROR., WC_NO [" + pWC_NO + "]  MSG [" + Wrk._pBdb.ErrMsg + "]", m_nThNo);
                    return false;
                }

                pCmdCnt = nSelCnt;
                return true;
            }
            catch (Exception ex)
            {
                MakeMsg_Error(strTitle + "Exception Error" + ex.Message, m_nThNo);
                pRtnMsg = strTitle + "Exception Error" + ex.Message;
                return false;
            }
        }
        #endregion
    }
}
