using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Yags.Log;
using Yags.Session;

namespace Yags.Core
{
    public static class ServerMethodFactory
    {
        public static IReadOnlyList<ServerMethod> GetServerMethods(Assembly targetAssembly, LoggerFactoryFunc factory, SessionController sessionController)
        {
            var logger = LogHelper.CreateLogger(factory, typeof (ServerMethodFactory));

            var constructorTypes = new[]{typeof(LoggerFunc), typeof(SessionController)};
            var constructorArgs = new object[] { logger, sessionController };
            var methods = targetAssembly.DefinedTypes.AsParallel()
                .Where(t => !t.IsDefined(typeof(ServerMethodDisabledAttribute)) && t.IsSubclassOf(typeof(ServerMethod)))
                .Select(t => t.GetConstructor(constructorTypes))
                .Select(c => c.Invoke(constructorArgs) as ServerMethod).ToList();
            return methods;
        }
    }
}
