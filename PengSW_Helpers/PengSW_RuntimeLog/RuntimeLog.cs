using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

namespace PengSW.RuntimeLog
{
    /// <summary>
    /// ����ʱ��־
    ///     Ϊ�򻯴�����ʽ����RuntimeLog��дΪRL��
    /// </summary>
    public class RL
    {
        /// <summary>
        /// ����һ����־��¼��
        /// </summary>
        /// <param name="aLogFilename">ָ����־�ļ�����������׺��Լ����׺Ϊ.yyyyMMdd.log</param>
        public RL(string aLogFilename)
        {
            LogFileName = aLogFilename;
        }

        /// <summary>
        /// ��Ĭ����־�ļ�������һ����־��¼��
        /// </summary>
        public RL()
        {
            LogFileName = DefaultLogFileName;
        }

        /// <summary>
        /// Ĭ�ϱ��漶��Ϊ4�����쳣��Ϣ�ļ���Ϊ0��
        /// </summary>
        static RL()
        {
            MinLevelToSave = 9;
            MinLevelToClarify = 9;
        }

        /// <summary>
        /// ָ��������־��¼������־�ļ���
        /// </summary>
        /// <param name="aLogFileName">��־�ļ�����������׺��Լ����׺Ϊ.yyyyMMdd.log</param>
        public static void SetShareLog(string aLogFileName)
        {
            GlobalLogFileName = aLogFileName;
        }
        
        /// <summary>
        /// ����Ĭ�ϵĹ�����־�ļ���
        ///     Ĭ�ϵĹ�����־�ļ���Ϊ��ģ������.log
        /// </summary>
        public static void SetShareLog()
        {
            SetShareLog(DefaultLogFileName);
        }

        public static int MinLevelToSave { get; set; }
        public static int MinLevelToClarify { get; set; }

        private static string DefaultLogFileName
        {
            get
            {
                FileInfo aFileInfo = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                string aDirectoryName = aFileInfo.DirectoryName;
                string aName = Path.GetFileNameWithoutExtension(aFileInfo.Name);
                return Path.Combine(aDirectoryName, aName);
            }
        }

        public static event System.Action<string> ClarifyShareLog;
        private static void RaiseClarifyShareLog(string aLog)
        {
            ClarifyShareLog?.Invoke(aLog);
        }

        public event System.Action<string> ClarifyLog;
        private void RaiseClarifyLog(string aLog)
        {
            ClarifyLog?.Invoke(aLog);
        }

        /// <summary>
        /// ��¼������־��Ϣ
        /// </summary>
        /// <param name="aLog">��־����</param>
        public void Write(string aLog, bool aShowDialog = false, bool aClarify = true, int aLevel = 0)
        {
            _Write(LogFileName, aLog, aLog, aLog, aShowDialog, aClarify, aLevel);
        }
        public void WriteException(Exception ex, bool aShowDialog = false, bool aClarify = true, string aTitle = null, bool aStackTrace = true)
        {
            _WriteException(LogFileName, ex, aShowDialog, aClarify, aTitle, aStackTrace);
        }

        /// <summary>
        /// ��¼����������־
        /// </summary>
        /// <param name="aLog">��־����</param>
        public static void WriteLog(string aLog, bool aShowDialog = false, bool aClarify = true, int aLevel = 4)
        {
            _Write(GlobalLogFileName, aLog, aLog, aLog, aShowDialog, aClarify, aLevel);
        }
        public static void L(string aLog, int aLevel = 4, bool aShowDialog = false, bool aClarify = true)
        {
            _Write(GlobalLogFileName, aLog, aLog, aLog, aShowDialog, aClarify, aLevel);
        }

        public static void WriteExceptionLog(Exception ex, bool aShowDialog = false, bool aClarify = true, string aTitle = null, bool aStackTrace = true)
        {
            _WriteException(GlobalLogFileName, ex, aShowDialog, aClarify, aTitle, aStackTrace);
        }

        public static void WriteExceptionLog(Exception ex, string aTitle, bool aShowDialog = false, bool aClarify = true, bool aStackTrace = true)
        {
            _WriteException(GlobalLogFileName, ex, aShowDialog, aClarify, aTitle, aStackTrace);
        }
        public static void E(Exception ex, string aTitle, bool aShowDialog = false, bool aClarify = true, bool aStackTrace = true)
        {
            _WriteException(GlobalLogFileName, ex, aShowDialog, aClarify, aTitle, aStackTrace);
        }

        public static void MethodTrace(string aPrefix, params object[] aParameters)
        {
            string aMethodName = new StackFrame(1).GetMethod().Name;
            StringBuilder aStringBuilder = new StringBuilder();
            aStringBuilder.Append(aPrefix);
            aStringBuilder.Append(aMethodName);
            aStringBuilder.Append("(");
            foreach (object aParameter in aParameters)
            {
                aStringBuilder.Append(aParameter);
                aStringBuilder.Append(", ");
            }
            if (aParameters.Length > 0) aStringBuilder.Length -= 2;
            aStringBuilder.Append(");");
            WriteLog(aStringBuilder.ToString(), false, false);
        }

        private static string CreateRealLogFileName(string aFileName) => aFileName + "." + System.DateTime.Now.ToString("yyyyMMdd") + ".log";

