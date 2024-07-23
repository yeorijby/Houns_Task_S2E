using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace WCS_TASK_WC
{
    partial class ASRS_BCR : ServiceBase
    {
        public System.Timers.Timer timer = new System.Timers.Timer();
        string strRtnMsg = "";
        cThread[] WrkComThWrk = new cThread[100]; //스레드 객체 배열.
        public ASRS_BCR()
        {
            InitializeComponent();
        }
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);


        protected override void OnStart(string[] args)
        {
            // TODO: 여기에 서비스를 시작하는 코드를 추가합니다.
            Log.Event("BCR 서비스 시작.");
            timer.Interval = 5000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);

            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            cDefApi.GsGetIntInitPorFile("DELAY", "SND", ref cDefApp.GM_COMM_SND_TIME_OUT, ref strRtnMsg);
            cDefApi.GsGetIntInitPorFile("DELAY", "ACK", ref cDefApp.GM_COMM_ACK_TIME_OUT, ref strRtnMsg);
            cDefApi.GsGetIntInitPorFile("DELAY", "RCV", ref cDefApp.GM_COMM_RCV_TIME_OUT, ref strRtnMsg);
            cDefApi.GsGetIntInitPorFile("DELAY", "READ", ref cDefApp.GM_COMM_READ, ref strRtnMsg);

            // @@.프로세스 스레드 갯수.
            cDefApi.GsGetIntInitPorFile("PROCESS", "CNT", ref cDefApp.GM_PROCESS_CNT, ref strRtnMsg);

            ////@@.DB접속정보ini 읽어오기
            cDefApi.GsGetStringInitPorFile("DB", "PROVIDER", ref cDefApp.GM_DB_PROVIDER, ref strRtnMsg);
            cDefApi.GsGetStringInitPorFile("DB", "ALIAS", ref cDefApp.GM_DB_ALIAS, ref strRtnMsg);
            cDefApi.GsGetStringInitPorFile("DB", "USERID", ref cDefApp.GM_DB_USERID, ref strRtnMsg);
            cDefApi.GsGetStringInitPorFile("DB", "PASSWORD", ref cDefApp.GM_DB_PASSWORD, ref strRtnMsg);

            //@@.POST DB접속정보ini 읽어오기
            cDefApi.GsGetInitPorFilePostDB(ref cDefApp.GM_PDB_IP, ref cDefApp.GM_PDB_PORT, ref cDefApp.GM_PDB_DATABASE, ref cDefApp.GM_PDB_USER, ref cDefApp.GM_PDB_USER_PW, ref strRtnMsg);

            WrkThStart();   // @.쓰레드 시작
            cDefApp.GM_STAT_MAIN = true; // @.메인 시스템 동작상태
            timer.Start();
        }

        #region @@@.[Method]WrkThStart::스레드 실행
        private void WrkThStart()
        {

            string strPostStrConnect = "host=" + cDefApp.GM_PDB_IP + ";username=" + cDefApp.GM_PDB_USER + ";password=" + cDefApp.GM_PDB_USER_PW + ";database=" + cDefApp.GM_PDB_DATABASE;

            for (int i = 1; i <= cDefApp.GM_PROCESS_CNT; i++)
            {
                WrkComThWrk[i] = new cThread(i);
                string strAppName = "COMM" + i.ToString();

                string strGrpTyp = "";
                string strEqmtTyp = "";
                string strMcNo = "";
                string strSpName = "";
                string strIp = "";
                string strPort = "";

                cDefApi.GsGetStringInitPorFile(strAppName, "GRP_TYP", ref strGrpTyp, ref strRtnMsg);
                cDefApi.GsGetStringInitPorFile(strAppName, "EQMT_TYP", ref strEqmtTyp, ref strRtnMsg);
                cDefApi.GsGetStringInitPorFile(strAppName, "MC_NO", ref strMcNo, ref strRtnMsg);
                cDefApi.GsGetStringInitPorFile(strAppName, "SP_NAME", ref strSpName, ref strRtnMsg);
                cDefApi.GsGetStringInitPorFile(strAppName, "IP", ref strIp, ref strRtnMsg);
                cDefApi.GsGetStringInitPorFile(strAppName, "PORT", ref strPort, ref strRtnMsg);

                WrkComThWrk[i].m_strGrpTyp = strGrpTyp;
                WrkComThWrk[i].m_strConnectionString = strPostStrConnect;
                WrkComThWrk[i].m_strEqmtTyp = strEqmtTyp;
                WrkComThWrk[i].m_strMcNo = strMcNo;
                WrkComThWrk[i].m_strSpName = strSpName;
                WrkComThWrk[i].m_strIp = strIp;
                WrkComThWrk[i].m_strPort = strPort;
            }
        }
        #endregion

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            timer.Stop();
            try
            {

                #region IOTASK
                for (int i = 1; i <= cDefApp.GM_PROCESS_CNT; i++)
                {
                    if (WrkComThWrk[i].m_thThread == null)
                    {
                        WrkComThWrk[i].m_thThread = new Thread(WrkComThWrk[i].WrkCommBcr);
                        WrkComThWrk[i].m_thThread.IsBackground = true;
                        WrkComThWrk[i].m_thThread.Start();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            timer.Start();
        }

        protected override void OnStop()
        {
            // TODO: 서비스를 중지하는 데 필요한 작업을 수행하는 코드를 여기에 추가합니다.
            cDefApp.GM_STAT_MAIN = false;
            Log.Event("BCR 서비스 중지.");
        }
    }
}
