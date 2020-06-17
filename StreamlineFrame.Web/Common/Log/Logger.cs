using System;
using System.IO;


namespace StreamlineFrame.Web.Common
{
    public class Logger
    {
        #region Instance
        private static object logLock;

        private static Logger _instance;

        private Logger() { }

        /// <summary>
        /// Logger instance
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                    logLock = new object();
                }
                return _instance;
            }
        }
        #endregion
        public void WriteError(string logContent)
        {
            this.WriteLog(logContent, LogType.Error);
        }

        public void WriteDebug(string logContent)
        {
            this.WriteLog(logContent, LogType.Debug);
        }

        public void WriteSuccess(string logContent)
        {
            this.WriteLog(logContent, LogType.Success);
        }

        public void WriteWarning(string logContent)
        {
            this.WriteLog(logContent, LogType.Warning);
        }

        public void WriteInformation(string logContent)
        {
            this.WriteLog(logContent, LogType.Information);
        }

        /// <summary>
        /// Write log to log file
        /// </summary>
        /// <param name="logContent">Log content</param>
        /// <param name="logType">Log type</param>
        private void WriteLog(string logContent, LogType logType)
        {
            try
            {
                var basePath = ServerHelper.HostingMapPath("/Log");
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                var dataPath = $@"{basePath}\{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                var logText = new string[] { $"{DateTime.Now.ToString("hh:mm:ss")} -> {IPHelper.GetWebClientIp()} | {logType.ToString()} : {logContent}" };

                lock (logLock)
                    File.AppendAllLines($@"{dataPath}\{logType.ToString()}", logText);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Write exception to log file
        /// </summary>
        /// <param name="exception">Exception</param>
        public void WriteException(Exception exception, string specialText = null)
        {
            if (exception != null)
            {
                var exceptionType = exception.GetType();
                var text = string.Empty;
                if (!string.IsNullOrEmpty(specialText))
                    text += specialText + Environment.NewLine;
                text = "Exception: " + exceptionType.Name + Environment.NewLine;
                text += "\t" + "Message: " + exception.Message + Environment.NewLine;
                text += "\t" + "Source: " + exception.Source + Environment.NewLine;
                text += "\t" + "StackTrace: " + exception.StackTrace + Environment.NewLine;
                WriteLog(text, LogType.Error);
            }
        }
    }
}
