using System;
using System.Text;
using System.Threading.Tasks;

namespace KamaradaBot.Core
{
    public class Logger
    {
        private const string FILE_EXT = ".log";
        private readonly string datetimeFormat;
        private readonly string logFilename;

        public Logger()
        {
            datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT;

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLine(DateTime.Now.ToString(datetimeFormat) + " " + logHeader, false);
            }
        }

        public async Task Debug(string text)
        {
            await WriteFormattedLog(LogLevel.DEBUG, text);
        }

        public async Task Error(string text)
        {
            await WriteFormattedLog(LogLevel.ERROR, text);
        }

        public async Task Fatal(string text)
        {
            await WriteFormattedLog(LogLevel.FATAL, text);
        }

        public async Task Info(string text)
        {
            await WriteFormattedLog(LogLevel.INFO, text);
        }

        public async Task Trace(string text)
        {
            await WriteFormattedLog(LogLevel.TRACE, text);
        }

        public async Task Warning(string text)
        {
            await WriteFormattedLog(LogLevel.WARNING, text);
        }

        private async Task WriteLineAsync(string text, bool append = true)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        await writer.WriteLineAsync(text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void WriteLine(string text, bool append = true)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        writer.WriteLine(text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;
            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [TRACE]   ";
                    break;
                case LogLevel.INFO:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [INFO]    ";
                    break;
                case LogLevel.DEBUG:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [DEBUG]   ";
                    break;
                case LogLevel.WARNING:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [WARNING] ";
                    break;
                case LogLevel.ERROR:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [ERROR]   ";
                    break;
                case LogLevel.FATAL:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [FATAL]   ";
                    break;
                default:
                    pretext = "";
                    break;
            }

            await WriteLineAsync(pretext + text);
        }

        [Flags]
        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
    }
}
