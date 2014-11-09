using System;

namespace Yags.Session
{
    public class UserSessionData
    {
        public UserSessionData(string key, Guid userId)
        {
            UserId = userId;
            Key = key;
        }

        public Guid UserId { get; private set; }
        public string Key { get; private set; }
    }
}
