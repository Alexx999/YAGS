using System;
using System.Threading.Tasks;
using Yags.Log;

namespace Yags.Core
{
    public abstract class Server : IDisposable
    {
        protected const int DefaultMaxRequests = Int32.MaxValue;
        protected static readonly int DefaultMaxAccepts = 5 * Environment.ProcessorCount;
        protected PumpLimits _pumpLimits;
        protected Action _startNextRequestAsync;
        protected Action<Task> _startNextRequestError;
        protected LoggerFunc _logger;

        protected Server(LoggerFactoryFunc loggerFactory)
        {
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void SetRequestProcessingLimits(int maxAccepts, int maxRequests)
        {
            _pumpLimits = new PumpLimits(maxAccepts, maxRequests);

            if (IsListening())
            {
                // Kick the pump in case we went from zero to non-zero limits.
                StartListening();
            }
        }
        protected void StartListening()
        {
            if (IsListening() && CanAcceptMoreRequests())
            {
                Task.Factory.StartNew(_startNextRequestAsync)
                    .ContinueWith(_startNextRequestError, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        protected abstract bool CanAcceptMoreRequests();

        protected abstract bool IsListening();

        public abstract void Start(int port);
    }

    public class PumpLimits
    {
        internal PumpLimits(int maxAccepts, int maxRequests)
        {
            MaxOutstandingAccepts = maxAccepts;
            MaxOutstandingRequests = maxRequests;
        }

        internal int MaxOutstandingAccepts { get; private set; }

        internal int MaxOutstandingRequests { get; private set; }
    }
}
