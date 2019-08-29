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
    //"true":打开调试日志;"false":关闭调试日志
    private static bool isDebugger()
    {
        bool isDebug = true;
        //string openState = System.Configuration.ConfigurationManager.AppSettings["isDebugger"].ToString();
        //if (openState == "true")
        //{
        //    isDebug = true;
        //}
        //else if (openState == "false")
        //{
        //    isDebug = false;
        //}
        return isDebug;
    }

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
            throw ex;
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
        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsFatalEnabled)
            {
                log.Fatal(pMessage);
            }
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
        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsFatalEnabled)
            {
                log.Fatal(pMessage + "," + ec.ToString());
            }
        }
    }

    /// <summary>
    /// Erros log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    public static void Error(string pMessage)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsErrorEnabled)
            {
                log.Error(pMessage + Environment.NewLine);
            }
        }
    }

    public static void Error(Exception ex)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsErrorEnabled)
            {
                log.Error((ex.InnerException == null ? ex.Message : ex.InnerException.Message) + Environment.NewLine);
            }
        }
    }

    /// <summary>
    /// Erros log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Error(string pMessage, Exception ec)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsErrorEnabled)
            {
                log.Error(pMessage + "," + ec.ToString() + Environment.NewLine);
            }
        }
    }

    /// <summary>
    /// Warning log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    public static void Warn(string pMessage)
    {
        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsWarnEnabled)
            {
                log.Warn(pMessage);
            }
        }
    }

    /// <summary>
    /// Warning log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Warn(string pMessage, Exception ec)
    {
        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsWarnEnabled)
            {
                log.Warn(pMessage + "," + ec.ToString());
            }
        }
    }



    /// <summary>
    /// Information log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    public static void Info(string pMessage)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsInfoEnabled)
            {
                log.Info(pMessage);
            }
        }
    }

    /// <summary>
    /// Information log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    public static void Info(string pMessage, Exception ec)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsInfoEnabled)
            {
                log.Info(pMessage + "," + ec.ToString() + Environment.NewLine);
            }
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
        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsInfoEnabled)
            {
                log.Info("错误信息:" + message + ",接口:" + apiMethod + "调用方法:" + callMethod + ",参数：" + string.Concat(param));
            }
        }
    }

    /// <summary>
    /// Debugging log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    public static void Debug(string pMessage)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsDebugEnabled)
            {
                log.Debug(pMessage);

            }
        }
    }

    /// <summary>
    /// Debugging log
    /// </summary>
    /// <param name="pMethod">method.</param>
    /// <param name="pMessage">message.</param>
    /// <param name="Exception">ec.</param>
    /// 
    public static void Debug(string pMessage, Exception ec)
    {

        if (isDebugger())
        {
            ILog log = Log4netUtil.GetLog();
            if (log.IsDebugEnabled)
            {
                log.Debug(pMessage + "," + ec.ToString());

            }
        }
    }

    public static string GetMethodInfo()
    {
        string str = "";

        //取得当前方法命名空间    
        str += "命名空间名:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "\n";

        //取得当前方法类全名 包括命名空间    
        str += "类名:" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + "\n";

        //取得当前方法名    
        str += "方法名:" + System.Reflection.MethodBase.GetCurrentMethod().Name + "\n"; str += "\n";

        //父方法

        System.Diagnostics.StackTrace ss = new System.Diagnostics.StackTrace(true);
        System.Reflection.MethodBase mb = ss.GetFrame(1).GetMethod();

        //取得父方法命名空间    
        str += mb.DeclaringType.Namespace + "\n";

        //取得父方法类名    
        str += mb.DeclaringType.Name + "\n";

        //取得父方法类全名    
        str += mb.DeclaringType.FullName + "\n";

        //取得父方法名    
        str += mb.Name + "\n";
        return str;
    }

}