namespace Valuator
{
    public class RegionTypes
    {
        public const string RUS = "DB_RUS";
        public const string EU = "DB_EU";
        public const string OTHER = "DB_OTHER";
        public const string SEGMENTER = "DB_SEGMENTER";

        public static readonly Dictionary<string, string> COUNTRY_TO_REGION = new()
        {
            ["russia"] = RUS,
            ["france"] = EU,
            ["germany"] = EU,
            ["usa"] = OTHER,
            ["india"] = OTHER
        };
    }
}
