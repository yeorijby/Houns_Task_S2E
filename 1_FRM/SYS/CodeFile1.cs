  #region @@@.[Method]WrkCommMc18::TSK <-> M/C18 ���[Soket]
        private void WrkCommMc18()
        {
            cWrkBcrSkt Wrk;                                                     // @.�۾� Ŭ����[���]
            cDefApp.stutLogMsgInfo logMsg = new cDefApp.stutLogMsgInfo();     // @.�α׸޼���ó������ü
            cDefApp.stutComProc log = new cDefApp.stutComProc();                // @.�α׸޼���ó������ü 
            string sRtnMsg = "";                                                // @.Return Error Message
            int nRtnReq = 0;                                                   // @.Return Request Count
            string sReadTm = "";                                              // @.�б�ð�[�����б�]
            string MC_NO = "001";
            string MC_TYP = "001";
            string MC_IP = "127.0.0.1";
            int MC_PORT = 5001;

            // @@.���� �κ��� ������ ���� ������ �־�� ��[�����ȣ, DB���� ǥ�� ��Ʈ��, �۾����� ǥ�� ��Ʈ��]
            PictureBox picStatOp = this.picStatOp18;                             // @.��Ż���/�۾� ���� ��ȣ��
            PictureBox picStatDbCn = this.picStatDbCn18;                         // @.DB ���� ���� ��ȣ��

            if (cDefApi.GsReadInitProfileCom("COMM18", ref MC_NO, ref MC_TYP, ref MC_IP, ref MC_PORT, ref sRtnMsg) == false) //@@.A/L ��������ini �о����
            {
                PsMsgView(sRtnMsg, "WrkCommMc18", "ERR", "", cDefApp.eLogMsgType.MSG_ERR);          // @.Logging[View]
                return;
            }
            this.cboMcNo.Items.Add(MC_NO);
            Wrk = new cWrkBcrSkt(MC_NO, MC_TYP);                                       // @.�۾� Ŭ����[���, DB]

            while (true)
            {
                try
                {
                    // @@.����������üũ[SYSTME ���� �� ������ ����]
                    if (cDefApp.GM_STAT_MAIN == false)
                    {
                        if (chkUseDB.Checked == true)   // @.DB��뿩��üũ
                        {
                            // @@.DB�� ���α׷� ���� ���� Update!![�ʿ� ���� �� �����ص� ��]
                            if (Wrk.mDbWrk.mCnMain.State == System.Data.ConnectionState.Open & Wrk.mDbWrk.DbConnted == true)
                            {
                                logMsg.Com = "DB"; if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "ED", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg);
                            }
                        }
                        break;
                    }

                    if (chkUseDB.Checked == true)
                    {
                        // @@.DB ����[Wrk, Log]
                        if (Wrk.mDbWrk.mCnMain.State == System.Data.ConnectionState.Closed || Wrk.mDbWrk.DbConnted == false |
                           Wrk.mDbLog.mCnMain.State == System.Data.ConnectionState.Closed || Wrk.mDbLog.DbConnted == false)
                        {
                            PfSetStatImgView(picStatDbCn, "T");         // @.Stat Connection[C:����, T:�õ�, D:�񿬰�]
                            Wrk.DBLogIn();                              // @.DBLogin
                            PsMsgView(Wrk.mMsg, Wrk.mDVC_NO, "DB����", "", Wrk.mDbWrk.DbConnted == true ? cDefApp.eLogMsgType.MSG_IMP : cDefApp.eLogMsgType.MSG_ERR);
                            continue;
                        }
                        else
                        {
                            PfSetStatImgView(picStatDbCn, "C");   // @.Stat Connection[C:����, T:�õ�, D:�񿬰�]
                        }
                    }

                    // @@.��� ����[TSK <-> PLC]
                    if (Wrk.IsSktConnect == false)
                    {
                        if (chkUseDB.Checked == true)   // @.DB��뿩��üũ
                        {
                            logMsg.Com = "DB";
                            if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "ER", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg); // @.������� ���� Update �Ѵ�.
                        }
                        Wrk.mStatComm = "T"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp); // @.������ �������[�õ�], �۾�����ǥ��
                        Wrk.SktConnect(MC_IP, MC_PORT, ref sRtnMsg);  // @.��������
                        continue;
                    }
                    else
                    {
                        if (chkUseDB.Checked == true)   // @.DB��뿩��üũ
                        {
                            logMsg.Com = "DB";
                            if (Wrk.UpdMC_STA_MST_BCR_COM_STA(cDefApp.GM_WH_TYP, "OK", "", ref sRtnMsg) == false) throw new Exception(sRtnMsg); // @.������� ���� Update �Ѵ�.
                        }
                        Wrk.mStatComm = "C"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp); // @.������ �������[����], �۾�����ǥ��
                    }

                    Wrk.mStatOp = "W"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.������ �������[����], �۾�����ǥ��[�۾�]

                    // @@.������Ź��� ����
                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref  sRtnMsg) > 0) PsMsgView(sRtnMsg, Wrk.mDVC_NO, "���ۺ���", System.Text.Encoding.Default.GetString(Wrk.mByRxBuffDum), cDefApp.eLogMsgType.MSG_ERR);


                    // ##.�׽�Ʈ����
                    // @@.BCD���� �о����
                    if (this.chkSndReqBCD.Checked == true && this.cboMcNo.Text == Wrk.mDVC_NO)
                    {
                        Wrk.mStatOp = "N"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.������ �������[����], �۾�����ǥ��[�۾�]
                        this.chkSndReqBCD.Checked = false;

                        switch (MC_TYP)
                        {
                            case "001":
                                {
                                    if (Wrk.SndReqBCD(Wrk.mSndTgm = cDefApp.GM_STR_STX + "21" + cDefApp.GM_STR_ETX, ref sRtnMsg) == true)
                                    {
                                        // @.BCD �б� ��û�۽�
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                        if (Wrk.RcvAck(ref Wrk.mByRxBuff, 4, ref sRtnMsg) == true)
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK����", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_IMP);

                                            if (Wrk.ChkAckTgm(ref sRtnMsg) == 0)
                                            {
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACKüũ", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                                if (Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 1, ref sRtnMsg) == true)
                                                {
                                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_IMP);

                                                    if (Wrk.ChkBCDTgm(ref sRtnMsg) == 0)
                                                    {
                                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                                    }
                                                    else
                                                    {
                                                        if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                        {
                                                            Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                        }
                                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                                    }
                                                }
                                                else
                                                {
                                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                                }
                                            }
                                            else
                                            {
                                                if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                {
                                                    Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                }
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACKüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                            }
                                        }
                                        else
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK����", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                        }
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    break;
                                }
                            case "003":
                                {
                                    if (Wrk.SndReqBCD(Wrk.mSndTgm = "K", ref sRtnMsg) == true)
                                    {
                                        // @.BCD �б� ��û�۽�
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);

                                        if (Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 3, ref sRtnMsg) == true)
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_IMP);

                                            if (Wrk.ChkBCDTgm(ref sRtnMsg) == 0)
                                            {
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                            }
                                            else
                                            {
                                                if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                                {
                                                    Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                                }
                                                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                            }
                                        }
                                        else
                                        {
                                            PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", System.Text.Encoding.Default.GetString(Wrk.mByRxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                        }
                                    }
                                    else
                                    {
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", System.Text.Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                    break;
                                }
                            default: break;

                        }

                    }
                    // ##.�׽�Ʈ ��

                    Wrk.mStatOp = "W"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);

                    if (chkUseDB.Checked == false) // @.DB��뿩��üũ
                    {
                        continue;
                    }

                    // @@.��û PlcCvIF Ȯ�� �� ó��[��û PlcCvIF ���� ���� �� ���][��û üũ �� WrkDb.mDtPlcCvIF ���̺� ��� ��ȸ]
                    logMsg.Com = "DB"; if (Wrk.GetReqBcrIF(ref nRtnReq, ref sRtnMsg) == false) throw new Exception(sRtnMsg);

                    if (nRtnReq < 1)
                    {
                        //Thread.Sleep(1)
                        continue;
                    }

                    Wrk.mStatOp = "N"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);    // @.������ �������[����], �۾�����ǥ��[�۾�]

                    // @@.������Ź��� ����
                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref  sRtnMsg) > 0) PsMsgView(sRtnMsg, Wrk.mDVC_NO, "���ۺ���", System.Text.Encoding.Default.GetString(Wrk.mByRxBuffDum), cDefApp.eLogMsgType.MSG_ERR);

                    log.init(); Wrk.mRcvTgm = "";                   // @.���� �ʱ�ȭ

                    switch (MC_TYP)
                    {
                        case "001":
                            {
                                Wrk.mSndTgm = cDefApp.GM_STR_STX + "21" + cDefApp.GM_STR_ETX;
                                log.bSndTgm = Wrk.SndReqBCD(Wrk.mSndTgm, ref sRtnMsg);       // @.BCD �б� ��û�۽�

                                if (log.bSndTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvAck = Wrk.RcvAck(ref Wrk.mByRxBuff, 4, ref sRtnMsg); // @.ACK����
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvAck == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkAck = Wrk.ChkAckTgm(ref sRtnMsg);   // @.ACK����üũ
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACK����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkAck > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ACKüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.bRcvAck == true)
                                {
                                    //PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvTgm = Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 1, ref sRtnMsg); // @.BCD����
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkTgm = Wrk.ChkBCDTgm(ref sRtnMsg);   // @.BCD����üũ
                                }
                                else if (log.bSndTgm == true && log.bRcvAck == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkTgm > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.nChkTgm == 0)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
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
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
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
                                log.bSndTgm = Wrk.SndReqBCD(Wrk.mSndTgm, ref sRtnMsg);       // @.BCD �б� ��û�۽�

                                if (log.bSndTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_NOR);
                                    log.bRcvTgm = Wrk.RcvBCD(ref Wrk.mRcvTgm, cDefApp.GM_STR_ETX, 3, ref sRtnMsg); // @.BCD����
                                }
                                else
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "REQ�۽�", Encoding.Default.GetString(Wrk.mByTxBuff), cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.bRcvTgm == true)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
                                    log.nChkTgm = Wrk.ChkBCDTgm(ref sRtnMsg);   // @.BCD����üũ
                                }
                                else if (log.bSndTgm == true && log.bRcvTgm == false)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCD����", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                }

                                if (log.nChkTgm > 0)
                                {
                                    if (Wrk.RcvDum(ref Wrk.mByRxBuffDum, ref sRtnMsg) > 0)
                                    {
                                        Wrk.mRcvTgm = Encoding.Default.GetString(Wrk.mByRxBuffDum);
                                        PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ���� �����б�", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
                                    }
                                }

                                if (log.nChkTgm == 0)
                                {
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_NOR);
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
                                    PsMsgView(sRtnMsg, Wrk.mDVC_NO, "BCDüũ", Wrk.mRcvTgm, cDefApp.eLogMsgType.MSG_ERR);
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
                Wrk.mStatOp = "E"; PfSetStatImgView(picStatOp, Wrk.mStatComm, Wrk.mStatOp);     // @.������ �������, �۾�����ǥ��[����]
                PsMsgView(sRtnMsg, Wrk.mDVC_NO, "ERR", "", cDefApp.eLogMsgType.MSG_ERR);          // @.Logging[View]
                logMsg.init();
            }
        }
        #endregion                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            