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

        IPEndPoint nextEP = new(nextIpAddress, nextPort);
        IPEndPoint currEP = new(currIpAddress, listeningPort);

        Socket nextSocket = new(
            nextIpAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        Socket currSocket = new(
            currIpAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        currSocket.Bind(currEP);
        currSocket.Listen(10);

        try
        {
            string value = Console.ReadLine() ?? throw new Exception("Read failed");
            int x = int.Parse(value);

            Console.WriteLine("Readed value : " + x);

            if (isInit)
            {
                ExecuteForInitProcess(x, nextSocket, nextEP, currSocket);
            }
            else
            {
                ExecuteForBaseProcess(x, nextSocket, nextEP, currSocket);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static void ExecuteForInitProcess(
        int x, Socket nextSocket, IPEndPoint nextEP, Socket currSocket)
    {
        int bytesSent = SendMessage(x.ToString(), nextSocket, nextEP);

        Socket currSocketWithNewConn = currSocket.Accept();

        x = GetValueFromSocket(currSocketWithNewConn);
        Console.WriteLine("Result value : " + x);

        bytesSent = SendMessage(x.ToString(), nextSocket, nextEP);

        nextSocket.Shutdown(SocketShutdown.Both);
        nextSocket.Close();
        currSocketWithNewConn.Shutdown(SocketShutdown.Both);
        currSocketWithNewConn.Close();
    }

    private static void ExecuteForBaseProcess(
        int x, Socket nextSocket, IPEndPoint nextEP, Socket currSocket)
    {
        Socket currSocketWithNewConn = currSocket.Accept();

        int y = GetValueFromSocket(currSocketWithNewConn);
        int max = x > y ? x : y;

        Console.WriteLine("Max value : " + max);

        int bytesSent = SendMessage(max.ToString(), nextSocket, nextEP);

        x = GetValueFromSocket(currSocketWithNewConn);
        Console.WriteLine("Result value : " + x);

        bytesSent = SendMessage(x.ToString(), nextSocket, nextEP);

        nextSocket.Shutdown(SocketShutdown.Both);
        nextSocket.Close();
        currSocketWithNewConn.Shutdown(SocketShutdown.Both);
        currSocketWithNewConn.Close();
    }

    private static int GetValueFromSocket(Socket socket)
    {
        byte[] buf = new byte[1024];
        int bytesCount = socket.Receive(buf);
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