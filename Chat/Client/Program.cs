using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

class Program
{
    public static void StartClient(string host, int port, string message)
    {
        try
        {
            if(message == "")
            {
                Console.WriteLine("message must not be empty");
                return;
            }
            if (host == "localhost") host = "127.0.0.1";
            // Разрешение сетевых имён
            IPAddress ipAddress = IPAddress.Parse(host);

            IPEndPoint remoteEP = new(ipAddress, port);

            // CREATE
            Socket sender = new(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // CONNECT
                sender.Connect(remoteEP);

                Console.WriteLine("Удалённый адрес подключения сокета: {0}",
                    sender.RemoteEndPoint.ToString());

                // Подготовка данных к отправке
                byte[] msg = Encoding.UTF8.GetBytes(message);

                // SEND
                int bytesSent = sender.Send(msg);

                // RECEIVE
                
                byte[] buf = new byte[1024];

                string data = null;
                while (true)
                {
                    int bytesRec = sender.Receive(buf);

                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (bytesRec <= 0)
                    {
                        break;
                    }
                }

                Console.WriteLine(data);

                // RELEASE
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void Main(string[] args)
    {
        StartClient(args[0], int.Parse(args[1]), args[2]);
    }
}