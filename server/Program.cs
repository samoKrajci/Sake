using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using protocolLibrary;
using gameLibrary;
using Microsoft.Xna.Framework;
using System.Diagnostics.Tracing;
using Microsoft.Xna.Framework.Input;

namespace Server
{
    class Program
    {
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private const int HEIGHT = 30, WIDTH = 50, CELLSIZE = 16;
        private const int TPS = 10;
        private const int maxPlayers = 4;
        private const int initialInvincibility = 3*TPS;

        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<User> users = new List<User>();
        private static readonly int tickMs = 1000 / TPS;
        private static readonly int beforeStartMs = 3000;
        private static MasterMap map;
        private static string state = "lobby";

        static void SendAll(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message); ;
            foreach (User u in users)
            {
                u.SendData(data);
            }
        }
        static void PrepareGame()
        {
            map = new MasterMap(HEIGHT, WIDTH, CELLSIZE);
            for (int i=0; i<users.Count; i++)
            {
                map.AddSnakeRandomPosition(initialInvincibility);
            }

            List<Vector2> snakesPositions = new List<Vector2>();
            foreach (Snake s in map.snakes)
                snakesPositions.Add(s.position);

            Console.WriteLine(String.Format("Map created.\nDimensions: {0}x{1}\nCell size: {2}\nPlayers: {3}", WIDTH, HEIGHT, CELLSIZE, map.snakes.Count));
            Console.WriteLine();

            for(int i=0; i<users.Count; i++)
            {
                User user = users[i];
                InitialInfoPacket initialInfo = new InitialInfoPacket(HEIGHT, WIDTH, CELLSIZE, users.Count, i, snakesPositions, initialInvincibility);
                byte[] data = Encoding.ASCII.GetBytes(initialInfo.serialized);
                try
                {
                    user.SendData(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(String.Format("Initial info sent to {0}", user.username));
            }

            Console.WriteLine();


        }
        static async Task RunGameLoop(CancellationToken cancellationToken)
        {
            PrepareGame();
            Thread.Sleep(beforeStartMs);
            Console.WriteLine("Game starting...");
            Console.WriteLine();

            int aliveCount = map.snakes.Count;
            int winner = -1;
            while (aliveCount > 1) {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                
                Thread.Sleep(tickMs);
                List<Task<string>> tasks = new List<Task<string>>();
                Console.WriteLine("requesting move from all users");
                Console.WriteLine(users.Count);
                foreach (User u in users)
                {
                    u.SendData(Encoding.ASCII.GetBytes("requesting move"));
                    tasks.Add(Task.Run(() => u.ReceiveResponse()));
                }

                var results = await Task.WhenAll(tasks);

                if (cancellationToken.IsCancellationRequested)
                {
                    SendAll("game over");
                    CloseAllUserSockets();
                }
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var item in results)
                {
                    Snake snake = map.snakes[Convert.ToInt32(item.Split(' ')[0])];
                    string direction = item.Split(' ')[1];
                    snake.nextDir = direction;
                }

                map.AutoUpdate();
                MapUpdatePacket mapUpdatePacket = map.CreateMapUpdatePacket();
                Console.WriteLine("sending updated map to all users");
                SendAll(mapUpdatePacket.serialized);

                Console.WriteLine(mapUpdatePacket.serialized);

                aliveCount = 0;
                foreach (Snake s in map.snakes)
                    if (!s.dead)
                        aliveCount++;

                watch.Stop();
                Console.WriteLine(String.Format("tick duration: {0}", watch.Elapsed));
                Console.WriteLine();
            }

            string message = "Game over!\n";

            foreach (Snake s in map.snakes)
                if (!s.dead)
                    winner = s._id;

            if (winner == -1)
                message += "No winner :(\n";
            else
                message += "Snake number " + winner.ToString() + " is the winner!";

            message += "\nWaiting for players...";
            SendAll("game over");
            CloseAllUserSockets();



            state = "lobby";
            Console.WriteLine(message);
        }
        static void GameMasterLoop()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            while(state != "quit")
            {
                string message = "";
                string command = Console.ReadLine();

                if(command == "quit")
                {
                    SendAll("game over");
                    cancellationTokenSource.Cancel();
                    message = "quitting\npress enter to exit";
                    state = command;
                }
                else if(command == "game")
                {
                    if (state != "game")
                    {
                        message = "starting game";
                        cancellationTokenSource = new CancellationTokenSource();
                        try
                        {
                            Task.Run(() => RunGameLoop(cancellationTokenSource.Token));
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("game over");
                        }
                    }
                    state = command;
                }
                else if(command == "lobby")
                {
                    cancellationTokenSource.Cancel();
                    message = "waiting for players...";
                    state = command;
                }

                if (message != "")
                    Console.WriteLine(message);
            }
        }
        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            GameMasterLoop();

            Console.ReadLine(); // When we press enter close everything
            CloseAllUserSockets();
            serverSocket.Close();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }
        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllUserSockets()
        {
            foreach (User u in users)
            {
                try
                {
                    u.socket.Shutdown(SocketShutdown.Both);
                }
                catch (ObjectDisposedException) {; }
                u.socket.Close();
            }
            users = new List<User>();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            if((users.Count >= maxPlayers) || (state != "lobby"))
            {
                byte[] message = Encoding.ASCII.GetBytes("Lobby full or no open lobby. Try again in a moment...");
                socket.Send(message);
                serverSocket.BeginAccept(AcceptCallback, null);

                Console.WriteLine("Someone tried to connect unsuccessfully");
                return;
            }

            User user = new User("user " + users.Count.ToString(), socket);
            users.Add(user);
            Console.WriteLine(String.Format("{0} connected.", user.username));
            Console.WriteLine(socket.RemoteEndPoint);
            Console.WriteLine(socket.LocalEndPoint);
            //user.socket.BeginReceive(user.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        // toto sa vlastne moze vymazat asi
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket currentSocket = (Socket)AR.AsyncState;
            User noUser = new User("noUser", currentSocket);
            User current = noUser;

            foreach (User u in users)
            {
                if (currentSocket == u.socket)
                {
                    current = u;
                    break;
                }
            }

            int received;

            try
            {
                received = current.socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.socket.Close();
                users.Remove(current);
                return;
            }

            Console.Write(received);
            byte[] recBuf = new byte[received];
            Array.Copy(current.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine(current.username + ":");
            Console.WriteLine(current.socket.LocalEndPoint.ToString());

            Console.WriteLine("received Text: " + text);

            if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.socket.Shutdown(SocketShutdown.Both);
                current.socket.Close();
                users.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                string[] msg = text.ToLower().Split(' ');
                Snake s = map.snakes[Convert.ToInt32(msg[0])];
                if (msg[1] == "r")
                    s.TurnRight();
                else if (msg[1] == "l")
                    s.TurnLeft();
                current.moveReceived = true;
                
                Console.WriteLine(String.Format("From {0}(snake {1}) received: {2}", current.username, msg[0], msg[1] ));
            }

            current.socket.BeginReceive(current.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current.socket);
        }
    }
}