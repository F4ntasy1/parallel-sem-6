namespace Valuator
{
    public class RegionTypes
    {
        public const string RUS = "db_rus";
        public const string EU = "db_eu";
        public const string OTHER = "db_other";

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
