using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using Client;
using System.Threading.Tasks;
using ProtocolLibrary;
using GameLibrary;

namespace Sake
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        int HEIGHT, WIDTH, CELLSIZE;

        readonly Client.TcpClient tcpClient = new Client.TcpClient();
        SnakeUser snakeUser;

        private static Texture2D[] textures = new Texture2D[3];
        private static Texture2D cat;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected async override void Initialize()
        {
            tcpClient.ConnectToServer();
            await tcpClient.ReceiveResponseAsync();
            InitialInfo initialInfo = new InitialInfo(tcpClient.LastResponse);
            snakeUser = new SnakeUser(initialInfo.id, initialInfo.snakesPositions[initialInfo.id], textures[initialInfo.id]);

            HEIGHT = initialInfo.height;
            WIDTH = initialInfo.width;
            CELLSIZE = initialInfo.cellSize;
            map = new Map(HEIGHT, WIDTH, CELLSIZE);

            for (int i = 0; i < initialInfo.players; i++)
            {
                map.AddSnake(new Snake(initialInfo.snakesPositions[i]));
                Debug.WriteLine(i);
                Debug.WriteLine(map.snakes[i]._texture);
            }

            graphics.PreferredBackBufferHeight = map._height * map._cellSize;
            graphics.PreferredBackBufferWidth = map._width * map._cellSize;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            textures[0] = Content.Load<Texture2D>("cat");
            textures[1] = Content.Load<Texture2D>("star");
            textures[2] = Content.Load<Texture2D>("lcd");

            for (int i = 0; i < map.snakes.Count; i++)
                map.snakes[i]._texture = textures[i];

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            map.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
