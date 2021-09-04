using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Logs
{
    public static class Logger
    {
        static Logger()
        {
            RawWrite((f, m) => File.WriteAllText(f, m), string.Empty);
        }

        public static void Log(string logMessage)
        {
            WriteLogLine(string.Format("[INFO] {0}", logMessage));
        }

        public static void LogWarning(string logMessage)
        {
            WriteLogLine(string.Format("[WARNING] {0}", logMessage));
        }

        public static void LogError(string logMessage)
        {
            WriteLogLine(string.Format("[ERROR] {0}", logMessage));
        }

        private static string Prefix()
        {
            return string.Format("[{0}]", DateTime.Now.ToString());
        }

        private static string GetFileName() => String.Format("{0}.log", Assembly.GetExecutingAssembly().GetName().Name);

        private static void WriteLogLine(string message)
        {
            string fullMessage = string.Format("{0}{1}\n", Prefix(), message);
            RawWrite((f, m) => File.AppendAllText(f, m), fullMessage);
        }

        private static void RawWrite(Action<string, string> writer, string message)
        {
            if (Directory.Exists("Content/mods"))
                writer?.Invoke(String.Format("{0}/{1}", "Content/mods", GetFileName()), message);
            else
                writer?.Invoke(GetFileName(), message);
        }
    }
}
