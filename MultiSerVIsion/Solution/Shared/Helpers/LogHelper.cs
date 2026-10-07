using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Shared.Helpers
{
    public static class LogHelper
    {
        private static readonly string _logPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            GlobalConst.AppDataFolder, "runtime.Log");

        public static void Info(string content)
        {
            WriteLog($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {content}");
        }

        /// <summary>
        /// 记录错误日志。
        /// 【空值契约】异常对象允许为 null：部分调用点只需记录业务失败原因（如“参数下发失败”），
        /// 此时没有异常对象；若直接访问 ex.Message / ex.StackTrace 会抛 NullReferenceException，
        /// 从而掩盖真正的失败原因。
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="ex">关联异常，可为 null</param>
        public static void Error(string content, Exception ex)
        {
            // 异常为空时只输出内容，避免空引用导致日志本身成为故障点
            string detail = ex == null
                ? string.Empty
                : $" | 异常：{ex.Message}\n{ex.StackTrace}";

            WriteLog($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {content}{detail}");
        }

        private static void WriteLog(string msg)
        {
            lock (_logPath)
            {
                string dir = Path.GetDirectoryName(_logPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(_logPath, msg + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
