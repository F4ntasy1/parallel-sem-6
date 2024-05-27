using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration? _configuration;

        public Repository(IConfiguration? configuration = null)
        {
            _configuration = configuration;
            _regionToDbConection.Add(
                RegionTypes.RUS,
                ConnectionMultiplexer.Connect(configuration.GetConnectionString(RegionTypes.RUS))
            );
            _regionToDbConection.Add(
                RegionTypes.EU,
                ConnectionMultiplexer.Connect(configuration.GetConnectionString(RegionTypes.EU))
            );
            _regionToDbConection.Add(
                RegionTypes.OTHER,
                ConnectionMultiplexer.Connect(configuration.GetConnectionString(RegionTypes.OTHER))
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

            _dbSegmenter = ConnectionMultiplexer.Connect(configuration.GetConnectionString("db_segmenter")).GetDatabase();
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

        public void StoreText(string id, string text, string country)
        {
            string region = RegionTypes.COUNTRY_TO_REGION[country];

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

        public List<string> GetValuesByKey(string key) //TEXT-
        {
            if (_configuration == null)
            {
                throw new Exception("Configuration is null");
            }

            List<string> result = [];
            
            foreach (var connection in _regionToDbConection)
            {
                var keys = connection.Value.GetServer(_configuration.GetConnectionString(connection.Key)).Keys();

                foreach (string? k in keys)
                {
                    if (k != null && k.Contains(key))
                    {
                        var val = _regionToDbInstance[connection.Key].StringGet(key);
                        if (!val.IsNull)
                        {
                            result.Add(val);
                        }
                    }
                }
            }

            return result;
        }

        private List<string> GetIds(string region)
        {
            var ids = _regionToDbInstance[region].StringGet("IDS");
            return JsonSerializer.Deserialize<List<string>>(ids);
        }

        private void LogLookup(string id, string region)
        {
            Console.WriteLine($"[Lookup] id - {id}, region - {region}");
        }
    }
}
