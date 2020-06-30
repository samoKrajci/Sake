using System.Net.Sockets;

namespace Server
{
    class User
    {
        public string username;
        public Socket socket;
        private static int BUFFER_SIZE = 2048;
        public readonly byte[] buffer = new byte[BUFFER_SIZE];

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
    }
}
