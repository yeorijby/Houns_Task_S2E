/* --------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	KIM JUNG HWAN
/*	Update		:	2016년 9월 27일
/*	Changes		:	ProcBCRReqAckData 메소드 추가
/*	Comment		:   BCR 71 로그 강화 (TrayNo 추가)
/* --------------------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NpgsqlTypes;
using Npgsql;

namespace SERIAL_CONVERTOR
{
    class cWrkBcrSkt : cDefApp
    {
        #region @@@.[Define]변수 및 객체선언
        // @@@.선언
        public string m_strGrpTyp = "";
        public string m_strStrgTyp = "";
        public string m_strEqmtTyp = "";
        public string m_strGrpNo = "";
        public string m_strMcNo = "";
        public string m_strConnectString = "";
        public bool m_bDBOpen;
        public bool m_bSocCon;
        public bool m_bSerialCon = false;
        public string mstrSndTgm;

        public string m_strSpName = "";

        public string mStatComm = "D";                       // @.M/C와 통신하는 쓰레드 연결상태[C:연결, T:시도, D:비연결]
        public string mStatOp = "W";                         // @.M/C와 통신하는 쓰레드 동작상태[N:정상, W:대기, E:에러]

        public string mCOM_STA = "";
        private DataTable mDtReqIF = new DataTable();                  // @.DataTable[요청]
        public string mRcvTgm = "";                           // @.수신Tgm
        public string mSndTgm = "";                           // @.전송Tgm
        public string mMsg = "";                                   // @.메세지

        // @@.소켓에서 사용되는 변수선언
        public TcpListener mListener;                           // @.소켓리스너객체선언
        public Socket mSkt;                                    // @.소켓객체선언
        public int mReSndCnt = 3;                              // @.재전송횟수
        public byte[] mByRxBuff;                               // @.수신버퍼
        public byte[] mByRxBuffDum;                             // @.수신버퍼[더미읽기]
        public byte[] mByTxBuff;                                // @.전송버퍼
        public byte[] mReqTxBuff;                               // @.전송데이터버퍼

        //post db 변수선언
#if ORACLE
        public OleDbConnection _pConObj;
        public cDbUse _pBdb;
#endif
#if POSTGRESQL
        public NpgsqlConnection _pConObj;
        public cDbPostUse _pBdb;
#endif
        public bool _IsPostDBOpen;
        private string _strPostConnectionString;
        public bool IsPostDBOpen { get { return _IsPostDBOpen; } set { _IsPostDBOpen = value; } }
        public string PostConnectionString { get { return _strPostConnectionString; } set { _strPostConnectionString = value; } }

        private IPEndPoint _ipEndPoint;
        private int _time_out;
        private string _strIp;
        private int _nPort;
        public int time_out { get { return _time_out; } set { _time_out = value; } }
        public string Ip { get { return _strIp; } set { _strIp = value; } }
        public int Port { get { return _nPort; } set { _nPort = value; } }

        #endregion

        #region @@@.[New]
        public cWrkBcrSkt(string strGrpTyp
                        , string strEqmtTyp
                        , string strMcNo
                        , string strSpName
                        , string strConnectString)
        {
            m_strGrpTyp = strGrpTyp;
            m_strEqmtTyp = strEqmtTyp;
            m_strMcNo = strMcNo;
            m_strSpName = strSpName;
            m_strConnectString = strConnectString;
        }
        #endregion

        public void SetConfig(string server_ip, int server_port, int time_out)
        {
            Ip = server_ip;
            IPAddress serverIP = IPAddress.Parse(server_ip);
            _ipEndPoint = new IPEndPoint(serverIP, server_port);
            Port = server_port;
            _time_out = time_out;
        }

        #region @@@.[Metod]Db Connection Close
        public void DBLogOut(ref string strRtnMsg)
        {
            string strTitle = "[DBLogOut]";

            try
            {
                if (this.mDbWrk.mComMain != null)
                {
                    this.mDbWrk.mCnMain.Close();
                    this.mDbWrk.mCnMain.Dispose();
                }

                this.mDbWrk.mCnMain = null;
                this.mDbWrk.DbConnected = false;

                strRtnMsg = strTitle + "정상 처리 되었습니다.";
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.ToString();
            }
        }
        #endregion

        public bool Open(string strIp, int nPort, ref string strRtnMsg)
        {
            string strTitle = "[Open]";

            m_bDBOpen = false;

            try
            {
                if (!SktConnect(strIp, nPort, ref strRtnMsg))
                {
                    strRtnMsg = strTitle + strRtnMsg;
                    return false;
                }

                m_bSocCon = true;
                return true;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.ToString();
                _pConObj.Dispose();
                return false;
            }
        }

        #region Close
        public void Close(ref string strRtnMsg)
        {
            strRtnMsg = "PostDBClose::";

            try
            {
                if (m_bSocCon)
                {
                    string msg = "";
                    SktClose(ref msg);
                }

                if (m_bDBOpen)
                {
                    _pConObj.Close();
                    _pConObj.Dispose();
                    m_bDBOpen = false;
                }

                strRtnMsg = "[Close] Socket Close. Success"; 
            }
            catch (SocketException sex)
            {
                strRtnMsg = "[Close] Socket Close중 Socket Exception Message [" + sex.ToString() + "]";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = "[Close] Socket Close중 Socket Exception Message [" + ex.ToString() + "]";
                return;
            }
        }
        #endregion PostDBClose

        #region @@@.[Metod]Db Connection Open
        public bool DBLogIn(ref string strConnect, ref string strRtnMsg)
        {
            if (!cCmLib.GfDBLogIn(ref mDbWrk.mCnMain, ref strConnect, ref strRtnMsg))
            {
                return false;
            }

            mDbWrk.Init();
            return true;
        }
        #endregion

        #region @@@.[속성]SktConnected
        private bool mSktConnected = false;      // @.IsSktConnect
        public bool SktConnected
        {
            get
            {
                return mSktConnected;
            }
            set
            {
                mSktConnected = value;
            }
        }
        #endregion

        #region @@@.[Method]SktConnect
        public bool SktConnect(string pIPAdress,
                                  int pPort,
                           ref string pMsg)
        {
            string strTitle = "SktConnect";
            System.Net.IPEndPoint rEP;
            System.Net.IPAddress rIP;

            try
            {
                pMsg = "";

                rIP = System.Net.IPAddress.Parse(pIPAdress);
                rEP = new System.Net.IPEndPoint(rIP, pPort);
                mSkt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSkt.Connect(rEP);

                if (mSkt.Connected == false) throw new SocketException();

                mStatOp = "W";
                //this.m_bSocCon = true;
                pMsg = "[SktConnect]::Sucess-Socket Connected";
                return true;
            }
            catch (SocketException sex)
            {
                this.m_bSocCon = false;
                pMsg = strTitle + sex.ToString();
                return false;
            }
            catch (Exception ex)
            {
                pMsg = strTitle + ex.ToString();
                return false;
            }
        }

        #endregion

        #region @@@.[Metod]SktListen
        public bool SktListen(int pPort,
                              int pTmOutMil,
                       ref string pMsg)
        {
            AutoResetEvent ListenExitEvent = new AutoResetEvent(false);

            try
            {
                pMsg = "";
                if (mListener == null)
                {
                    mListener = new TcpListener(System.Net.IPAddress.Any, pPort);
                }

                // @.리슨스레드 시작
                mListener.Start();

                //**[스레드]
                //**공유메서드 호출이 사용되고 있다면 사용이 끝날때까지 기다림
                //**1초 지연 후 동기화된 도메인 을 버린다.
                //**신호가 있으면 True 없으면 False
                //**1초후에 MyListenExitEvent.Set() 없으면 무한루프
                while (!ListenExitEvent.WaitOne(pTmOutMil, false))
                {
                    // @.SYSTME 종료시 스레드 종료[시스템 종료여부 체크]
                    if (cDefApp.GM_STAT_MAIN == false)
                    {
                        mListener.Stop();
                        throw new Exception();
                    }

                    if (mListener.Pending() == true) // @.연결이 보류중인지 확인
                    {
                        mSkt = mListener.AcceptSocket();

                        // @소켓의 스트림 제공
                        NetworkStream NetStream = new NetworkStream(mSkt);

                        // @.MySocket의 MyListener Ip, Port 의 연결지점 형성
                        System.Net.IPEndPoint remoteEP = mSkt.RemoteEndPoint as System.Net.IPEndPoint;

                        SktConnect(ref pMsg);
                        break;
                    }
                    break;
                }
                return true;
            }
            catch (SocketException sex)
            {
                throw sex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region @@@.[Metodc]SktConnect
        public bool SktConnect(ref string pMsg)
        {
            pMsg = "";
            mStatOp = "W";
            this.SktConnected = true;
            pMsg = "[SktConnect]::Sucess-Socket Connected";
            return true;
        }
        #endregion

        #region @@@.[Metod]SktClose::소켓 닫기
        public bool SktClose(ref string strRtnMsg)
        {
            string strTitle = "[SktClose]";
            try
            {
                strRtnMsg = "";

                if (mSkt != null)
                {
                    if (mSkt.Connected == true)
                    {
                        mSkt.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    }

                    mSkt.Close();
                    mSkt = null; // @.프로그램내에서 강제종료시 소켓 객체 사용여부 판단
                }

                m_bSocCon = false;
                strRtnMsg = strTitle + "정상 처리되었습니다.";
                return true;
            }
            catch (Exception ex)
            {
                strRtnMsg = strTitle + ex.Message;
            }

            m_bSocCon = false;
            return false;
        }
        #endregion

        #region @@@.[Method]Snd:MC에 Tgm을 전송처리한다.
        private bool Snd(byte[] pTxBuff,
                            int pSize,
                    SocketFlags pFlag,
                           bool pReSnd,
                            int pTmOutMil,
                     ref string strRtnMsg)
        {
            int nSnd = 0;

            try
            {
                strRtnMsg = "";
                while (true)
                {
                    if (nSnd > mReSndCnt)
                    {
                        strRtnMsg = "[Snd]::Send Count Over " + mReSndCnt.ToString() + " Times";
                        return false;
                    }

                    nSnd += 1;

                    // @.재 정의 수정 할 것
                    if (mSkt.Send(pTxBuff, pSize, pFlag) < 1)
                    {
                        if (pReSnd == true)
                        {
                            Thread.Sleep(pTmOutMil);
                            continue;
                        }

                        strRtnMsg = "[Snd]::Send Time Out";
                        return false;
                    }
                    break;
                }
                strRtnMsg = "[Snd]::Success Send Socket";
                return true;
            }
            catch (SocketException se)
            {
                throw se;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region @@@.[Method]Rcv::PLC에서 전송한 Tgm을 수신처리한다.
        public bool Rcv(ref byte[] pRxBuff,
                               int pReadCnt,
                       SocketFlags pFlag,
                            double pTmOutMil,
                           ref int nRemain,
                        ref string strRSTMSG,
                        ref string strRtnMsg)
        {
            DateTime dtRcvTm;
            //int nRemain;
            byte[] TmpRxBuff;

            try
            {
                strRtnMsg = "";
                TmpRxBuff = new byte[pReadCnt];
                pRxBuff = new byte[pReadCnt];

                // @.WCS <-> WMS(S) 통신 에러 체크
                if (mSkt.Poll(1, SelectMode.SelectError) == true)
                {
                    strRtnMsg = "[Rcv]::This Socket has an error";
                    strRSTMSG = "2";
                    return false;
                }
                
                dtRcvTm = DateTime.Now.AddMilliseconds(pTmOutMil);

                while (true)
                {
                    if (dtRcvTm < DateTime.Now)
                    {
                        strRtnMsg = "[Rcv]::Time Out";
                        strRSTMSG = "1";
                        return false;
                    }

                    // @.WCS <- WMS(S) 통신 수신 정보 확인
                    if (mSkt.Poll(Convert.ToInt32(pTmOutMil), SelectMode.SelectRead) == false)
                    {
                        continue;
                    }

                    if (mSkt.Available == 0)
                    {
                        strRtnMsg = "[Rcv]::This Socket Disconnect";
                        strRSTMSG = "2";
                        return false;
                    }

                    break;

                    //if (mSkt.Available >= pReadCnt)
                    //{
                    //    break;
                    //}
                }

                nRemain = mSkt.Receive(TmpRxBuff, pReadCnt, SocketFlags.None);
                System.Buffer.BlockCopy(TmpRxBuff, 0, pRxBuff, 0, nRemain);
                strRtnMsg = "[Rcv]::Success Read Socket";
                return true;
            }
            catch (SocketException se)
            {
                throw se;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region @@@.[Method]RcvDum::소켓 더미데이터 읽기
        // Log of RcvDum
        public int RcvDum(ref byte[] pByRxBuff,
                   ref string strRtnMsg)
        {
            int nRemain;
            byte[] TmpRxBuff;
            int nDum = 0;

            try
            {
                strRtnMsg = "";
                // @.WCS <-> WMS(S) 통신 에러 체크
                if (mSkt.Poll(1, SelectMode.SelectError) == true)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket has an error";
                    throw new Exception(strRtnMsg);
                }

                // @.WCS <- WMS(S) 통신 수신 정보 확인
                if (mSkt.Poll(1, SelectMode.SelectRead) == false)
                {
                    if (mSkt.Poll(1, SelectMode.SelectWrite) == true)
                    {
                        string qeqef = "Qwd";
                    }
                    throw new Exception("");

                }

                if (mSkt.Available == 0)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket Disconnect";
                    throw new Exception(strRtnMsg);
                }
                else
                {
                    nDum = mSkt.Available;
                }

                TmpRxBuff = new byte[nDum];
                pByRxBuff = new byte[nDum];

                nRemain = mSkt.Receive(TmpRxBuff, nDum, SocketFlags.None);
                System.Buffer.BlockCopy(TmpRxBuff, 0, pByRxBuff, 0, nRemain);

                strRtnMsg = "[RcvDum]::Success Read Dummy[" + nDum.ToString() + "]";
                return nDum;
            }
            catch (SocketException se)
            {
                strRtnMsg = se.Message;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = "[RcvDum]::Fail Read Socket Dummy::" + strRtnMsg;
            return 0;
        }


        #endregion

        #region @@@.[Metod]PLC에 BCD 스켄 요청Tgm 전송
        public bool SndReqBCD(string pSndTgm, ref string strRtnMsg)
        {
            string sSnd = "";

            try
            {
                strRtnMsg = "";
                mByTxBuff = System.Text.Encoding.Default.GetBytes(pSndTgm);

                if (Snd(mByTxBuff, mByTxBuff.Length, SocketFlags.None, false, cDefApp.GM_COMM_SND_TIME_OUT, ref strRtnMsg) == false) throw new Exception(strRtnMsg);

                strRtnMsg = "[SndReqBCD]::BCR에 BCD 요청Tgm 전송 성공";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region @@@.[Metod]PLC에 BCD 스켄 요청Tgm 전송
        //public bool SndReqBCD(byte[] pByTxBuff, ref string strRtnMsg)
        //{
        //    string sSnd = "";
        //
        //    try
        //    {
        //        strRtnMsg = "";
        //        //mByTxBuff = System.Text.Encoding.Default.GetBytes(pSndTgm);
        //
        //        if (Snd(pByTxBuff, pByTxBuff.Length, SocketFlags.None, false, cDefApp.GM_COMM_SND_TIME_OUT, ref strRtnMsg) == false) throw new Exception(strRtnMsg);
        //
        //        strRtnMsg = "[SndReqBCD]::BCR에 BCD 요청Tgm 전송 성공";
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        #endregion

        #region @@@.[Method]RcvAck::PLC에 BCD상태 요청Tgm에 대한 Ack Tgm수신
        //public bool RcvAck(ref byte[] pRxBuff, 
        //                   int pReadByteLen,
        //                   ref string strRtnMsg)
        //{

        //    byte[] tmpBuff = new byte[pReadByteLen];

        //    pRxBuff = new byte[pReadByteLen];

        //    try
        //    {
        //        strRtnMsg = "";
        //        pRxBuff = new byte[pReadByteLen];

        //        // @.응답Tgm 을 읽어온다.[실패시 빠져나감]
        //        if (Rcv(ref pRxBuff, pRxBuff.Length, SocketFlags.None, cDefApp.GM_COMM_ACK_TIME_OUT, ref  strRtnMsg) == false) throw new Exception(strRtnMsg);

        //        strRtnMsg = "[RcvAck]::BCR에 요청Tgm에 대한 Ack Tgm수신 성공";
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        //public bool RcvValueChk(ref string strRtnMsg)
        //{


        //}

        #region @@@.[Method]RcvBCD::BCR에 요청Tgm에 대한 응답Tgm수신
        public bool RcvBCD(ref string pRcvTgm,
                                 char pEtx,
                                  int pReqEtxCnt,
                              ref int nReadCnt, 
                           ref string strRSTMsg,
                           ref string strRtnMsg)
        {

            //byte[] tmpBuff = new byte[1];
            byte[] tmpBuff = new byte[50];
            pRcvTgm = "";
            //int nRcvEtxCnt = 0;
            //mByRxBuff = new byte[0];
            mByRxBuff = new byte[50];

            try
            {
                strRtnMsg = "";

                // @.응답Tgm 을 읽어온다.[실패시 빠져나감]
                if (Rcv(ref tmpBuff, tmpBuff.Length, SocketFlags.None, cDefApp.GM_COMM_RCV_TIME_OUT,ref nReadCnt, ref strRSTMsg, ref  strRtnMsg) == false)
                {
                    return false;
                }

                pRcvTgm = System.Text.Encoding.Default.GetString(tmpBuff);

                pRcvTgm = pRcvTgm.Substring(0, nReadCnt);

                this.mByRxBuff = System.Text.Encoding.Default.GetBytes(pRcvTgm);
                strRSTMsg = "0";
                strRtnMsg = "[RcvBCD]::BCR에 요청Tgm에 대한 응답Tgm수신 성공";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region @@@.[Mehod]ChkAckTgm::수신 Ack Tgm 체크
        public int ChkAckTgm(ref string strRtnMsg)
        {
            try
            {
                strRtnMsg = "";
                mRcvTgm = System.Text.Encoding.Default.GetString(mByRxBuff);

                if (mRcvTgm.Substring(0, 1) != cDefApp.GM_STR_STX.ToString())
                {
                    strRtnMsg = "[ChkAckTgm]::STX에러...";
                    return 1;
                }

                if (mRcvTgm.Substring(mRcvTgm.Length - 1, 1) != cDefApp.GM_STR_ETX.ToString())
                {
                    strRtnMsg = "[ChkAckTgm]::ETX에러...";
                    return 2;
                }

                strRtnMsg = "[ChkAckTgm]::Ack Data 정상...";
                return 0;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = "[ChkAckTgm]::수신Tgm체크에러::" + strRtnMsg;
            return 99;
        }
        #endregion

        #region @@@.[Mehod]ChkBCDTgm::수신 BCD Tgm 체크
        public int ChkBCDTgm(ref string strRtnMsg)
        {
            string strTitle = "[ChkBCDTgm]";
            try
            {
                strRtnMsg = "";

                if (mRcvTgm.Substring(0, 1) != cDefApp.GM_STR_STX.ToString())
                {
                    strRtnMsg = strTitle + "STX에러.";
                    return 1;
                }

                if (mRcvTgm.Substring(mRcvTgm.Length - 1, 1) != cDefApp.GM_STR_ETX.ToString())
                {
                    strRtnMsg = strTitle + "ETX에러.";
                    return 2;
                }

                strRtnMsg = strTitle + "BCD Data 정상.";
                return 0;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = strTitle + "수신Tgm체크에러" + strRtnMsg;
            return 99;
        }
        #endregion

        public int RcvDum(byte[] pByRxBuff,
                   ref string strRtnMsg)
        {
            int nRemain;
            byte[] TmpRxBuff;
            int nDum = 0;

            try
            {
                strRtnMsg = "";
                // @.WCS <-> WMS(S) 통신 에러 체크
                if (mSkt.Poll(1, SelectMode.SelectError) == true)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket has an error";
                    throw new Exception(strRtnMsg);
                }

                // @.WCS <- WMS(S) 통신 수신 정보 확인
                if (mSkt.Poll(1, SelectMode.SelectRead) == false)
                {
                    if (mSkt.Poll(1, SelectMode.SelectWrite) == true)
                    {
                        string qeqef = "Qwd";
                    }
                    throw new Exception("");

                }

                if (mSkt.Available == 0)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket Disconnect";
                    throw new Exception(strRtnMsg);
                }
                else
                {
                    nDum = mSkt.Available;
                }

                TmpRxBuff = new byte[nDum];
                pByRxBuff = new byte[nDum];

                nRemain = mSkt.Receive(TmpRxBuff, nDum, SocketFlags.None);
                System.Buffer.BlockCopy(TmpRxBuff, 0, pByRxBuff, 0, nRemain);

                strRtnMsg = "[RcvDum]::Success Read Dummy[" + nDum.ToString() + "]";
                return nDum;
            }
            catch (SocketException se)
            {
                strRtnMsg = se.Message;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = "[RcvDum]::Fail Read Socket Dummy::" + strRtnMsg;
            return 0;
        }

    }
}
