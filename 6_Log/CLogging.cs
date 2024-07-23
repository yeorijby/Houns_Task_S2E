using System;
using System.Collections.Generic;
using System.Text;

namespace SERIAL_CONVERTOR
{
	public class CLogging
	{
		public string m_strLogFile = "";
		public string m_strLogName = "";
		public string m_strLogDir = "";
		public int m_nSaveDay = 0;
		public bool m_bHex = false;
		public string m_strCrLf = "";
		public bool m_bLog = false;
		public bool m_bTimeLog = false;
		public string m_strContent = "";

		public CLogging(string strLogDir, string strLogName, int nSaveDay)
		{
			m_strLogDir = strLogDir;
			m_strLogName = strLogName;
			m_nSaveDay = nSaveDay;
			m_strLogFile = m_strLogDir + @"\" + m_strLogName;

			IsExistFolder(m_strLogDir);

			EnabledCrLf();
			Enable();
			EnableTimeLog();
		}

		public void IsExistFolder(string strFolder)
		{
			System.IO.DirectoryInfo diFolder = new System.IO.DirectoryInfo(strFolder);
			if (!(diFolder.Exists)) diFolder.Create();
			diFolder = null;
		}

		public void EnabledCrLf()
		{
			m_strCrLf = "\r\n";
		}
		public void DisabledCrLf()
		{
			m_strCrLf = "";
		}
		public bool IsHex()
		{
			return m_bHex;
		}
		public bool IsLog()
		{
			return m_bLog;
		}
		public bool IsTime()
		{
			return m_bTimeLog;
		}
		public void Enable()
		{
			m_bLog = true;
		}
		public void Disable()
		{
			m_bLog = false;
		}
		public void EnableTimeLog()
		{
			m_bTimeLog = true;
		}
		public void DisableTimeLog()
		{
			m_bTimeLog = false;
		}

		public void Write(DateTime dtLog, string strContent)
		{
			if (!(IsLog())) return;

			//Directory에 파일이 있는지 확인.
			IsExistFolder(m_strLogDir);

			int nDay;
			string strTemp;
			string strLogFile;
			//DateTime dDate = DateTime.Now;

			DateTime dCmpDate = dtLog.AddDays(-m_nSaveDay);

			for (nDay = 0; nDay < 20; nDay++)
			{
				strTemp = dCmpDate.AddDays(-nDay).ToString("yyyyMMdd");
				strLogFile = m_strLogFile + "_" + strTemp + ".log";
				System.IO.File.Delete(strLogFile);
			}

			System.Text.Encoding Korean;
			Korean = System.Text.Encoding.GetEncoding(949);


			strTemp = dtLog.ToString("yyyyMMdd");
			strLogFile = m_strLogFile + "_" + strTemp + ".log";


			System.IO.FileStream Stream = new System.IO.FileStream(strLogFile, System.IO.FileMode.Append);
			System.IO.StreamWriter Writer = new System.IO.StreamWriter(Stream, Korean);

			string strTime = "";

			if (IsTime())
			{
				dtLog = DateTime.Now;
				strTime = dtLog.ToString("HH:mm:ss:") + dtLog.Millisecond.ToString("000");
			}

			Writer.Write(strTime + "-" + strContent + m_strCrLf);
			Writer.Close();

			Stream.Dispose();
			Writer.Dispose();

			Stream = null;
			Writer = null;
		}
	}
}
