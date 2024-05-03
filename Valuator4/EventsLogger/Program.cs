using NATS.Client;
using System.Text;

namespace EventsLogger
{
    class Program
    {
        public static void Main(string[] args)
        {
            IConnection c = new ConnectionFactory().CreateConnection();

            c.SubscribeAsync("RankCalculated", (sender, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Message.Data);
                string[] splittedMsg = message.Split(';');

                Console.WriteLine(message);

                if (splittedMsg.Length < 2)
                {
                    throw new ArgumentException("Wrong message format");
                }

                var rankKey = splittedMsg[0];
                var rank = splittedMsg[1];

                Console.WriteLine("RankCalculated");
                Console.WriteLine(rankKey);
                Console.WriteLine($"rank: {rank}");
            });

            c.SubscribeAsync("SimilarityCalculated", (sender, args) =>
            {
                var messageBytes = args.Message.Data;

                string message = Encoding.UTF8.GetString(messageBytes);
                string[] splittedMsg = message.Split(';');

                if (splittedMsg.Length < 2)
                {
                    throw new ArgumentException("Wrong message format");
                }

                var similarityKey = splittedMsg[0];
                var similarity = splittedMsg[1];

                Console.WriteLine("SimilarityCalculated");
                Console.WriteLine(similarityKey);
                Console.WriteLine($"similarity: {similarity}");
            });

            Console.WriteLine("[Events] Ожидание сообщений...");
            Console.ReadLine();
        }
    }
}