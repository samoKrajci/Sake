using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Client;
using System.Threading.Tasks;
using protocolLibrary;
using gameLibrary;
using System;

namespace Sake
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        int HEIGHT, WIDTH, CELLSIZE;
        string state = "lobby";
        string serverAddress = Sake.Properties.Settings.Default.serverAddress;

     
        readonly Client.TcpClient tcpClient = new Client.TcpClient("localhost");
        SnakeUser snakeUser;

        private Texture2D 
            snakeHeadTexture, 
            snakeBodyTexture, 
            foodTexture, 
            invincibilityTexture, 
            stoneTexture, 
            slowTexture, 
            megaFoodTexture, 
            reverseTexture;

        private SpriteFont someFont;

        private string postgameMessage;

        private void ResponseWrapper()
        {
            string response = tcpClient.LastResponse;
            if (response == "requesting move")
                tcpClient.SendRequest(snakeUser.SendNextDireciton());
            else if (response == "game over")
            {
                state = "postgame";
                string winner = map.getWinner();
                if (winner == "none")
                    postgameMessage = "Everyone lost";
                else if (winner == "multiple")
                    postgameMessage = "server fail";
                else
                    postgameMessage = String.Format("{0} won!", winner);
                tcpClient.Disconnect();
            }
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
                map.AddSnake(new Snake(initialInfo.snakes[i], initialInfo.initialInvincibility));

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

            snakeHeadTexture = Content.Load<Texture2D>("head");
            snakeBodyTexture = Content.Load<Texture2D>("body");
            foodTexture = Content.Load<Texture2D>("food");
            invincibilityTexture = Content.Load<Texture2D>("invincibility");
            stoneTexture = Content.Load<Texture2D>("stone");
            slowTexture = Content.Load<Texture2D>("slow");
            megaFoodTexture = Content.Load<Texture2D>("megaFood");
            reverseTexture = Content.Load<Texture2D>("reverse");

            someFont = Content.Load<SpriteFont>("fonts/SomeFont");

            Snake.headTexture = snakeHeadTexture;
            Snake.bodyTexture = snakeBodyTexture;
            Powerup._foodTexture = foodTexture;
            Powerup._invincibilityTexture = invincibilityTexture;
            Powerup._stoneTexture = stoneTexture;
            Powerup._slowTexture = slowTexture;
            Powerup._megaFoodTexture = megaFoodTexture;
            Powerup._reverseTexture = reverseTexture;

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            Keyboard.GetState();

            if (state == "game")
            {
                if (Keyboard.HasBeenPressed(Keys.Right))
                    snakeUser.nextDirection = "r";
                else if (Keyboard.HasBeenPressed(Keys.Left))
                    snakeUser.nextDirection = "l";
            }
            else if (state == "postgame")
            {
                if (Keyboard.HasBeenPressed(Keys.Space))
                    this.Exit();
            }
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (state == "lobby")
                return;
            spriteBatch.Begin();

            map.Draw(spriteBatch);

            if (state == "postgame")
            {
                spriteBatch.DrawString(someFont, postgameMessage, new Vector2(WIDTH / 2 * CELLSIZE - 100, HEIGHT / 2 * CELLSIZE - 20), Color.White);
                spriteBatch.DrawString(someFont, "press [space] to exit", new Vector2(WIDTH / 2 * CELLSIZE - 100, HEIGHT * CELLSIZE - 40), Color.White);
            }
            else if (map.snakes[snakeUser._id].dead)
                spriteBatch.DrawString(someFont, "Game Over!", new Vector2(WIDTH / 2 * CELLSIZE - 100, HEIGHT / 2 * CELLSIZE - 20), Color.White);


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
