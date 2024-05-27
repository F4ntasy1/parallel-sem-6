using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            throw new Exception("Too few arguments");
        }

        int listeningPort = int.Parse(args[0]);
        string nextHost = args[1];
        int nextPort = int.Parse(args[2]);

        if (args.Length == 4 && bool.Parse(args[3]))
        {
            // процесс инициатор
            Start(listeningPort, nextHost, nextPort, true);
        }
        else
        {
            Start(listeningPort, nextHost, nextPort);
        }
    }

    private static void Start(int listeningPort, string nextHost, int nextPort, bool isInit = false)
    {
        IPAddress nextIpAddress = IPAddress.Parse(nextHost);
        IPAddress currIpAddress = IPAddress.Parse("127.0.0.1");

        IPEndPoint senderEP = new(nextIpAddress, nextPort);
        IPEndPoint listenerEP = new(currIpAddress, listeningPort);

        Socket sender = new(
            nextIpAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        Socket listener = new(
            currIpAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        listener.Bind(listenerEP);
        listener.Listen(10);

        try
        {
            string value = Console.ReadLine() ?? throw new Exception("Read failed");
            int x = int.Parse(value);

            Console.WriteLine("Readed value : " + x);

            if (!isInit)
            {
                int y = GetValueFromSocket(listener);
                x = x > y ? x : y;
            }

            int bytesSent = SendMessage(x.ToString(), sender, senderEP);
            x = GetValueFromSocket(listener);
            bytesSent = SendMessage(x.ToString(), sender, senderEP);

            Console.WriteLine("Result value : " + x);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static int GetValueFromSocket(Socket socket)
    {
        Socket handler = socket.Accept();

        byte[] buf = new byte[1024];
        int bytesCount = handler.Receive(buf);

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        return int.Parse(Encoding.UTF8.GetString(buf, 0, bytesCount));
    }

    /* Возвращает количество отправленных байт */
    private static int SendMessage(string message, Socket socket, IPEndPoint endpoint)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        if (!socket.Connected)
        {
            socket.Connect(endpoint);
        }
        return socket.Send(msg);
    }
}