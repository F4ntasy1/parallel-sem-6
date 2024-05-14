using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class Program
{
    public static void StartListening(int port)
    {
        // Разрешение сетевых имён

        // Привязываем сокет ко всем интерфейсам на текущей машинe
        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new(ipAddress, port);

        // CREATE
        Socket listener = new(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        List<string> messages = [];

        try
        {
            // BIND
            listener.Bind(localEndPoint);

            // LISTEN
            listener.Listen(10);

            while (true)
            {
                Console.WriteLine("Ожидание соединения клиента...");
                // ACCEPT
                Socket handler = listener.Accept();

                Console.WriteLine("Получение данных...");
                byte[] buf = new byte[1024];
                string data = null;
                int remainingBytes = 1024;
                while (remainingBytes>0)
                {
                    // RECEIVE
                    int bytesRec = handler.Receive(buf);
                    remainingBytes = handler.Available;
                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                }
                messages.Add(data);
                Console.WriteLine("Полученный текст: {0}", data);

                List<byte> byteList = [];
                foreach (var mes in messages)
                {
                    var bytes = Encoding.UTF8.GetBytes(mes + "\n");
                    byteList.AddRange(bytes);
                }

                // SEND
                handler.Send(byteList.ToArray());

                // RELEASE
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Запуск сервера...");
            StartListening(int.Parse(args[0]));
        }
        catch(Exception ex)
        {
            Console.Write(ex.ToString());   
        }

        Console.WriteLine("\nНажмите ENTER чтобы выйти...");
        Console.Read();
    }
}