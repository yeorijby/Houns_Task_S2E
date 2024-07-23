/* --------------------------------------------------------------------------------------------------------------------*/
/*	Modifier	:	KIM JUNG HWAN
/*	Update		:	2016�� 9�� 27��
/*	Changes		:	ProcBCRReqAckData �޼ҵ� �߰�
/*	Comment		:   BCR 71 �α� ��ȭ (TrayNo �߰�)
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
        #region @@@.[Define]���� �� ��ü����
        // @@@.����
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

        public string mStatComm = "D";                       // @.M/C�� ����ϴ� ������ �������[C:����, T:�õ�, D:�񿬰�]
        public string mStatOp = "W";                         // @.M/C�� ����ϴ� ������ ���ۻ���[N:����, W:���, E:����]

        public string mCOM_STA = "";
        private DataTable mDtReqIF = new DataTable();                  // @.DataTable[��û]
        public string mRcvTgm = "";                           // @.����Tgm
        public string mSndTgm = "";                           // @.����Tgm
        public string mMsg = "";                                   // @.�޼���

        // @@.���Ͽ��� ���Ǵ� ��������
        public TcpListener mListener;                           // @.���ϸ����ʰ�ü����
        public Socket mSkt;                                    // @.���ϰ�ü����
        public int mReSndCnt = 3;                              // @.������Ƚ��
        public byte[] mByRxBuff;                               // @.���Ź���
        public byte[] mByRxBuffDum;                             // @.���Ź���[�����б�]
        public byte[] mByTxBuff;                                // @.���۹���
        public byte[] mReqTxBuff;                               // @.���۵����͹���

        //post db ��������
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

                strRtnMsg = strTitle + "���� ó�� �Ǿ����ϴ�.";
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
                strRtnMsg = "[Close] Socket Close�� Socket Exception Message [" + sex.ToString() + "]";
                return;
            }
            catch (Exception ex)
            {
                strRtnMsg = "[Close] Socket Close�� Socket Exception Message [" + ex.ToString() + "]";
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

        #region @@@.[�Ӽ�]SktConnected
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

                // @.���������� ����
                mListener.Start();

                //**[������]
                //**�����޼��� ȣ���� ���ǰ� �ִٸ� ����� ���������� ��ٸ�
                //**1�� ���� �� ����ȭ�� ������ �� ������.
                //**��ȣ�� ������ True ������ False
                //**1���Ŀ� MyListenExitEvent.Set() ������ ���ѷ���
                while (!ListenExitEvent.WaitOne(pTmOutMil, false))
                {
                    // @.SYSTME ����� ������ ����[�ý��� ���Ῡ�� üũ]
                    if (cDefApp.GM_STAT_MAIN == false)
                    {
                        mListener.Stop();
                        throw new Exception();
                    }

                    if (mListener.Pending() == true) // @.������ ���������� Ȯ��
                    {
                        mSkt = mListener.AcceptSocket();

                        // @������ ��Ʈ�� ����
                        NetworkStream NetStream = new NetworkStream(mSkt);

                        // @.MySocket�� MyListener Ip, Port �� �������� ����
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

        #region @@@.[Metod]SktClose::���� �ݱ�
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
                    mSkt = null; // @.���α׷������� ��������� ���� ��ü ��뿩�� �Ǵ�
                }

                m_bSocCon = false;
                strRtnMsg = strTitle + "���� ó���Ǿ����ϴ�.";
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

        #region @@@.[Method]Snd:MC�� Tgm�� ����ó���Ѵ�.
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

                    // @.�� ���� ���� �� ��
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

        #region @@@.[Method]Rcv::PLC���� ������ Tgm�� ����ó���Ѵ�.
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

                // @.WCS <-> WMS(S) ��� ���� üũ
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

                    // @.WCS <- WMS(S) ��� ���� ���� Ȯ��
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

        #region @@@.[Method]RcvDum::���� ���̵����� �б�
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
                // @.WCS <-> WMS(S) ��� ���� üũ
                if (mSkt.Poll(1, SelectMode.SelectError) == true)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket has an error";
                    throw new Exception(strRtnMsg);
                }

                // @.WCS <- WMS(S) ��� ���� ���� Ȯ��
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

        #region @@@.[Metod]PLC�� BCD ���� ��ûTgm ����
        public bool SndReqBCD(string pSndTgm, ref string strRtnMsg)
        {
            string sSnd = "";

            try
            {
                strRtnMsg = "";
                mByTxBuff = System.Text.Encoding.Default.GetBytes(pSndTgm);

                if (Snd(mByTxBuff, mByTxBuff.Length, SocketFlags.None, false, cDefApp.GM_COMM_SND_TIME_OUT, ref strRtnMsg) == false) throw new Exception(strRtnMsg);

                strRtnMsg = "[SndReqBCD]::BCR�� BCD ��ûTgm ���� ����";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region @@@.[Metod]PLC�� BCD ���� ��ûTgm ����
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
        //        strRtnMsg = "[SndReqBCD]::BCR�� BCD ��ûTgm ���� ����";
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        #endregion

        #region @@@.[Method]RcvAck::PLC�� BCD���� ��ûTgm�� ���� Ack Tgm����
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

        //        // @.����Tgm �� �о�´�.[���н� ��������]
        //        if (Rcv(ref pRxBuff, pRxBuff.Length, SocketFlags.None, cDefApp.GM_COMM_ACK_TIME_OUT, ref  strRtnMsg) == false) throw new Exception(strRtnMsg);

        //        strRtnMsg = "[RcvAck]::BCR�� ��ûTgm�� ���� Ack Tgm���� ����";
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

        #region @@@.[Method]RcvBCD::BCR�� ��ûTgm�� ���� ����Tgm����
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

                // @.����Tgm �� �о�´�.[���н� ��������]
                if (Rcv(ref tmpBuff, tmpBuff.Length, SocketFlags.None, cDefApp.GM_COMM_RCV_TIME_OUT,ref nReadCnt, ref strRSTMsg, ref  strRtnMsg) == false)
                {
                    return false;
                }

                pRcvTgm = System.Text.Encoding.Default.GetString(tmpBuff);

                pRcvTgm = pRcvTgm.Substring(0, nReadCnt);

                this.mByRxBuff = System.Text.Encoding.Default.GetBytes(pRcvTgm);
                strRSTMsg = "0";
                strRtnMsg = "[RcvBCD]::BCR�� ��ûTgm�� ���� ����Tgm���� ����";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region @@@.[Mehod]ChkAckTgm::���� Ack Tgm üũ
        public int ChkAckTgm(ref string strRtnMsg)
        {
            try
            {
                strRtnMsg = "";
                mRcvTgm = System.Text.Encoding.Default.GetString(mByRxBuff);

                if (mRcvTgm.Substring(0, 1) != cDefApp.GM_STR_STX.ToString())
                {
                    strRtnMsg = "[ChkAckTgm]::STX����...";
                    return 1;
                }

                if (mRcvTgm.Substring(mRcvTgm.Length - 1, 1) != cDefApp.GM_STR_ETX.ToString())
                {
                    strRtnMsg = "[ChkAckTgm]::ETX����...";
                    return 2;
                }

                strRtnMsg = "[ChkAckTgm]::Ack Data ����...";
                return 0;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = "[ChkAckTgm]::����Tgmüũ����::" + strRtnMsg;
            return 99;
        }
        #endregion

        #region @@@.[Mehod]ChkBCDTgm::���� BCD Tgm üũ
        public int ChkBCDTgm(ref string strRtnMsg)
        {
            string strTitle = "[ChkBCDTgm]";
            try
            {
                strRtnMsg = "";

                if (mRcvTgm.Substring(0, 1) != cDefApp.GM_STR_STX.ToString())
                {
                    strRtnMsg = strTitle + "STX����.";
                    return 1;
                }

                if (mRcvTgm.Substring(mRcvTgm.Length - 1, 1) != cDefApp.GM_STR_ETX.ToString())
                {
                    strRtnMsg = strTitle + "ETX����.";
                    return 2;
                }

                strRtnMsg = strTitle + "BCD Data ����.";
                return 0;
            }
            catch (Exception ex)
            {
                strRtnMsg = ex.Message;
            }
            strRtnMsg = strTitle + "����Tgmüũ����" + strRtnMsg;
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
                // @.WCS <-> WMS(S) ��� ���� üũ
                if (mSkt.Poll(1, SelectMode.SelectError) == true)
                {
                    this.SktClose(ref strRtnMsg);
                    strRtnMsg = "[RcvDum]::This Socket has an error";
                    throw new Exception(strRtnMsg);
                }

                // @.WCS <- WMS(S) ��� ���� ���� Ȯ��
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
