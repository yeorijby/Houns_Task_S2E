using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace SERIAL_CONVERTOR
{  
    public class cDbUse : cDbBaseOra   //@.���
    {
         //@. �Ϲ����� PC Client���� Global Connection ��ü 1�� ��� ��
        public cDbUse()
        {
            this.DbBaseOra(cDefApp.GM_DB_CN, false);
        }
        public  cDbUse(bool pBind)
        {
            this.DbBaseOra(cDefApp.GM_DB_CN, pBind);
        }

        //@. cBaseDb���� Connection�� ������ �������� �� ���, PDA Server
        //@. Backgroud Process���� cBaseDb�� Connection��ü�� ��� �� ���
        //@. strDummy�� Overload �Լ� ���� �� �����ϱ� ���� �Ķ����, �� �ǹ̰� ����.
        //@. ex) Public BDb As New cDbUse("Multi", False)
        public cDbUse(string pStrDummy)
        {
            this.DbBaseOra(false); //@.Backgroud Process���� CBaseDb�� Connection��ü�� ��� �� ���(//@. ����� comMain.Close�� �ݵ�� ȣ��)
        }
        public  cDbUse(string pStrDummy, bool pBind )
        {
            this.DbBaseOra(pBind); //@.Backgroud Process���� CBaseDb�� Connection��ü�� ��� �� ���(//@. ����� comMain.Close�� �ݵ�� ȣ��)
        }
        //@. Connection�� 2�� �̻��� ��� (����), ������ ������ �ʿ��� ��
        public cDbUse(ref OleDbConnection pCon, bool pBind)
        {
            this.DbBaseOra(ref pCon, pBind);
        }

        //@. Connection�� 2�� �̻��� ��� (����), ������ ������ �ʿ��� ��
        public cDbUse(OleDbConnection pCn)
        {
            this.DbBaseOra(pCn, false);
        }
        public  cDbUse(OleDbConnection  pCn, bool pBind)
        {
            this.DbBaseOra(pCn, pBind);
        }
    }
}
