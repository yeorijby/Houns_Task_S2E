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
        //@@@.���� ���α׷��� ���� �ν��Ͻ��� ���� ������ ���θ� Ȯ��
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
                pMsg = strTitle + "���� ó���Ǿ����ϴ�.";
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
