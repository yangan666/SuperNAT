using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Repository;

public class Log4netUtil
{
    public static ILoggerRepository LogRepository { get; set; }
    private static ILog log { get; set; }

    private static ILog GetLog()
    {
        try
        {
            if (log == null)
            {
                log = LogManager.GetLogger(LogRepository.Name, "NETCorelog4net");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"初始化日志失败：{ex}");
            return null;
        }
        return log;
    }

    /// <summary>
    /// Fatals log
    /// </summary>
    /// <param name="pMethod">method</param>
    /// <param name="pMessage">message</param>
    public static void Fatal(string pMessage)
    {
        ILog log = GetLog();
        if (log.IsFatalEnabled)
        {
            log.Fatal(pMessage);
        }
    }

    /// <summary>
    /// Fatals log
    /// </summary>
    /// <param name="pMethod">method</param>
    /// <param name="pMessage">message</param>
    /// <param name="Exception">ec</param>
    public static void Fatal(string pMessage, Exception ec)
    {
        ILog log = GetLog();
        if (log.IsFatalEnabled)
        {
            log.Fatal(pMessage + "," + ec.ToString());
        }
    }

    /// <summary>
    /// Erros log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    public static void Error(string pMessage)
    {
        ILog log = GetLog();
        if (log?.IsErrorEnabled ?? false)
        {
            log.Error(pMessage + Environment.NewLine);
        }
    }

    public static void Error(Exception ex)
    {
        ILog log = GetLog();
        if (log?.IsErrorEnabled ?? false)
        {
            log.Error((ex.InnerException == null ? ex.Message : ex.InnerException.Message) + Environment.NewLine);
        }
    }

    /// <summary>
    /// Erros log
    /// </summary>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Error(string pMessage, Exception ec)
    {
        ILog log = GetLog();
        if (log?.IsErrorEnabled ?? false)
        {
            log.Error(pMessage + "," + ec.ToString() + Environment.NewLine);
        }
    }

    /// <summary>
    /// Warning log
    /// </summary>
    /// <param name="pMessage">message.</param>
    public static void Warn(string pMessage)
    {
        ILog log = GetLog();
        if (log?.IsWarnEnabled ?? false)
        {
            log.Warn(pMessage);
        }
    }

    /// <summary>
    /// Warning log
    /// </summary>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Warn(string pMessage, Exception ec)
    {
        ILog log = GetLog();
        if (log?.IsWarnEnabled ?? false)
        {
            log.Warn(pMessage + "," + ec.ToString());
        }
    }

    /// <summary>
    /// Information log
    /// </summary>
    /// <param name="pMessage">message.</param>
    public static void Info(string pMessage)
    {
        ILog log = GetLog();
        if (log?.IsInfoEnabled ?? false)
        {
            log.Info(pMessage);
        }
    }

    /// <summary>
    /// Information log
    /// </summary>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Info(string pMessage, Exception ec)
    {
        ILog log = GetLog();
        if (log?.IsInfoEnabled ?? false)
        {
            log.Info(pMessage + "," + ec.ToString() + Environment.NewLine);
        }
    }

    /// <summary>
    /// API 错误信息记录
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="apiMethod">API接口</param>
    /// <param name="callMethod">所调用数据层方法</param>
    /// <param name="param">调用方法所传递参数</param>
    public static void DebugInfo(string message, string apiMethod, string callMethod, params string[] param)
    {
        ILog log = GetLog();
        if (log?.IsDebugEnabled ?? false)
        {
            log.Info("错误信息:" + message + ",接口:" + apiMethod + "调用方法:" + callMethod + ",参数：" + string.Concat(param));
        }
    }

    /// <summary>
    /// Debugging log
    /// </summary>
    /// <param name="pMessage">message.</param>
    public static void Debug(string pMessage)
    {
        ILog log = GetLog();
        if (log?.IsDebugEnabled ?? false)
        {
            log.Debug(pMessage);

        }
    }

    /// <summary>
    /// Debugging log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="Exception">ec.</param>
    /// 
    public static void Debug(string pMessage, Exception ec)
    {
        ILog log = GetLog();
        if (log?.IsDebugEnabled ?? false)
        {
            log.Debug(pMessage + "," + ec.ToString());
        }
    }
}