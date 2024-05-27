using NATS.Client;
using StackExchange.Redis;
using System.Text;
using Newtonsoft.Json;

namespace RankCalculator
{
    public class MessageModel
    {
        public string Id { get; set; }
        public string HostAndPort { get; set; }
        public string Region { get; set; }
    }

    class RankCalculator
    {
        static void Main(string[] args)
        {
            ConnectionFactory cf = new();
            using IConnection c = cf.CreateConnection("127.0.0.1:4222");

            var s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                var messageObject = JsonConvert.DeserializeObject<MessageModel>(
                    Encoding.UTF8.GetString(args.Message.Data)) ?? throw new Exception("Message is null");

                string id = messageObject.Id;
                string hostAndPort = messageObject.HostAndPort;
                string region = messageObject.Region;

                IDatabase db = ConnectionMultiplexer.Connect(hostAndPort).GetDatabase();

                string textKey = "TEXT-" + id;
                string? text = db.StringGet(textKey);
                if (text == null)
                {
                    throw new Exception($"Not found by key {textKey}");
                }

                string rank = GetRank(text).ToString();

                db.StringSetAsync("RANK-" + id, rank);

                Console.WriteLine($"Calculated rank for id {id} and region {region}");

                // Публикация в nats
                var msgBytes = Encoding.UTF8.GetBytes($"{id};{rank}");
                c.Publish("RankCalculated", msgBytes);

            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            s.Unsubscribe();

            c.Drain();
            c.Close();
        }

        private static double GetRank(string text)
        {
            double notLetterCharacters = 0;
            foreach (var ch in text)
            {
                if (!char.IsLetter(ch))
                {
                    notLetterCharacters++;
                }
            }
            return notLetterCharacters / text.Length;
        }
    }
}