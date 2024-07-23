using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms;
 
namespace SERIAL_CONVERTOR
{
    class cCmLib
    {
        //@@@.응용 프로그램의 이전 인스턴스가 실행 중인지 여부를 확인
        public static bool GfPrevInstance()
        {
            if (Process.GetCurrentProcess().ProcessName.IndexOf(".vshost") > 0) return false;
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).GetUpperBound(0) > 0)
            {
                return true;
            }
            else
            {
                return false;
            } 
        }

        //@@@.Data Base Connection Open
        public static bool GfDBLogIn(ref OleDbConnection pConObj,ref string pConnectionString, ref string  pMsg)
        {
            string strTitle = "[GfDBLogIn] ";

            try
            {
                pMsg = "";

                pConObj = new OleDbConnection();
                pConObj.ConnectionString = pConnectionString;

                pConObj.Open();
                pMsg = strTitle + "정상 처리되었습니다.";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
