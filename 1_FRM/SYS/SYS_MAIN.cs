using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using log4net;
using log4net.Config;
using System.Linq;
using System.IO;

namespace SERIAL_CONVERTOR
{
    public delegate void PsMsgView(string pMsg, string pObjID, string pCommTyp, string pTgm, cDefApp.eLogMsgType pMsgTyp);
    public delegate void PfSetStatImgViewDB(PictureBox pPic, string pStatDbCn);
    public delegate void PfSetStatImgViewSOCKET(PictureBox pPic, string pStatSkt, string pStatOp);

    public partial class SYS_MAIN : Form
    {
        #region @@@.[Define]::���� �� ��ü

        string strRtnMsg = "";
        cThread[] m_thWcThread = new cThread[200]; //������ ��ü �迭.
        private string m_strRtnMsg = "";
        private maindefine m_mfgClass = new maindefine();

        public string m_strConnectString;
        public int m_nProcessCnt;

        // EtherNet ������� - Server Socket �̹Ƿ�
        //public string[] m_strPLC_NO = new string[200];
        //public string[] m_strEQMT_TYP = new string[200];
        //public string[] m_strCOMM_IP = new string[200];
        public string[] m_strCOMM_PORT = new string[200];
        public string[] m_strS2E_NO = new string[200];
        //public string[] m_strWC_MC_NO = new string[200];
        public string[] m_strLogPath = new string[200];
        public string[] m_strLogFileNm = new string[200];

        // Serial �������
        public string[] m_strCOMMPORT  = new string[200];
        public string[] m_strBAUDRATE  = new string[200];
        public string[] m_strSTOPBIT   = new string[200];
        public string[] m_strDATABIT   = new string[200];
        public string[] m_strPARITYBIT = new string[200];



        // JBY
        private Object thisLock = new Object();

        public string[] m_strSerialMsg = new string[200];
        public bool m_bSerialDataReceived = false;
        public int m_nSerialMsgCnt = 0;


        #endregion

        #region @@@.������
        public SYS_MAIN()
        {
            InitializeComponent();
        }
        #endregion

