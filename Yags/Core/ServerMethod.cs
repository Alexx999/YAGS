using System;
using System.Linq;
using System.Linq.Expressions;
using Yags.Extensions;
using Yags.Log;
using Yags.Session;

namespace Yags.Core
{
    public abstract class ServerMethod
    {
        private readonly Delegate _executionDelegate;
        private LoggerFunc _logger;
        private SessionController _sessionController;
        private Type[] _arguments;
        private string[] _bindings;

        protected ServerMethod(LoggerFunc logger, SessionController sessionController, string[] bindings)
        {
            _logger = logger;
            _sessionController = sessionController;
            _bindings = bindings;
            var method = GetType().GetMethod("Execute");

            if (method == null)
            {
                LogHelper.LogCritical(_logger, string.Format("Class {0} is missing method \"Execute\"", GetType().FullName));
                throw new MissingMethodException("Missing \"Execute\" method");
            }

            _arguments = method.GetParameters().Select(p => p.ParameterType).ToArray();
            _executionDelegate =
                method.CreateDelegate(Expression.GetDelegateType(method.GetParameters().Select(p => p.ParameterType)
                    .Concat(method.ReturnType.Yield()).ToArray()), this);
        }

        public string[] Bindings
        {
            get { return _bindings; }
        }

        public Delegate ExecutionDelegate
        {
            get { return _executionDelegate; }
        }

        public Type[] Arguments
        {
            get { return _arguments; }
        }

        protected LoggerFunc Logger
        {
            get { return _logger; }
        }

        protected SessionController SessionController
        {
            get { return _sessionController; }
        }
    }
}
