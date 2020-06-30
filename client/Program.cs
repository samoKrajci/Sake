using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    public class TcpClient
    {
        readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly int PORT = 100;
        public string LastResponse = "none response yet";

        public void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException)
                {
                    ;// Console.Clear();
                }
            }

            Console.WriteLine("TCP Connected");

        }

        public void RequestLoop()
        {
            Console.WriteLine(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                Console.Write("Send a request: ");
                string request = Console.ReadLine();
                SendRequest(request);
                ReceiveResponseAsync();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        public void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        public void SendRequest(string request)
        {
            SendString(request);

            if (request.ToLower() == "exit")
                Exit();
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        public void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
        public Task RunTaskAfterResponseLoopAsync(Action action)
        {
            return Task.Run(() => RunTaskAfterResponseLoop(() => action()));
        }
        public void RunTaskAfterResponseLoop(Action action)
        {
            while (true)
                RunTaskAfterResponse(action);
        }
        public Task RunTaskAfterResponseAsync(Action action)
        {
            return Task.Run(() => RunTaskAfterResponse(action));
        }
        private void RunTaskAfterResponse(Action action)
        {
            ReceiveResponse();
            action();
        }
        public Task ReceiveResponseAsync()
        {
            return Task.Run(() => ReceiveResponse());
        }
        private bool ReceiveResponse()
        {
            var buffer = new byte[2048];
            int receivedTcp = ClientSocket.Receive(buffer, SocketFlags.None);
            if (receivedTcp == 0)
            {
                return false;
            }
            var data = new byte[receivedTcp];
            Array.Copy(buffer, data, receivedTcp);
            LastResponse = Encoding.ASCII.GetString(data);
            return true;
        }
    }
    class Program
    {
        private static readonly TcpClient tcpClient = new TcpClient();

        static void Main()
        {
            tcpClient.ConnectToServer();
            tcpClient.RunTaskAfterResponseLoop(() => Console.WriteLine(tcpClient.LastResponse));

            tcpClient.RequestLoop();
            tcpClient.Exit();
        }
    }
}