using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// 日志操作类委托，具体方法用户自行定义
/// </summary>
public class LogHelper
{
    /// <summary>
    /// 日志委托，通过委托将代码所有的日志通过同一个方法打印到控制台和输出到文件
    /// </summary>
    public static Action<LogLevel, string, bool> WriteLog { get; set; }
    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="logLevel">日志等级</param>
    /// <param name="log">日志内容</param>
    /// <param name="isPrint">是否输出控制台</param>
    public static void Log(LogLevel logLevel, string log, bool isPrint = true)
    {
        WriteLog?.Invoke(logLevel, log, isPrint);
    }

    public static void Trace(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Trace, log, isPrint);
    }

    public static void Debug(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Debug, log, isPrint);
    }

    public static void Info(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Information, log, isPrint);
    }

    public static void Warning(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Warning, log, isPrint);
    }

    public static void Error(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Error, log, isPrint);
    }

    public static void Fatal(string log, bool isPrint = true)
    {
        WriteLog?.Invoke(LogLevel.Critical, log, isPrint);
    }

    /// <summary>
    /// 获取日志等级的字符串
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public static string GetString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trace: ",
            LogLevel.Debug => "debug: ",
            LogLevel.Information => "info: ",
            LogLevel.Warning => "warn: ",
            LogLevel.Error => "error: ",
            LogLevel.Critical => "fatal: ",
            LogLevel.None => "none: ",
            _ => $"{logLevel.ToString().ToLower()}: ",
        };
    }
}
