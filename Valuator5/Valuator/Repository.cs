using Microsoft.Extensions.Configuration;
using NATS.Client;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System.Text.Json;

namespace Valuator
{
    public class Repository
    {
        private readonly Dictionary<string, ConnectionMultiplexer> _regionToDbConection = [];
        private readonly Dictionary<string, IDatabase> _regionToDbInstance = [];

        private readonly IDatabase _dbSegmenter;

        public Repository()
        {
            _regionToDbConection.Add(
                RegionTypes.RUS,
                ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RegionTypes.RUS)!)
            );
            _regionToDbConection.Add(
                RegionTypes.EU,
                ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RegionTypes.EU)!)
            );
            _regionToDbConection.Add(
                RegionTypes.OTHER,
                ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RegionTypes.OTHER)!)
            );

            _regionToDbInstance.Add(
                RegionTypes.RUS,
                _regionToDbConection[RegionTypes.RUS].GetDatabase()
            );
            _regionToDbInstance.Add(
                RegionTypes.EU,
                _regionToDbConection[RegionTypes.EU].GetDatabase()
            );
            _regionToDbInstance.Add(
                RegionTypes.OTHER,
                _regionToDbConection[RegionTypes.OTHER].GetDatabase()
            );

            _dbSegmenter = ConnectionMultiplexer.Connect(
                Environment.GetEnvironmentVariable(RegionTypes.SEGMENTER)!
            ).GetDatabase();
        }

        public string? Get(string id, string keyName)
        {
            string? region = _dbSegmenter.StringGet(id);
            if (region == null)
            {
                throw new Exception($"Region not found by id {id}");
            }

            LogLookup(id, region);
            return _regionToDbInstance[region].StringGet($"{keyName}-{id}");
        }

        public void StoreText(string id, string text, string region)
        {
            LogLookup(id, region);
            _regionToDbInstance[region].StringSet($"TEXT-{id}", text);
            _dbSegmenter.StringSet(id, region);

            var idsList = GetIds(region);
            idsList.Add(id);
            _regionToDbInstance[region].StringSet("IDS", JsonSerializer.Serialize(idsList));
        }

        public void StoreRank(string id, double rank)
        {
            string? region = _dbSegmenter.StringGet(id);
            if (region == null)
            {
                throw new Exception($"Region not found by id {id}");
            }

            LogLookup(id, region);
            _regionToDbInstance[region].StringSet($"RANK-{id}", rank);
        }

        public void StoreSimilarity(string id, int similarity)
        {
            string? region = _dbSegmenter.StringGet(id);
            if (region == null)
            {
                throw new Exception($"Region not found by id {id}");
            }

            LogLookup(id, region);
            _regionToDbInstance[region].StringSet($"SIMILARITY-{id}", similarity);
        }

        public List<string> GetValuesByKey(string key, string region) //TEXT-
        {
            List<string> result = [];

            var connection = _regionToDbConection[region];

            var keys = connection.GetServer(Environment.GetEnvironmentVariable(region)!).Keys();

            foreach (string? k in keys)
            {
                if (k != null && k.Contains(key))
                {
                    var val = _regionToDbInstance[region].StringGet(k);
                    if (!val.IsNull)
                    {
                        result.Add(val);
                    }
                }
            }

            return result;
        }

        private List<string> GetIds(string region)
        {
            var ids = _regionToDbInstance[region].StringGet("IDS");
            if (ids.IsNull)
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<string>>(ids);
        }

        private void LogLookup(string id, string region)
        {
            Console.WriteLine($"[Lookup] id - {id}, region - {region}");
        }
    }
}
