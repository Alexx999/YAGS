using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yags.Annotations;
using Yags.Log;

namespace Yags.Core
{
    public abstract class MethodRunner
    {
        protected readonly LoggerFunc _logger;
        protected readonly Dictionary<string, ServerMethod> _methods = new Dictionary<string, ServerMethod>();
        private static Type _voidTaskType;
        private static object _syncRoot = new object();
        protected static readonly object[] EmptyArgs = new object[0];

        protected MethodRunner(LoggerFactoryFunc loggerFactory, IEnumerable<ServerMethod> methods)
        {
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());

            foreach (var method in methods)
            {
                foreach (var binding in method.Bindings)
                {
                    if (_methods.ContainsKey(binding))
                    {
                        LogHelper.LogCritical(_logger,
                            string.Format("Http binding conflict.\nBinding name: \"{0}\"\nMethod1:{1}\nMethod2:{2}",
                                binding, method.GetType().FullName, _methods[binding].GetType().FullName));
                    }

                    _methods[binding] = method;
                }
            }
        }

        public abstract Task<byte[]> Execute(byte[] data, CancellationToken token); 

        private static bool IsVoidTask([NotNull] Task task)
        {
            var taskType = task.GetType();
            if (_voidTaskType != null)
            {
                return taskType == _voidTaskType;
            }
            lock (_syncRoot)
            {
                if (_voidTaskType != null)
                {
                    return taskType == _voidTaskType;
                }
                var voidTask = Task.Delay(0);
                _voidTaskType = voidTask.GetType();
                voidTask.Wait();
                return taskType == _voidTaskType;
            }
        }

        protected static async Task<object> ProcessTask([NotNull] Task task)
        {
            await task;

            if (IsVoidTask(task))
            {
                return null;
            }

            var result = task.GetType().GetMethod("get_Result").Invoke(task, new object[0]);

            return result;
        }
    }
}
