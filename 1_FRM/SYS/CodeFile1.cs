  #region @@@.[Method]WrkCommMc18::TSK <-> M/C18 통신[Soket]
        private void WrkCommMc18()
        {
            cWrkBcrSkt Wrk;                                                     // @.작업 클레스[통신]
            cDefApp.stutLogMsgInfo logMsg = new cDefApp.stutLogMsgInfo();     // @.로그메세지처리구조체
            cDefApp.stutComProc log = new cDefApp.stutComProc();                // @.로그메세지처리구조체 
            string sRtnMsg = "";                                                // @.Return Error Message
            int nRtnReq = 0;                                                   // @.Return Request Count
            string sReadTm = "";                                              // @.읽기시간[상태읽기]
            string MC_NO = "001";
            string MC_TYP = "001";
            string MC_IP = "127.0.0.1";
            int MC_PORT = 5001;

            // @@.여기 부분은 쓰레드 별로 설정해 주어야 함[설비번호, DB상태 표시 컨트롤, 작업상태 표시 컨트롤]
            PictureBox picStatOp = this.picStatOp18;                             // @.통신상태/작업 상태 신호등
            PictureBox picStatDbCn = this.picStatDbCn18;                         // @.DB 연결 상태 신호등

            if (cDefApi.GsReadInitProfileCom("COMM18", ref MC_NO, ref MC_TYP, ref MC_IP, ref MC_PORT, ref sRtnMsg) == false) //@@.A/L 접속정보ini 읽어오기
            {
                PsMsgView(sRtnMsg, "WrkCommMc18", "ERR", "", cDefApp.eLogMsgType.MSG_ERR);          // @.Logging[View]
                return;
            }
            this.cboMcNo.Items.Add(MC_NO);
            Wrk = new cWrkBcrSkt(MC_NO, MC_TYP);                                       // @.작업 클레스[통신, DB]

            while (true)
            {
                try
                {
                    // @@.쓰레드종료체크[SYSTME 종료 시 스레드 종료]
                    if (cDefApp.GM_STAT_MAIN == false)
                    {
                        if (chkUseDB.Checked == true)   // @.DB사용여부체크
                        {
                            // @@.DB에 프로그램 종료 상태 Update!![필요 없을 시 삭제해도 됨]
                            if (Wrk.mDbWrk.mCnMain.State == System.Data.ConnectionState.Open & Wrk.mDbWrk.DbConnted == true)
                            {
                                logMsg.Com = "DB"; if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "ED", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg);
                            }
                        }
                        break;
                    }

                    if (chkUseDB.Checked == true)
                    {
                        // @@.DB 접속[Wrk, Log]
                        if (Wrk.mDbWrk.mCnMain.State == System.Data.ConnectionState.Closed || Wrk.mDbWrk.DbConnted == false |
                           Wrk.mDbLog.mCnMain.State == System.Data.ConnectionState.Closed || Wrk.mDbLog.DbConnted == false)
                        {
                            PfSetStatImgView(picStatDbCn, "T");         // @.Stat Connection[C:연결, T:시도, D:비연결]
                            Wrk.DBLogIn();                              // @.DBLogin
                            PsMsgView(Wrk.mMsg, Wrk.mDVC_NO, "DB접속", "", Wrk.mDbWrk.DbConnted == true ? cDefApp.eLogMsgType.MSG_IMP : cDefApp.eLogMsgType.MSG_ERR);
                            continue;
                        }
                        else
                        {
                            PfSetStatImgView(picStatDbCn, "C");   // @.Stat Connection[C:연결, T:시도, D:비연결]
                        }
                    }

                    // @@.통신 접속[TSK <-> PLC]
                    if (Wrk.IsSktConnect == false)
                    {
                        if (chkUseDB.Checked == true)   // @.DB사용여부체크
                        {
                            logMsg.Com = "DB";
                            if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "ER", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg); // @.통신접속 상태 Update 한다.
                        }
                        Wrk.mStatComm = "T"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp); // @.쓰레드 연결상태[시도], 작업상태표시
                        Wrk.SktConnect(MC_IP, MC_PORT, ref sRtnMsg);  // @.소켓접속
                        continue;
                    }
                    else
                    {
                        if (chkUseDB.Checked == true)   // @.DB사용여부체크
                        {
                            logMsg.Com = "DB";
                            if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "OK", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg); // @.통신접속 상태 Update 한다.
                        }
                        Wrk.mStatComm = "C"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp); // @.쓰레드 연결상태[연결], 작업상태표시
                    }

                    Wrk.mStatOp = "W"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.쓰레드 연결상태[연결], 작업상태표시[작업]

                    // @@.소켓통신버퍼 비우기
                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref  sRtnMsg) > 0) PsMsgView(sRtnMsg, Wrk.mDVC_NO, "버퍼비우기", System.Text.Encoding.Default.GetString(Wrk.mByRxBuffDum), cDefApp.eLogMsgType.MSG_ERR);


                    // ##.테스트시작
                    // @@.BCD정보 읽어오기
                    if (this.chkSndReqBCD.Checked == true && this.cboMcNo.Text == Wrk.mDVC_NO)
                    {
                        Wrk.mStatOp = "N"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.쓰레드 연결상태[연결], 작업상태표시[작업]
                        this.chkSndReqBCD.Checked = false;

                        switch (MC_TYP)
                        {
                            case "001":
                                {
                                    if (Wrk.SndReqBCD(Wrk.mSndTgm = cDefApp.GM_STR_STX + "21" + cDefApp.GM_STR_ETX, ref sRtnMsg) == true)
                                    {
                                        // @.BCD 읽기 요청송신
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                        if (Wrk.RcvAck(ref Wrk.mByRxBuff, 4, ref sRtnMsg) == true)
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK수신", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_IMP);

                                            if (Wrk.ChkAckTgm(ref sRtnMsg) == 0)
                                            {
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK체크", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                                if (Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 1, ref sRtnMsg) == true)
                                                {
                                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_IMP);

                                                    if (Wrk.ChkBCDTgm(ref sRtnMsg) == 0)
                                                    {
                                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                                    }
                                                    else
                                                    {
                                                        if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                        {
                                                            Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                        }
                                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                                    }
                                                }
                                                else
                                                {
                                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                                }
                                            }
                                            else
                                            {
                                                if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                {
                                                    Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                }
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                            }
                                        }
                                        else
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK수신", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                        }
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    break;
                                }
                            case "003":
                                {
                                    if (Wrk.SndReqBCD(Wrk.mSndTgm = "K", ref sRtnMsg) == true)
                                    {
                                        // @.BCD 읽기 요청송신
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                        if (Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 3, ref sRtnMsg) == true)
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_IMP);

                                            if (Wrk.ChkBCDTgm(ref sRtnMsg) == 0)
                                            {
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                            }
                                            else
                                            {
                                                if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                {
                                                    Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                }
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                            }
                                        }
                                        else
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                        }
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    break;
                                }
                            default: break;

                        }

                    }
                    // ##.테스트 끝

                    Wrk.mStatOp = "W"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);

                    if (chkUseDB.Checked == false) // @.DB사용여부체크
                    {
                        continue;
                    }

                    // @@.요청 PlcCvIF 확인 및 처리[요청 PlcCvIF 정보 없을 시 대기][요청 체크 및 WrkDb.mDtPlcCvIF 테이블에 대상 조회]
                    logMsg.Com = "DB"; if (Wrk.GetReqBcrIF(ref nRtnReq, ref sRtnMsg) == false) throw new Exception(sRtnMsg);

                    if (nRtnReq < 1)
                    {
                        //Thread.Sleep(1)
                        continue;
                    }

                    Wrk.mStatOp = "N"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.쓰레드 연결상태[연결], 작업상태표시[작업]

                    // @@.소켓통신버퍼 비우기
                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref  sRtnMsg) > 0) PsMsgView(sRtnMsg, Wrk.mDVC_NO, "버퍼비우기", System.Text.Encoding.Default.GetString(Wrk.mByRxBuffDum), cDefApp.eLogMsgType.MSG_ERR);

                    log.init(); Wrk.mRcvTgm = "";                   // @.변수 초기화

                    switch (MC_TYP)
                    {
                        case "001":
                            {
                                Wrk.mSndTgm = cDefApp.GM_STR_STX + "21" + cDefApp.GM_STR_ETX;
                                log.bSndTgm = Wrk.SndReqBCD(Wrk.mSndTgm, ref sRtnMsg);       // @.BCD 읽기 요청송신

                                if (log.bSndTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvAck = Wrk.RcvAck(ref Wrk.mByRxBuff, 4, ref sRtnMsg); // @.ACK수신
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvAck == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkAck = Wrk.ChkAckTgm(ref sRtnMsg);   // @.ACK오류체크
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkAck > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.bRcvAck == true)
                                {
                                    //PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvTgm = Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 1, ref sRtnMsg); // @.BCD수신
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkTgm = Wrk.ChkBCDTgm(ref sRtnMsg);   // @.BCD오류체크
                                }
                                else if (log.bSndTgm == true && log.bRcvAck == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkTgm > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.nChkTgm == 0)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    if (Wrk.ProcBCRReqAckData(log.nChkTgm, sReadTm, ref sRtnMsg) == false)
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_IMP);
                                    }
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    if (Wrk.ProcBCRReqAckData(log.nChkTgm, sReadTm, ref sRtnMsg) == false)
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }
                                break;
                            }
                        case "003":
                            {
                                Wrk.mSndTgm = "K";
                                log.bSndTgm = Wrk.SndReqBCD(Wrk.mSndTgm, ref sRtnMsg);       // @.BCD 읽기 요청송신

                                if (log.bSndTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvTgm = Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 3, ref sRtnMsg); // @.BCD수신
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ송신", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkTgm = Wrk.ChkBCDTgm(ref sRtnMsg);   // @.BCD오류체크
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD수신", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkTgm > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크에러 더미읽기", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.nChkTgm == 0)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    if (Wrk.ProcBCRReqAckData(log.nChkTgm, sReadTm, ref sRtnMsg) == false)
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_IMP);
                                    }
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD체크", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    if (Wrk.ProcBCRReqAckData(log.nChkTgm, sReadTm, ref sRtnMsg) == false)
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "DB", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_IMP);
                                    }
                                }
                                break;
                            }
                        default: break;
                    }
                    Wrk.mStatOp = "W"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);
                    continue;
                }
                catch (SocketException se)
                {
                    sRtnMsg = se.Message;
                }
                catch (Exception ex)
                {
                    sRtnMsg = ex.Message;
                }
                Wrk.mStatOp = "E"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);     // @.쓰레드 연결상태, 작업상태표시[에러]
                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ERR", "", cDefApp.eLogMsgType.MSG_ERR);          // @.Logging[View]
                logMsg.init();
            }
        }
        #endregion                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            