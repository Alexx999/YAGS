using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Yags.Core;
using Yags.Log;

namespace Yags.Http
{
    public class HttpServer : Core.Server
    {
        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
        private const long DefaultRequestQueueLength = 1000;  // Http.sys default.

        private HttpListener _listener;

        private int _currentOutstandingAccepts;
        private int _currentOutstandingRequests;
        private long? _requestQueueLength;

        private HttpHandler _handler;

        public HttpServer(LoggerFactoryFunc loggerFactory, HttpHandler handler):base(loggerFactory)
        {
            _handler = handler;
            _listener = new HttpListener();
            _startNextRequestAsync = new Action(ProcessRequestsAsync);
            _startNextRequestError = new Action<Task>(StartNextRequestError);
            SetRequestProcessingLimits(DefaultMaxAccepts, DefaultMaxRequests);
        }

        public void SetRequestQueueLimit(long limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException("limit", limit, string.Empty);
            }
            if ((!_requestQueueLength.HasValue && limit == DefaultRequestQueueLength)
                || (_requestQueueLength.HasValue && limit == _requestQueueLength.Value))
            {
                return;
            }

            _requestQueueLength = limit;

            SetRequestQueueLimit();
        }

        private void StartNextRequestError(Task faultedTask)
        {
            // StartNextRequestAsync should handle it's own exceptions.
            LogHelper.LogException(_logger, "Unexpected exception.", faultedTask.Exception);
            Contract.Assert(false, "Un-expected exception path: " + faultedTask.Exception);
        }

        private async void ProcessRequestsAsync()
        {
            while (_listener.IsListening && CanAcceptMoreRequests())
            {
                Interlocked.Increment(ref _currentOutstandingAccepts);

                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (ApplicationException ae)
                {
                    // These come from the thread pool if HttpListener tries to call BindHandle after the listener has been disposed.
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    LogHelper.LogException(_logger, "Accept", ae);
                    return;
                }
                catch (HttpListenerException hle)
                {
                    // These happen if HttpListener has been disposed
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    LogHelper.LogException(_logger, "Accept", hle);
                    return;
                }
                catch (ObjectDisposedException ode)
                {
                    // These happen if HttpListener has been disposed
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    LogHelper.LogException(_logger, "Accept", ode);
                    return;
                }

                Interlocked.Decrement(ref _currentOutstandingAccepts);
                Interlocked.Increment(ref _currentOutstandingRequests);
                StartListening();

                // This needs to be separate from ProcessRequestsAsync so that async/await will clean up the execution context.
                // This prevents changes to Thread.CurrentPrincipal from leaking across requests.
                await _handler.Handle(context);
            }
        }

        protected override bool CanAcceptMoreRequests()
        {
            PumpLimits limits = _pumpLimits;
            return (_currentOutstandingAccepts < limits.MaxOutstandingAccepts
                && _currentOutstandingRequests < limits.MaxOutstandingRequests - _currentOutstandingAccepts);
        }

        protected override bool IsListening()
        {
            return _listener.IsListening;
        }

        public override void Start(int port)
        {
            var urls = new List<string> {string.Format("http://+:{0}/", port)};
            Contract.Assert(urls != null);
            Contract.Assert(urls.Count > 0);

            foreach (var url in urls)
            {
                _listener.Prefixes.Add(url);
            }

            if (!_listener.IsListening)
            {
                _listener.Start();
            }

            SetRequestQueueLimit();

            _handler.DisconnectHandler = new DisconnectHandler(_listener, _logger);

            StartListening();
        }

        private void SetRequestQueueLimit()
        {
            // The listener must be active for this to work.  Call from Start after activating.
            // Platform check. This isn't supported on XP / Http.Sys v1.0, or Mono.
            if (IsMono || !_listener.IsListening || !_requestQueueLength.HasValue || Environment.OSVersion.Version.Major < 6)
            {
                return;
            }

            NativeMethods.SetRequestQueueLength(_listener, _requestQueueLength.Value);
        }

        public override void Dispose()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            ((IDisposable)_listener).Dispose();
        }
    }
}