        private static void _WriteException(string aFileName, System.Exception ex, bool aShowDialog, bool aClarify, string aTitle, bool aStackTrace)
        {
            StringBuilder aLogBuilder = new StringBuilder();
            StringBuilder aClarifyBuilder = new StringBuilder();
            aLogBuilder.AppendLine("Exception catched.");
            if (!string.IsNullOrEmpty(aTitle))
            {
                aLogBuilder.Append("Exception Title: ");
                aLogBuilder.AppendLine(aTitle);
                aClarifyBuilder.Append(aTitle);
                aClarifyBuilder.Append(":");
            }
            aLogBuilder.Append("Exception Message: ");
            aLogBuilder.AppendLine(ex.Message);
            aClarifyBuilder.Append(ex.Message);
            aLogBuilder.Append("Exception Type: ");
            aLogBuilder.AppendLine(ex.GetType().FullName);
            if (aStackTrace)
            {
                aLogBuilder.AppendLine("Stack Trace:");
                aLogBuilder.AppendLine(ex.StackTrace);
            }

            _Write(aFileName, aLogBuilder.ToString(), aClarifyBuilder.ToString(), aClarifyBuilder.ToString(), aShowDialog, aClarify, 0);
        }

        private static void _Write(string aFileName, string aWriteLog, string aClarifyLog, string aDialogLog, bool aShowDialog, bool aClarify, int aLevel)
        {
            StringBuilder aLogBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(aFileName) && aLevel <= MinLevelToSave)
            {
                aLogBuilder.Append(string.Format("[{0:yyyy-MM-dd HH:mm:ss}]", System.DateTime.Now));
                aLogBuilder.Append(string.Format("[{0}] ", aLevel));
                aLogBuilder.AppendLine(aWriteLog);
                _Write(aFileName, aLogBuilder.ToString());
            }

            if (aClarify && aLevel <= MinLevelToClarify)
            {
                aLogBuilder.Length = 0;
                aLogBuilder.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ");
                aLogBuilder.AppendLine(aClarifyLog);
                RaiseClarifyShareLog(aLogBuilder.ToString());
            }

            if (aShowDialog)
            {
                MessageBox.Show(aDialogLog, "Runtime Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static object _lockobject = new object();
        private static void _Write(string aFileName, string aLog)
        {
            try
            {
                lock (_lockobject)
                {
                    File.AppendAllText(CreateRealLogFileName(aFileName), aLog, Encoding.Unicode);
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public string LogFileName  { get; private set; }

        public static string GlobalLogFileName 
        {
            get 
            { 
                if (string.IsNullOrEmpty(_GlobalLogFileName)) _GlobalLogFileName = DefaultLogFileName;
                return _GlobalLogFileName; 
            } 
            private set
            {
                _GlobalLogFileName = value;
            }
        }
        private static string _GlobalLogFileName = null;

        protected static void _OpenLogFile(string aFileName)
        {
            try
            {
                Process.Start(CreateRealLogFileName(aFileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Log Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static void OpenLogFile()
        {
            _OpenLogFile(GlobalLogFileName);
        }

        public void Open()
        {
            _OpenLogFile(LogFileName);
        }

        private static void _RemoveLogFile(System.DateTime aRetainTime, string aFileNameHead)
        {
            try
            {
                DirectoryInfo aDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(aFileNameHead));
                foreach (FileInfo aFileInfo in aDirectoryInfo.GetFiles(Path.GetFileName(aFileNameHead) + ".*.log"))
                {
                    Match aMatch = Regex.Match(aFileInfo.Name, @"\.(\d\d\d\d)(\d\d)(\d\d)\.log$", RegexOptions.IgnoreCase);
                    if (aMatch != null && aMatch.Success)
                    {
                        System.DateTime aFileTime;
                        if (System.DateTime.TryParse(string.Format("{0}-{1}-{2}", aMatch.Groups[1].Value, aMatch.Groups[2].Value, aMatch.Groups[3].Value), out aFileTime))
                        {
                            if (aFileTime < aRetainTime) aFileInfo.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExceptionLog(ex, false, false);
            }
        }

        public static void RemoveLogFileBefore(System.DateTime aRetainTime)
        {
            _RemoveLogFile(aRetainTime, GlobalLogFileName);
        }

        public void RemoveBefore(System.DateTime aRetainTime)
        {
            _RemoveLogFile(aRetainTime, LogFileName);
        }

        public static void DumpDatas(object[] aDatas)
        {
            System.Text.StringBuilder aStringBuilder = new StringBuilder();
            aStringBuilder.AppendLine(string.Format("Dump {1}, {0} elements: ", aDatas.Length, aDatas.GetType().Name));
            foreach (object aData in aDatas)
            {
                aStringBuilder.Append("    ");
                foreach (System.Reflection.PropertyInfo aPropertyInfo in aData.GetType().GetProperties())
                {
                    aStringBuilder.Append(string.Format("[{0}]=[{1}], ", aPropertyInfo.Name, aPropertyInfo.GetValue(aData, null)));
                }
                aStringBuilder.AppendLine();
            }
            WriteLog(aStringBuilder.ToString());
        }
    }
}
