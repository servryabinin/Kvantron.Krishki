using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Logger
{
    public class ErrorLogger
    {
        private static readonly object SyncRoot = new object();

        private static readonly string LogFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        private static readonly string LogFile =
            Path.Combine(LogFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");

        private static readonly string FrameQueueLogFile =
            Path.Combine(LogFolder, $"frame_queue_{DateTime.Now:yyyyMMdd}.txt");

        static ErrorLogger()
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                    Directory.CreateDirectory(LogFolder);
            }
            catch { /* если даже логер упал — ничего не делаем */ }
        }

        public static void Log(Exception ex, string context = "")
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("======================================");
                sb.AppendLine($"TIME: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                if (!string.IsNullOrWhiteSpace(context))
                    sb.AppendLine($"CONTEXT: {context}");

                sb.AppendLine($"MESSAGE: {ex.Message}");
                sb.AppendLine($"TYPE: {ex.GetType().Name}");
                sb.AppendLine("STACK:");
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine("======================================");
                sb.AppendLine();

                lock (SyncRoot)
                {
                    File.AppendAllText(LogFile, sb.ToString());
                }
            }
            catch
            {
                // Если даже лог записать не можем - ничего не делаем
            }
        }

        public static void LogMessage(string message, string context = "")
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("======================================");
                sb.AppendLine($"TIME: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

                if (!string.IsNullOrWhiteSpace(context))
                    sb.AppendLine($"CONTEXT: {context}");

                sb.AppendLine($"MESSAGE: {message}");
                sb.AppendLine("======================================");
                sb.AppendLine();

                lock (SyncRoot)
                {
                    File.AppendAllText(LogFile, sb.ToString());
                }
            }
            catch
            {
                // Если даже лог записать не можем - ничего не делаем
            }
        }
    }
}
