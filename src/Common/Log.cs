using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace framer.Common
{
    public static class Logger
    {
        public static void Log(params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Join(" ", args));
        }

        public static void Warn(params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[WARNING] " + string.Join(" ", args));
        }

        public static void Error(params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] " + string.Join(" ", args));
        }

        public static void Info(params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[INFO] " + string.Join(" ", args));
        }

        public static void Debug(params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] " + string.Join(" ", args));
        }

        public static void Table(object[] array, params string[] properties)
        {
            if (array.Length > 0)
            {
                string[] header = properties.Length > 0 ? properties : array[0].GetType().GetProperties().Select(p => p.Name).ToArray();
                System.Diagnostics.Debug.WriteLine(string.Join(" | ", header));
                System.Diagnostics.Debug.WriteLine(new string('-', header.Sum(x => x.Length + 3) - 1));
                foreach (object item in array)
                {
                    System.Diagnostics.Debug.WriteLine(string.Join(" | ", header.Select(h => item.GetType().GetProperty(h).GetValue(item, null)?.ToString() ?? "")));
                }
            }
        }

        private static readonly Stopwatch stopwatch = new Stopwatch();
        public static void Time(string label)
        {
            System.Diagnostics.Debug.WriteLine($"[{label}] Started");
            stopwatch.Restart();
        }

        public static void TimeEnd(string label)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[{label}] Completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        public static void Trace()
        {
            StackTrace stackTrace = new StackTrace(true);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var fileName = frame?.GetFileName();
                int lineNumber = frame?.GetFileLineNumber()??0;
                System.Diagnostics.Debug.WriteLine($"at {method?.DeclaringType?.FullName}.{method?.Name} ({fileName}:{lineNumber})");
            }
        }
    }
}
