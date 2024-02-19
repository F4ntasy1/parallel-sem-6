using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace Valuator
{
    public class Repository
    {
        private readonly IDatabase m_db;
        private readonly IServer m_server;

        public Repository()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            m_server = redis.GetServer("localhost", 6379);
            IDatabase db = redis.GetDatabase();

            if (db == null)
            {
                throw new Exception("DB is null");
            }
            m_db = db;
        }

        public string? Get(string key)
        {
            return m_db.StringGet(key);
        }

        public void Set(string key, string value)
        {
            m_db.StringSet(key, value);
        }

        public void Set(string key, double value)
        {
            m_db.StringSet(key, value);
        }

        public void Set(string key, bool value)
        {
            m_db.StringSet(key, value);
        }

        public List<string> GetValuesByKey(string key)
        {
            List<string> result = [];
            foreach (var k in GetAllKeys())
            {
                if (k.Contains(key))
                {
                    result.Add(m_db.StringGet(k));
                }
            }
            return result;
        }

        private List<string> GetAllKeys()
        {
            List<string> listKeys = [];
            var keys = m_server.Keys();
            foreach (string key in keys)
            {
                if (key != null)
                {
                    listKeys.Add(key);
                }
            }
            return listKeys;
        }
    }
}
