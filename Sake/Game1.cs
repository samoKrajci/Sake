using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using Client;
using System.Threading.Tasks;
using protocolLibrary;
using gameLibrary;

namespace Sake
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        int HEIGHT, WIDTH, CELLSIZE;
        string state = "lobby";

     
        readonly Client.TcpClient tcpClient = new Client.TcpClient();
        SnakeUser snakeUser;

        private Texture2D snakeTexture, powerupTexture;

        private void ResponseWrapper()
        {
            string response = tcpClient.LastResponse;
            if (response == "requesting move")
                tcpClient.SendRequest(snakeUser.SendNextDireciton());
            else if (response == "game over")
                this.Exit();
            else
            {
                MapUpdatePacket mapUpdatePacket = new MapUpdatePacket(tcpClient.LastResponse);
                map.UpdateFromMapUpdatePacket(mapUpdatePacket);
            }
        }
      
        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected async override void Initialize()
        {
            tcpClient.ConnectToServer();
            await tcpClient.ReceiveResponseAsync();
            state = "game";
            InitialInfoPacket initialInfo = new InitialInfoPacket(tcpClient.LastResponse);
            snakeUser = new SnakeUser(initialInfo.id, initialInfo.snakes[initialInfo.id]);

            HEIGHT = initialInfo.height;
            WIDTH = initialInfo.width;
            CELLSIZE = initialInfo.cellSize;
            map = new Map(HEIGHT, WIDTH, CELLSIZE);

            for (int i = 0; i < initialInfo.snakeCount; i++)
                map.AddSnake(new Snake(initialInfo.snakes[i]));
      
            graphics.PreferredBackBufferHeight = map._height * map._cellSize;
            graphics.PreferredBackBufferWidth = map._width * map._cellSize;
            graphics.ApplyChanges();

            _ = tcpClient.RunTaskAfterResponseLoopAsync(() => ResponseWrapper());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            snakeTexture = Content.Load<Texture2D>("star");
            powerupTexture = Content.Load<Texture2D>("cat");

            Snake._texture = snakeTexture;
            Powerup._texture = powerupTexture;
            //for (int i = 0; i < map.snakes.Count; i++)
            //    map.snakes[i]._texture = snakeTexture;

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            Keyboard.GetState();

            if (Keyboard.HasBeenPressed(Keys.Right))
                snakeUser.nextDirection = "r";
            else if (Keyboard.HasBeenPressed(Keys.Left))
                snakeUser.nextDirection = "l";

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (state == "lobby")
                return;

            spriteBatch.Begin();

            map.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
