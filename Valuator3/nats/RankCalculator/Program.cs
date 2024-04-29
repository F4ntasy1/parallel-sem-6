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
                    ProcessingValuatorMessage(m);
                    Console.WriteLine("Message successfully processed");
                } catch (ArgumentException ex)
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

        private static void ProcessingValuatorMessage(string msg)
        {
            string[] splittedMsg = msg.Split(',');

            if (splittedMsg.Length != 2)
            {
                throw new ArgumentException("Wrong message format");
            }

            Console.WriteLine(1);
            string? text = m_repository?.Get(splittedMsg[0]);
            Console.WriteLine(2);
            if (text == null)
            {
                throw new KeyNotFoundException($"Not found by key {splittedMsg[0]}");
            }

            m_repository?.Set(splittedMsg[1], GetRank(text));
            Console.WriteLine(3);
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