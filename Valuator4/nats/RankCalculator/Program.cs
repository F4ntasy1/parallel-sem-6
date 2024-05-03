using NATS.Client;
using System;
using System.Text;

namespace RankCalculator
{
    // #2 подумать про конкурирующих потребителей
    class Program
    {
        private static Repository ?m_repository;

        static void Main(string[] args)
        {
            m_repository = new Repository();
            Console.WriteLine("Consumer started");

            ConnectionFactory cf = new();
            using IConnection c = cf.CreateConnection();

            var s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string m = Encoding.UTF8.GetString(args.Message.Data);
                Console.WriteLine("Consuming from subject {1}", m, args.Message.Subject);
                try
                {
                    string msg = ProcessingValuatorMessage(m);
                    Console.WriteLine("Message successfully processed");

                    // Публикация в nats
                    var msgBytes = Encoding.UTF8.GetBytes(msg);
                    c.Publish("RankCalculated", msgBytes);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            s.Unsubscribe();

            c.Drain();
            c.Close();
        }

        // Возвращает строку, которая содержит rankKey и значение rank
        private static string ProcessingValuatorMessage(string msg)
        {
            string[] splittedMsg = msg.Split(',');

            if (splittedMsg.Length != 2)
            {
                throw new ArgumentException("Wrong message format");
            }

            string textKey = splittedMsg[0];
            string rankKey = splittedMsg[1];

            string? text = m_repository?.Get(textKey);
            if (text == null)
            {
                throw new KeyNotFoundException($"Not found by key {textKey}");
            }
            double rank = GetRank(text);

            m_repository?.Set(rankKey, rank);

            return $"{rankKey},{rank}";
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