        #region @@@.[Event]SYS_MAIN_FormClosing
        private void SYS_MAIN_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cDefApp.GM_RE_START == false)
            {
                if (MessageBox.Show("�����Ͻðڽ��ϱ�?", "SERIAL_CONVERTOR", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cDefApp.GM_STAT_MAIN = false;
                    LogManager.Shutdown();
                    return;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                MessageBox.Show(this, "���α׷� : SERIAL_CONVERTOR \n���α׷��� �̹� ���� �� �Դϴ�.", "SERIAL_CONVERTOR");
                cDefApp.GM_STAT_MAIN = false;
                LogManager.Shutdown();
                return;
            }

        }
        #endregion

        #region @@@.[Event]SYS_MAIN_Load
        private void SYS_MAIN_Load(object sender, EventArgs e)
        {
            try
            {
                if (cCmLib.GfPrevInstance() == true)
                {
                    cDefApp.GM_RE_START = true;
                    Application.Exit();
                }

#if ORACLE
                cDefApi.GsGetInitPorFileDB_1(ref cDefApp.GM_DB1_PROVIDER, ref cDefApp.GM_DB1_ALIAS, ref cDefApp.GM_DB1_USERID, ref cDefApp.GM_DB1_PASSWORD, ref m_strRtnMsg);
                m_strConnectString = "Provider=" + cDefApp.GM_DB1_PROVIDER + "; Data Source=" + cDefApp.GM_DB1_ALIAS + "; User ID=" + cDefApp.GM_DB1_USERID + "; Password =" + cDefApp.GM_DB1_PASSWORD;
#endif
#if POSTGRESQL
            cDefApi.GsGetInitPorFileDB_2(ref cDefApp.GM_DB2_IP, ref cDefApp.GM_DB2_DATABASE, ref cDefApp.GM_DB2_PORT, ref cDefApp.GM_DB2_USER, ref cDefApp.GM_DB2_USER_PW, ref m_strRtnMsg);
                m_strConnectString = "host=" + cDefApp.GM_DB2_IP + ";username=" + cDefApp.GM_DB2_USER + ";password=" + cDefApp.GM_DB2_USER_PW + ";database=" + cDefApp.GM_DB2_DATABASE + ";MAXPOOLSIZE=50;";
#endif
#if SQL
#endif
                //CNF, PROCESS �о����
                cDefApi.GsGetInitPorFileCNF(ref cDefApp.GM_WH_TYP, ref cDefApp.GM_USERID, ref m_strRtnMsg);
                cDefApi.GsReadInitProfileProcessCnt("PROCESS", ref m_nProcessCnt, ref m_strRtnMsg);

                //@@.WC ��������ini CNT��ŭ �о����
                for (int ii = 0; ii < m_nProcessCnt; ii++)
                {
                    string Name = null;

                    Name = "E-COMM" + ii.ToString();
                    //@@.CV #1 ��������ini �о����
                    cDefApi.GsReadInitProfileEthernetCom(Name,
                                                 ref m_strCOMM_PORT[ii],
                                                 ref m_strS2E_NO[ii],
                                                 ref m_strLogPath[ii],
                                                 ref m_strLogFileNm[ii],
                                                 ref m_strRtnMsg);

                    Name = "S-COMM" + ii.ToString();
                    //@@.CV #1 ��������ini �о����
                    cDefApi.GsReadInitProfileSerialCom(Name,
                                                 ref m_strCOMMPORT[ii],
                                                 ref m_strBAUDRATE[ii],
                                                 ref m_strSTOPBIT[ii],
                                                 ref m_strDATABIT[ii],
                                                 ref m_strPARITYBIT[ii],
                                                 ref m_strLogPath[ii],
                                                 ref m_strLogFileNm[ii],
                                                 ref m_strRtnMsg);

                    // JBY ���Ͽ��� �ø��� ���� ���� �о� �ͼ� �ø��� ��Ʈ �����ϱ� 
                    string strFile = "";
                    ReadyToSerialPort(m_strCOMMPORT[ii], m_strBAUDRATE[ii]);     // ���ڰ��� �����̸� ������ ����!!!!
                                                                                 //SerialPort_Open();              // �ڵ� ���� �õ�;

                    if (m_strS2E_NO[ii] == "")
                    {
                        m_strS2E_NO[ii] = null;
                        break;
                    }

                    SetVisable(pnlTop, ii, "picWcDbCn" + ii.ToString(), "DB  Status #" + ii.ToString("00"));
                    SetVisable(pnlTop, ii, "picWcSkt" + ii.ToString(), "Socket  Status #" + ii.ToString("00"));
                    //
                    //SetVisableListView(tab, ii, "tabPage" + ii.ToString(), "TabPage #" + ii.ToString("00"));
                    //
                    SetDisplay(pnlTop, ii, "picWcDbCn" + ii.ToString(), "D");
                    SetDisplay(pnlTop, ii, "picWcSkt" + ii.ToString(), "D", "E");

                }

                // @@.��� ������ Ÿ���о����
                cDefApi.GsGetIntInitPorFile("DELAY", "SND", ref cDefApp.GM_COMM_SND_TIME_OUT, ref m_strRtnMsg); // @.����
                cDefApi.GsGetIntInitPorFile("DELAY", "RCV", ref cDefApp.GM_COMM_RCV_TIME_OUT, ref m_strRtnMsg); // @.����
                cDefApi.GsGetIntInitPorFile("DELAY", "ACK", ref cDefApp.GM_COMM_ACK_TIME_OUT, ref m_strRtnMsg); // @.�̻��

                // @@.���⼭ ���� ������ ����
                cDefApp.GM_STAT_MAIN = true; // @.���� �ý��� ���ۻ���
                WrkThStart();   // @.������ ����
            }
            catch (Exception ex)
            {
                MessageBox.Show("���α׷� ������ ����. Message [" + ex.ToString() + "]", "����", MessageBoxButtons.OK);
                Application.Exit();
            }
        }
        #endregion

        #region @@@.[Method]WrkThStart::������ ����
        private void WrkThStart()
        {
            CheckForIllegalCrossThreadCalls = false;
            Tm_ChkThread.Enabled = true;

            for (int ii = 0; ii < m_nProcessCnt; ii++)
            {
                //Conveyor ��� ������.
                m_thWcThread[ii] = new cThread(ii
                                             , cDefApp.GM_WH_TYP
                                             //, m_strEQMT_TYP[ii]
                                             //, m_strPLC_NO[ii]
                                             , m_strS2E_NO[ii]
                                             //, m_strWC_MC_NO[ii]
                                             //, m_strCOMM_IP[ii]
                                             , m_strCOMM_PORT[ii]
                                             , m_strConnectString
                                             , m_strLogFileNm[ii]);
            }
        }
        #endregion

        #region @@@.[Motod] @@@.������ ���¸� ȭ�鿡 ǥ��
        delegate void DelPfSetStatImgViewSocket(PictureBox pPic, string pStatSkt, string pStatOp);
        public void PfSetStatImgViewSocket(PictureBox pPic,
                                          string pStatSkt,
                                          string pStatOp)
        {
            // @.Stat Connection : C:����, T:�õ�, D:�񿬰�
            // @.Stat Operation : N:����, W:���, E:����
            try
            {
                if (pPic.InvokeRequired == true)
                {
                    DelPfSetStatImgViewSocket d = new DelPfSetStatImgViewSocket(this.PfSetStatImgViewSocket);
                    this.Invoke(d, pPic, pStatSkt, pStatOp);
                }
                else
                {
                    switch (pStatSkt + pStatOp)
                    {
                        case "CN": if (pPic.Tag.ToString() != "0") pPic.Image = this.imgLstStat.Images[0]; pPic.Tag = "0"; break;
                        case "CW": if (pPic.Tag.ToString() != "1") pPic.Image = this.imgLstStat.Images[1]; pPic.Tag = "1"; break;
                        case "CE": if (pPic.Tag.ToString() != "2") pPic.Image = this.imgLstStat.Images[2]; pPic.Tag = "2"; break;
                        case "TN": if (pPic.Tag.ToString() != "3") pPic.Image = this.imgLstStat.Images[3]; pPic.Tag = "3"; break;
                        case "TW": if (pPic.Tag.ToString() != "4") pPic.Image = this.imgLstStat.Images[4]; pPic.Tag = "4"; break;
                        case "TE": if (pPic.Tag.ToString() != "5") pPic.Image = this.imgLstStat.Images[5]; pPic.Tag = "5"; break;
                        case "DN": if (pPic.Tag.ToString() != "6") pPic.Image = this.imgLstStat.Images[6]; pPic.Tag = "6"; break;
                        case "DW": if (pPic.Tag.ToString() != "7") pPic.Image = this.imgLstStat.Images[7]; pPic.Tag = "7"; break;
                        case "DE": if (pPic.Tag.ToString() != "8") pPic.Image = this.imgLstStat.Images[8]; pPic.Tag = "8"; break;
                        default: break;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
            }
            return;
        }
        #endregion

        #region @@@.[Motod]PfSetStatImgView::DB���� ���¸� ȭ�鿡 ǥ��
        delegate void DelPfSetStatImgViewDB(PictureBox pPic, string pStatDbCn);
        public void PfSetStatImgViewDB(PictureBox pPic,
                                      string pStatDbCn)
        {
            // @.Stat Connection : C:����, T:�õ�, D:�񿬰�
            try
            {
                if (pPic.InvokeRequired == true)
                {
                    DelPfSetStatImgViewDB d = new DelPfSetStatImgViewDB(this.PfSetStatImgViewDB);
                    this.Invoke(d, pPic, pStatDbCn);
                }
                else
                {
                    switch (pStatDbCn)
                    {
                        case "C": if (pPic.Tag.ToString() != "0") pPic.Image = this.ImgLstBkgStat.Images[0]; pPic.Tag = "0"; break;
                        case "T": if (pPic.Tag.ToString() != "1") pPic.Image = this.ImgLstBkgStat.Images[1]; pPic.Tag = "1"; break;
                        case "D": if (pPic.Tag.ToString() != "2") pPic.Image = this.ImgLstBkgStat.Images[2]; pPic.Tag = "2"; break;
                        default: break;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
            }
            return;
        }
        #endregion

        #region @@@.ListView�� �α�[PsMsgView();]
        // @@@.�븮�� ����
        delegate void DelegateListViewItem(ListViewItem item, cDefApp.eLogWriteGbn eThGbn);

        // @@@.Client �޼��� Listview Invoke ����
        // @@@.Client �޼��� Listview Invoke ����
        private void PsSetMsg(ListViewItem item, cDefApp.eLogWriteGbn eThGbn)
        {
            try
            {
                string strCtrlName = "";
                if (eThGbn == cDefApp.eLogWriteGbn.COMM1)
                    strCtrlName = "lsvCOMM1";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM2)
                    strCtrlName = "lsvCOMM2";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM3)
                    strCtrlName = "lsvCOMM3";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM4)
                    strCtrlName = "lsvCOMM4";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM5)
                    strCtrlName = "lsvCOMM5";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM6)
                    strCtrlName = "lsvCOMM6";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM7)
                    strCtrlName = "lsvCOMM7";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM8)
                    strCtrlName = "lsvCOMM8";
                else if (eThGbn == cDefApp.eLogWriteGbn.COMM9)
                    strCtrlName = "lsvCOMM9";
                else
                    strCtrlName = "";

