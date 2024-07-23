using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ServiceProcess;

namespace SERIAL_CONVERTOR
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new ASRS_BCR()
            //};
            //ServiceBase.Run(ServicesToRun);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SYS_MAIN());
        }
    }
}