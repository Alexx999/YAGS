using System.Diagnostics;
using NLog;
using Yags.Annotations;

namespace Yags.Log
{
    public static class NLogFactory
    {
        public static LoggerFunc Create(string name)
        {
            var logger = LogManager.GetLogger(name);

            return (type, id, state, exception, formatter) =>
            {
                logger.Log(type.ToLogLevel(), formatter(state, exception));
                return true;
            };
        }

        [NotNull]
        private static LogLevel ToLogLevel(this TraceEventType traceEventType)
        {
            switch (traceEventType)
            {
                case TraceEventType.Critical:
                {
                    return LogLevel.Fatal;
                }
                case TraceEventType.Error:
                {
                    return LogLevel.Error;
                }
                case TraceEventType.Information:
                {
                    return LogLevel.Info;
                }
                case TraceEventType.Resume:
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Transfer:
                case TraceEventType.Verbose:
                {
                    return LogLevel.Trace;
                }
                case TraceEventType.Warning:
                {
                    return LogLevel.Warn;
                }
                default:
                {
                    return LogLevel.Off;
                }
            }
        }
    }
}
