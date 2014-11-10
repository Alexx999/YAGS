using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yags.Annotations;
using Yags.Log;

namespace Yags.Core
{
    public class JsonMethodRunner : MethodRunner
    {
        public JsonMethodRunner(LoggerFactoryFunc loggerFactory, IEnumerable<ServerMethod> methods) : base(loggerFactory, methods)
        {
        }

        public override async Task<byte[]> Execute(byte[] data, CancellationToken token)
        {
            var requestStr = Encoding.UTF8.GetString(data);
            var requestObj = JsonConvert.DeserializeObject<ClientRequest>(requestStr);
            var result = await ExecuteInternal(requestObj, token);
            var resultStr = JsonConvert.SerializeObject(result);
            var resultBytes = Encoding.UTF8.GetBytes(resultStr);
            return resultBytes;
        }

        private async Task<MethodResult> ExecuteInternal(ClientRequest request, CancellationToken token)
        {
            ServerMethod method;

            if (!_methods.TryGetValue(request.Func, out method))
            {
                return MethodResult.Fail;
            }

            var executionDelegate = method.ExecutionDelegate;

            var args = EmptyArgs;
            var methodArgs = method.Arguments;
            try
            {
                if (methodArgs.Length > 0)
                {
                    args = GetArgsForMethod(request, methodArgs, token);
                }
            }
            catch (Exception exception)
            {
                LogHelper.LogException(_logger, "Failed to process arguments for request\n" + request.Func + " " + request.Args, exception);
                return MethodResult.Fail;
            }

            object resultObject;

            try
            {
                resultObject = executionDelegate.DynamicInvoke(args);

                if (resultObject is Task)
                {
                    resultObject = await ProcessTask(resultObject as Task);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogHelper.LogException(_logger, request.Func, ex);
                return MethodResult.Fail;
            }

            var result = resultObject as MethodResult ?? new MethodResult<object>(resultObject);

            return result;
        }

        private object[] GetArgsForMethod(ClientRequest request, Type[] methodArgs, CancellationToken token)
        {
            var paramCount = methodArgs.Length;

            var result = new object[paramCount];

            if (methodArgs.Last() == typeof(CancellationToken))
            {
                result[paramCount - 1] = token;
                paramCount -= 1;
            }

            var argArray = request.Args;

            if (paramCount > 0 && (argArray == null || argArray.Count < paramCount))
            {
                return null;
            }

            var strArgs = new JToken[paramCount];

            for (int i = 0; i < paramCount; i++)
            {
                var arg = argArray[i];
                if (arg == null)
                {
                    return null;
                }
                strArgs[i] = arg;
            }
            for (int i = 0; i < paramCount; i++)
            {
                var tgtType = methodArgs[i];
                if (tgtType == typeof(string))
                {
                    result[i] = strArgs[i].ToString();
                    continue;
                }
                if (tgtType == typeof(JObject))
                {
                    result[i] = strArgs[i] as JObject;
                    continue;
                }
                if (tgtType == typeof(JArray))
                {
                    result[i] = strArgs[i] as JArray;
                    continue;
                }

                result[i] = strArgs[i].ToObject(tgtType);
            }

            return result;
        }


        [UsedImplicitly]
        public struct ClientRequest
        {
            [NotNull, UsedImplicitly]
            public string Func;
            [CanBeNull, UsedImplicitly]
            public JArray Args;
        }
    }
}