                Control Ctrl = PfCtlFind1(splBodySkt.Panel1, strCtrlName);

                if (Ctrl == null) return;

                ListView lstView = (ListView)Ctrl;

                if (lstView.InvokeRequired == true)
                {
                    DelegateListViewItem d = new DelegateListViewItem(this.PsSetMsg); // SetListview
                    this.Invoke(d, item, eThGbn);
                }
                else
                {
                    lstView.Items.Add(item);
                    if (lstView.Items.Count > 500)
                    {
                        lstView.Items.RemoveAt(0);
                    }

                    if (this.chkShow.Checked == true)
                    {
                        lstView.EnsureVisible(lstView.Items.Count - 1);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        //@@@.PsMsgView[ȭ�鿡 �α�...]
        private void PsMsgView(string pMsg,
                               string pObjID,
                               string pCommTyp,
                               string pTgm,
                  cDefApp.eLogMsgType pMsgTyp)
        {
            try
            {
                cDefApp.stutLogMsgInfo LogMsg;
                LogMsg.Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffffff");
                LogMsg.MsgTyp = pMsgTyp.ToString();
                LogMsg.ID = pObjID;
                LogMsg.Com = pCommTyp;
                LogMsg.Msg = pMsg;
                LogMsg.Tgm = pTgm;
                if (chkStopLog.Checked) return;
                ListViewItem vItem = new ListViewItem(LogMsg.Time, 0);
                vItem.SubItems.Add(LogMsg.ID);
                vItem.SubItems.Add(LogMsg.Com);
                vItem.SubItems.Add(LogMsg.Msg);
                vItem.SubItems.Add(LogMsg.Tgm);
                switch (pMsgTyp)
                {
                    case cDefApp.eLogMsgType.MSG_IMP: vItem.BackColor = Color.Blue; vItem.ForeColor = Color.White; break;
                    case cDefApp.eLogMsgType.MSG_ERR: vItem.BackColor = Color.Red; vItem.ForeColor = Color.White; break;
                    default: vItem.BackColor = Color.White; vItem.ForeColor = Color.Black; break;

                }
                //this.PsSetMsg(vItem, (cDefApp.eLogWriteGbn)nThGbn);
                return;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region @@@.[Event]btnDelLog_Click::ȭ��α� ����
        private void btnDelLog_Click(object sender, EventArgs e)
        {
            this.lsvCOMM1.Items.Clear();
            this.txtMsg.Text = "";
            this.txtTgm.Text = "";
        }
        #endregion

        #region @@@.[Event]btnDelLog_Click::�α� �� ����
        private void lsvMsg_Click(object sender, EventArgs e)
        {
            try
            {
                this.txtMsg.Text = this.lsvCOMM1.SelectedItems[0].SubItems[3].Text;
                this.txtTgm.Text = this.lsvCOMM1.SelectedItems[0].SubItems[4].Text;
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region @@@.[Event]tsbEnd_Click::���α׷� ����
        private void tsbEnd_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        public Control FindControl(string pCtlNm)
        {
            Control[] ctl;

            try
            {
                ctl = pnlTop.Controls.Find(pCtlNm, true);

                //ctl  = cDefApp.GM_MC_FRM.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    ctl = pnlTop.Controls.Find(pCtlNm, true);
                    if (ctl.Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return ctl[0];
                    }
                }
                else
                {
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
            }
            return null;

        }

        private void Tm_ChkThread_Tick(object sender, EventArgs e)
        {
            try
            {
                Tm_ChkThread.Enabled = false;

                for (int ii = 0; ii < m_nProcessCnt; ii++)
                {
                    if (m_thWcThread[ii].m_thThread == null)
                    {
                        SetDisplay(pnlTop, ii, "picWcSkt" + ii.ToString(), "T");
                        SetDisplay(pnlTop, ii, "picWcDbCn" + ii.ToString(), "T");
                        //SetVisableListView(tab, ii, "tabPage" + ii.ToString(), "TabPage #" + ii.ToString("00"));

                        m_thWcThread[ii].m_thThread = new Thread(m_thWcThread[ii].Thread_Doing);
                        m_thWcThread[ii].m_thThread.IsBackground = true;
                        m_thWcThread[ii].m_frmMain = this;
                        m_thWcThread[ii].m_thThread.Start();

                        Thread.Sleep(100);
                    }
                    else
                    {
                        if (m_thWcThread[ii].IsOpen)
                        {
                            SetDisplay(pnlTop, ii, "picWcSkt" + ii.ToString(), "C");
                            SetDisplay(pnlTop, ii, "picWcDbCn" + ii.ToString(), "C");
                            //SetVisableListView(tab, ii, "tabPage" + ii.ToString(), "TabPage #" + ii.ToString("00"));
                        }
                    }
                }
                Tm_ChkThread.Enabled = true;
            }
            catch (Exception ex)
            {
                Tm_ChkThread.Enabled = true;
            }
        }

        #region SetVisable, SetDisplay
        private void SetVisable(Panel obj, int ii, string ctrName, string tipname)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;

            ctrl = PfCtlFind(ref obj, ctrName);
            if (ctrl == null)
            {
                return;
            }

            FindPictureBox = ctrl as PictureBox;
            this.ToolTip.SetToolTip(FindPictureBox, tipname);
            FindPictureBox.Visible = true;
        }
        private void SetVisableListView(TabControl obj, int ii, string ctrName, string tipname)
        {
            Control ctrl;
            //PictureBox FindPictureBox = null;
            TabPage FindTabPage = null;

            string msg = null;
            ctrl = m_mfgClass.PfCtlFindTab(ref obj, ctrName, ref msg);
            if (ctrl == null)
            {
                return;
            }

            FindTabPage = ctrl as TabPage;
            this.ToolTip.SetToolTip(FindTabPage, tipname);
            FindTabPage.Visible = true;
        }
        private void SetDisplay(Panel obj, int ii, string ctrName, params string[] opt)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;


            ctrl = PfCtlFind(ref obj, ctrName);
            if (ctrl == null)
            {
                return;
            }

            FindPictureBox = ctrl as PictureBox;

            if (opt.Length == 1)
                PfSetStatImgView(FindPictureBox, opt[0]);
            else
                PfSetStatImgView(FindPictureBox, opt[0], opt[1]);
        }

        private void SetDisplay(Panel obj, int ii, string ctrName, string opt)
        {
            Control ctrl;
            PictureBox FindPictureBox = null;

            ctrl = PfCtlFind(ref obj, ctrName);
            if (ctrl == null)
            {
                return;
            }

            FindPictureBox = ctrl as PictureBox;

            PfSetStatImgView(FindPictureBox, opt);
        }
        #endregion SetVisable, SetDisplay

        #region [Method]::PfCtlFind::��Ʈ�� ��üã�� �Լ�
        public Control PfCtlFind(ref Panel pPnl, string pCtlNm)
        {
            strRtnMsg = "PfCtlFind::";
            Control[] ctl;
            try
            {
                strRtnMsg = "";

                ctl = pPnl.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    return null;
                }
                else
                {
                    strRtnMsg += "Success::" + "(" + pCtlNm + ")";
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
                strRtnMsg += ex.Message;
            }
            strRtnMsg += "(" + pCtlNm + ")";
            return null;
        }

        public Control PfCtlFind1(SplitterPanel pPnl, string pCtlNm)
        {
            strRtnMsg = "PfCtlFind::";
            Control[] ctl;
            try
            {
                strRtnMsg = "";

                ctl = pPnl.Controls.Find(pCtlNm, true);

                if (ctl.Length == 0)
                {
                    return null;
                }
                else
                {
                    strRtnMsg += "Success::" + "(" + pCtlNm + ")";
                    return ctl[0];
                }
            }
            catch (Exception ex)
            {
                strRtnMsg += ex.Message;
            }
            strRtnMsg += "(" + pCtlNm + ")";
            return null;
        }
        #endregion

        #region[Motod] @@@.������ ���¸� ȭ�鿡 ǥ��
        private bool PfSetStatImgView(PictureBox pPic,
                                          string pStatSkt,
                                          string pStatOp)
        {
            // @.Stat Connection : C:����, T:�õ�, D:�񿬰�
            // @.Stat Operation : N:����, W:���, E:����
            try
            {
                switch (pStatSkt + pStatOp)
                {
                    case "CN": if (pPic.Tag.ToString() != "0") pPic.Image = this.imgLstStat.Images[0]; pPic.Tag = "0"; break;
                    case "CW": if (pPic.Tag.ToString() != "1") pPic.Image = this.imgLstStat.Images[1]; pPic.Tag = "1"; break;
                    case "CE": if (pPic.Tag.ToString() != "2") pPic.Image = this.imgLstStat.Images[2]; pPic.Tag = "2"; break;
                    case "TN": if (pPic.Tag.ToString() != "3") pPic.Image = this.imgLstStat.Images[3]; pPic.Tag = "3"; break;
                    case "TW": if (pPic.Tag.ToString() != "4") pPic.Image = this.imgLstStat.Images[4]; pPic.Tag = "4"; break;
                    case "TE": if (pPic.Tag.ToString() != "5") pPic.Image = this.imgLstStat.Images[5]; pPic.Tag = "5"; break;
                    case "DN": if (pPic.Tag.ToString() != "6") pPic.Image = this.imgLstStat.Images[6]; pPic.Tag = "6"; break;
                    case "DW": if (pPic.Tag.ToString() != "7") pPic.Image = this.imgLstStat.Images[7]; pPic.Tag = "7"; break;
                    case "DE": if (pPic.Tag.ToString() != "8") pPic.Image = this.imgLstStat.Images[8]; pPic.Tag = "8"; break;
                    default: break;
                }
                return true;
            }
            catch (Exception ex)
            {
                string msg;
                msg = ex.Message;
            }
            return false;
        }
        #endregion

        #region[Motod] @@@.DB���� ���¸� ȭ�鿡 ǥ��
        private bool PfSetStatImgView(PictureBox pPic,
                                      string pStatDbCn)
        {
            // @.Stat Connection : C:����, T:�õ�, D:�񿬰�

            try
            {
                switch (pStatDbCn)
                {
                    case "C": if (pPic.Tag.ToString() != "0") pPic.Image = this.ImgLstBkgStat.Images[0]; pPic.Tag = "0"; break;
                    case "T": if (pPic.Tag.ToString() != "1") pPic.Image = this.ImgLstBkgStat.Images[1]; pPic.Tag = "1"; break;
                    case "D": if (pPic.Tag.ToString() != "2") pPic.Image = this.ImgLstBkgStat.Images[2]; pPic.Tag = "2"; break;
                    default: break;
                }

                return true;
            }
            catch (Exception ex)
            {
                string msg;
                msg = ex.Message;
            }
            return false;
        }
        #endregion

        //@@@.PsMsgView[ȭ�鿡 �α�...]
        public void PsMsgView_IMP(string pMsg, string pObjID, int nThGbn)
        {
            PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_IMP, nThGbn);
        }
        public void PsMsgView_Error(string pMsg, string pObjID, int nThGbn)
        {
            PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_ERR, nThGbn);
        }
        public void PsMsgView(string pMsg, string pObjID, int nThGbn)
        {
            PsMsgView(pMsg, pObjID, "", "", cDefApp.eLogMsgType.MSG_NOR, nThGbn);
        }

        private void PsMsgView(string pMsg,
                               string pObjID,
                               string pCommTyp,
                               string pTgm,
                  cDefApp.eLogMsgType pMsgTyp,
                               int nThGbn)
        {
            try
            {

                if (chkStopLog.Checked) return;

                cDefApp.stutLogMsgInfo LogMsg;
                LogMsg.Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffffff");
                LogMsg.MsgTyp = pMsgTyp.ToString();
                LogMsg.ID = pObjID;
                LogMsg.Com = pCommTyp;
                LogMsg.Msg = pMsg;
                LogMsg.Tgm = pTgm;
                if (chkStopLog.Checked) return;
                ListViewItem vItem = new ListViewItem(LogMsg.Time, 0);
                vItem.SubItems.Add(LogMsg.ID);
                vItem.SubItems.Add(LogMsg.Com);
                vItem.SubItems.Add(LogMsg.Msg);
                vItem.SubItems.Add(LogMsg.Tgm);
                switch (pMsgTyp)
                {
                    case cDefApp.eLogMsgType.MSG_IMP: vItem.BackColor = Color.Blue; vItem.ForeColor = Color.White; break;
                    case cDefApp.eLogMsgType.MSG_ERR: vItem.BackColor = Color.Red; vItem.ForeColor = Color.White; break;
                    default: vItem.BackColor = Color.White; vItem.ForeColor = Color.Black; break;

                }
                this.PsSetMsg(vItem, (cDefApp.eLogWriteGbn)nThGbn);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lsvCOMM1_Click(object sender, EventArgs e)
        {
            try
            {
                this.txtMsg.Text = this.lsvCOMM1.SelectedItems[0].SubItems[3].Text;
                this.txtTgm.Text = this.lsvCOMM1.SelectedItems[0].SubItems[4].Text;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    this.Invoke(new EventHandler(SerialPort_DataReceived_InMainThread));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }
        private void SerialPort_DataReceived_InMainThread(object s, EventArgs e)
        {
            string inStream = "";
            string indata = serialPort1.ReadExisting();
            string _inData = indata;
            char[] arr = indata.ToCharArray();

            if (arr.Length <= 0)
                return;

            if (arr[0] == cDefApp.STX)
                inStream = indata;
            else
                inStream += indata;

            string[] splited = inStream.Split((char)cDefApp.STX);

            SerialCommand(inStream);

            return;
            
            foreach (string sp in splited)
            {
                char[] arr2 = sp.ToCharArray();

                if (arr2.Length <= 0)
                    continue;

                int etxIndex = Array.IndexOf(arr2, (char)cDefApp.ETX);

                if (etxIndex <= 0)
                    continue;


                var arr3 = arr2.Skip(0).Take(etxIndex).ToArray();
                //var arr3 = arr2.SubArray(0, etxIndex);
                string cmd = new string(arr3);

                SerialCommand(cmd);
            }
        }
        public void SerialCommand(string cmd)
        {

            m_bSerialDataReceived = true;
            m_strSerialMsg[m_nSerialMsgCnt++] = cmd;
            
            //string strTemp = "[SerialCommand]...  " + cmd;

            //this.PsMsgView(strTemp, "", 0);     // �ϴ� ������ �ֱ� 

            // ���� ���� ó�� - ������ �޼��� �ڽ��� ���� �뵵 
            //MessageBox.Show(cmd + "\n 1. ��Ĺ���� �ش� ������ �������־�� �մϴ�.\n 2. �ø����� �����Ҽ��ִ� ���� �����ؾ� �մϴ�. ");


            // ���߿��� �� ������ Socket���� ������ ������ �ؾ���!
            // Serial2Socket(cmd);
        }
        private void ReadyToSerialPort( string strCOMMPORT  
                                      , string strBAUDRATE = "9600"  
                                    //, string strSTOPBIT = "1"  
                                    //, string strDATABIT = "8"  
                                    //, string strPARITYBIT ="0"
            )
        {



            try
            {
                if (strCOMMPORT == "")
                {
                    // �⺻������... 
                    serialPort1.PortName = "COM5";
                    serialPort1.BaudRate = Convert.ToInt32("9600");
                }
                else
                {
                    serialPort1.PortName = strCOMMPORT;
                    serialPort1.BaudRate = Convert.ToInt32(strBAUDRATE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }
        public bool SerialPort_Open(ref string strRtnMsg)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();

                    //txtSerialConStatus.Text = (serialPort1.IsOpen) ? "Conneted" : "Not Connected";
                    //lstSerialReceive.Items.Add(strTemp + "\t\t" + serialPort1.PortName.ToString() + " is Open !!");
                    //this.PsMsgView("", "", 0);
                    //m_bSerialCon = true;
                    strRtnMsg = serialPort1.PortName.ToString() + " is Open !!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    //lstSerialReceive.Items.Add(strTemp + "\t\t" + serialPort1.PortName.ToString() + " is Open Fail !!!!!!!!!!!!");
                    //m_bSerialCon = false;
                    strRtnMsg = serialPort1.PortName.ToString() + " is Open Fail !!!!!!!!!!";
                    return false;
                    throw;
                }
            }
            return true;
        }

        public void SendSerialData(string data)
        {
            //string strOutputData = (char)cDefApp.STX + data + (char)cDefApp.ETX;
            string strOutputData = data;

            if (serialPort1.IsOpen)
            {
                serialPort1.Write(strOutputData);
            }
            else
            {
                try
                {
                    serialPort1.Open();
                    serialPort1.Write(strOutputData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw;
                }
            }
        }

    }
}
  