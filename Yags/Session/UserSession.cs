using System;
using System.Threading;

namespace Yags.Session
{
    public class UserSession : IDisposable
    {
        private static Random _rnd = new Random();

        private string _key;
        private Guid _userId;
        private UserSessionData _sessionData;
        private Timer _inactivityTimer;
        private readonly int _timeout;

        public string Key
        {
            get { return _key; }
        }

        public Guid UserId
        {
            get { return _userId; }
        }

        public UserSessionData SessionData { get { return _sessionData; }}

        public UserSession(Guid userId, int timeout, TimerCallback timeoutCallback)
        {
            _userId = userId;
            _timeout = timeout;
            _key = GetSessionKey();
            _sessionData = new UserSessionData(_key, userId);
            _inactivityTimer = new Timer(timeoutCallback, this, timeout, Timeout.Infinite);
        }

        public void ResetTimeout()
        {
            _inactivityTimer.Change(_timeout, Timeout.Infinite);
        }

        private static string GetSessionKey()
        {
            var key = new byte[16];
            lock (_rnd)
            {
                _rnd.NextBytes(key);
            }

            return Convert.ToBase64String(key);
        }

        public void Dispose()
        {
            _inactivityTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
