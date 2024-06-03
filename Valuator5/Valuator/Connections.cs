namespace Valuator
{
    public class Connections
    {
        public static readonly Dictionary<string, string> REGION_TO_CONNECTION = new()
        {
            [RegionTypes.RUS] = "127.0.0.1:6456",
            [RegionTypes.EU] = "127.0.0.1:6457",
            [RegionTypes.OTHER] = "127.0.0.1:6458",
        };
    }
}
