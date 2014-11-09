using System;
using System.Collections.Concurrent;
using Yags.Annotations;
using Yags.Log;

namespace Yags.Session
{
    public class SessionController
    {
        private ConcurrentDictionary<Guid, UserSession> _sessions = new ConcurrentDictionary<Guid, UserSession>();
        private const int SessionTimeout = 1*60*1000;
        private LoggerFunc _logger;

        public SessionController([CanBeNull] LoggerFactoryFunc loggerFactory)
        {
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());
        }

        public bool CheckSession([CanBeNull] UserSessionData sessionData)
        {
            var session = GetSession(sessionData);
            var result = session != null;
            return result;
        }

        public UserSession GetSession([CanBeNull] UserSessionData sessionData)
        {
            if(sessionData == null) return null;
            UserSession session;

            if (!_sessions.TryGetValue(sessionData.UserId, out session))
            {
                return null;
            }

            if (session.Key != sessionData.Key) return null;

            session.ResetTimeout();
            return session;
        }

        public UserSessionData CreateSession(Guid userId)
        {
            var session = new UserSession(userId, SessionTimeout, TimeoutCallback);
            _sessions.AddOrUpdate(userId, session, (guid, userSession) =>
            {
                userSession.Dispose();
                return session;
            });
            return session.SessionData;
        }

        private void TimeoutCallback(object state)
        {
            var session = (UserSession) state;
            if (!_sessions.TryRemove(session.UserId, out session)) return;
            LogHelper.LogVerbose(_logger, string.Format("User {0} disconnected by timeout", session.UserId));
            session.Dispose();
        }
    }
}
