using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;

namespace Server
{
    class User
    {
        public string username;
        public Socket socket;
        private static int BUFFER_SIZE = 2048;
        public readonly byte[] buffer = new byte[BUFFER_SIZE];
        public bool moveReceived = false;

        public User(string n, Socket s)
        {
            this.username = n;
            this.socket = s;
        }

        public bool SendData(byte[] data)
        {
            this.socket.Send(data);
            return true;
        }
        public async Task<string> ReceiveResponse()
        {
            var buffer = new byte[2048];
            int receivedTcp = await Task.Run(() => socket.Receive(buffer, SocketFlags.None));
            var data = new byte[receivedTcp];
            Array.Copy(buffer, data, receivedTcp);
            string message = Encoding.ASCII.GetString(data);
            Console.WriteLine(String.Format("from {0} received: {1}", username, message));
            return message;
        }
    }
}
