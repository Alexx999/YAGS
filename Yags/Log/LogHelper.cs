using System;
using System.Diagnostics;
using System.Globalization;
using Yags.Annotations;

namespace Yags.Log
{
    public delegate string FormatterFunc(object state, Exception exception);
    public delegate bool LoggerFunc(TraceEventType eventType, int eventId, object state, Exception exception, FormatterFunc formatter);
    public delegate LoggerFunc LoggerFactoryFunc(string name);

    public static class LogHelper
    {
        private static readonly FormatterFunc LogState =
            (state, error) => Convert.ToString(state, CultureInfo.CurrentCulture);

        private static readonly FormatterFunc LogStateAndError =
            (state, error) => string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", state, error);

        public static LoggerFunc CreateLogger([CanBeNull] LoggerFactoryFunc factory, [NotNull] Type type)
        {
            return factory == null ? null : factory(type.FullName);
        }

        public static void LogException([CanBeNull] LoggerFunc logger, string location, Exception exception)
        {
            if (logger == null)
            {
                Debug.WriteLine(LogStateAndError(location, exception));
            }
            else
            {
                logger(TraceEventType.Error, 0, location, exception, LogStateAndError);
            }
        }

        public static void LogInfo([CanBeNull] LoggerFunc logger, string data)
        {
            LogWithEventType(logger, TraceEventType.Information, data);
        }

        [Conditional("DEBUG")]
        public static void LogVerbose([CanBeNull] LoggerFunc logger, string data)
        {
            LogWithEventType(logger, TraceEventType.Verbose, data);
        }

        public static void LogCritical([CanBeNull] LoggerFunc logger, string data)
        {
            LogWithEventType(logger, TraceEventType.Critical, data);
        }

        public static void LogWarning([CanBeNull] LoggerFunc logger, string data)
        {
            LogWithEventType(logger, TraceEventType.Warning, data);
        }

        private static void LogWithEventType([CanBeNull] LoggerFunc logger, TraceEventType level, string data)
        {
            if (logger == null)
            {
                Debug.WriteLine(LogState(data, null));
            }
            else
            {
                logger(level, 0, data, null, LogState);
            }
        }
    }
}