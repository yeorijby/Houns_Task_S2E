using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace SERIAL_CONVERTOR
{  
    public class cDbUse : cDbBaseOra   //@.상속
    {
         //@. 일반적인 PC Client에서 Global Connection 객체 1개 사용 시
        public cDbUse()
        {
            this.DbBaseOra(cDefApp.GM_DB_CN, false);
        }
        public  cDbUse(bool pBind)
        {
            this.DbBaseOra(cDefApp.GM_DB_CN, pBind);
        }

        //@. cBaseDb마다 Connection을 별도로 가져가야 할 경우, PDA Server
        //@. Backgroud Process에서 cBaseDb의 Connection객체를 사용 할 경우
        //@. strDummy는 Overload 함수 정의 시 구분하기 위한 파라미터, 즉 의미가 없다.
        //@. ex) Public BDb As New cDbUse("Multi", False)
        public cDbUse(string pStrDummy)
        {
            this.DbBaseOra(false); //@.Backgroud Process에서 CBaseDb의 Connection객체를 사용 할 경우(//@. 종료시 comMain.Close를 반드시 호출)
        }
        public  cDbUse(string pStrDummy, bool pBind )
        {
            this.DbBaseOra(pBind); //@.Backgroud Process에서 CBaseDb의 Connection객체를 사용 할 경우(//@. 종료시 comMain.Close를 반드시 호출)
        }
        //@. Connection이 2개 이상일 경우 (지정), 지정된 갯수만 필요할 때
        public cDbUse(ref OleDbConnection pCon, bool pBind)
        {
            this.DbBaseOra(ref pCon, pBind);
        }

        //@. Connection이 2개 이상일 경우 (지정), 지정된 갯수만 필요할 때
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
