using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Yags.Core;
using Yags.Log;

namespace Yags.Http
{
    public class HttpHandler
    {
        private Dictionary<string, HttpFile> _files;

        private MethodRunner _runner;
        private DisconnectHandler _disconnectHandler;
        private readonly LoggerFunc _logger;

        public HttpHandler(LoggerFactoryFunc loggerFactory, Dictionary<string, HttpFile> files, MethodRunner runner)
        {
            _files = files;
            _runner = runner;
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());
        }

        public DisconnectHandler DisconnectHandler
        {
            get { return _disconnectHandler; }
            set { _disconnectHandler = value; }
        }

        public async Task Handle(HttpListenerContext context)
        {
            var sw = Stopwatch.StartNew();
            context.Response.ProtocolVersion = new Version(1, 1);
            context.Response.KeepAlive = context.Request.KeepAlive;
            //context.Response.SendChunked = true;
            var token = _disconnectHandler.GetDisconnectToken(context);

            HttpFile result;

            if (_files.TryGetValue(context.Request.RawUrl, out result))
            {
                if (result.MimeType != null)
                {
                    context.Response.ContentType = result.MimeType;
                }
                await ReturnBytes(result.Data, context.Response, token);
                return;
            }

            byte[] response;
            try
            {
                var request = context.Request;
                var buffer = new byte[request.ContentLength64];
                await request.InputStream.ReadAsync(buffer, 0, (int)request.ContentLength64, token);
                response = await _runner.Execute(buffer, token);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException exception)
            {
                LogHelper.LogException(_logger, string.Format("Http Request was not processed in time.\nTime:{0}ms\nRequest:{1}", sw.ElapsedMilliseconds, context.Request.RawUrl), exception);
                return;
            }
            LogHelper.LogVerbose(_logger, string.Format("User request processed in {0}ms", sw.ElapsedMilliseconds));
            sw = Stopwatch.StartNew();

            await ReturnBytes(response, context.Response, token);

            LogHelper.LogVerbose(_logger, string.Format("Response sent in {0}ms", sw.ElapsedMilliseconds));
        }

        private async Task ReturnBytes(byte[] bytes, HttpListenerResponse response, CancellationToken cancellationToken)
        {
            await ReturnResponse(bytes, (int)HttpStatusCode.OK, response, cancellationToken);
        }

        private async Task ReturnResponse(byte[] bytes, int statusCode, HttpListenerResponse response, CancellationToken cancellationToken)
        {
            response.StatusCode = statusCode;
            response.ContentLength64 = bytes.LongLength;
            try
            {
                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                await response.OutputStream.FlushAsync(cancellationToken);
                response.Close();
            }
            catch (OperationCanceledException)
            {
            }
            catch (HttpListenerException)
            {
            }
        }
    }
}
