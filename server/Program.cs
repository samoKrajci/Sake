using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ProtocolLibrary;
using GameLibrary;
using Microsoft.Xna.Framework;

namespace Server
{
    class Program
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<User> users = new List<User>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private const int HEIGHT = 30, WIDTH = 30, CELLSIZE = 20;
        private static Map map;

        static void SendAllPeriodicallyAsync(int periode)
        {
            Task.Factory.StartNew(() => SendAllPeriodically(periode));
        }
        static void SendAllPeriodically(int periode)
        {
            while (true)
            {
                foreach (User u in users)
                {
                    byte[] data = Encoding.ASCII.GetBytes("sent data");
                    u.SendData(data);
                    Console.WriteLine("data send after " + periode + " ms");
                }
                Thread.Sleep(periode);
            }
        }

        static void SendAllLoop()
        {
            string message;
            byte[] data;
            while (true)
            {
                message = Console.ReadLine();
                data = Encoding.ASCII.GetBytes(message);
                foreach (User u in users)
                {
                    u.SendData(data);
                }
            }
        }

        static void Main()
        {
            Console.Title = "Server";
            SetupGame();
            SetupServer();
            SendAllLoop();

            //Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        private static void SetupGame()
        {
            map = new Map(HEIGHT, WIDTH, CELLSIZE);
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (User u in users)
            {
                u.socket.Shutdown(SocketShutdown.Both);
                u.socket.Close();
            }

            serverSocket.Close();
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

            User user = new User("user " + users.Count.ToString(), socket);
            users.Add(user);
            map.AddSnake(new Snake(new Vector2(Rand.om.Next() % WIDTH, Rand.om.Next() % HEIGHT)));
            Console.WriteLine("Client connected, sending initial info...\n");

            List<Vector2> snakesPositions = new List<Vector2>();
            foreach (Snake s in map.snakes)
                snakesPositions.Add(s.position);
            InitialInfo initialInfo = new InitialInfo(HEIGHT, WIDTH, CELLSIZE, users.Count, users.Count - 1, snakesPositions);
            byte[] data = Encoding.ASCII.GetBytes(initialInfo.serialized);
            user.socket.Send(data);

            Console.WriteLine("Initial info sent.\n");

            user.socket.BeginReceive(user.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

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

            Console.WriteLine("sleep (3s)");
            Thread.Sleep(3000);
            Console.WriteLine("woken up");


            if (text.ToLower() == "get time") // Client requested time
            {
                Console.WriteLine("Text is a get time request");
                byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                current.SendData(data);
                Console.WriteLine("Time sent to client");
            }
            else if (Regex.Match(text.ToLower(), "^change username").Success)
            {
                Console.WriteLine("user wants to change username");
                string[] splitInput = text.ToLower().Split(' ');
                byte[] data;
                string message;
                if (splitInput.Length < 3)
                {
                    message = "no username provided in input";
                }
                else
                {
                    string username = splitInput[2];
                    message = "username changed";
                    current.username = username;
                }
                data = Encoding.ASCII.GetBytes(message);
                Console.WriteLine(message);
                current.SendData(data);
            }
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.socket.Shutdown(SocketShutdown.Both);
                current.socket.Close();
                users.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }
            else if (text.ToLower() == "no response")
            {
                Console.WriteLine("no response sent");
            }
            else
            {
                Console.WriteLine("Text is an invalid request");
                byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                current.SendData(data);
                Console.WriteLine("Warning Sent");
            }

            Console.WriteLine();

            current.socket.BeginReceive(current.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current.socket);
        }
    }
}