using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public class SnakeUser : Snake
    {
        public readonly int _id;
        public string nextDirection = "f";
        public SnakeUser(int id, Vector2 initialPosition, Texture2D texture = null) : base(initialPosition, texture)
        {
            _id = id;
        }
        public string SendNextDireciton()
        {
            string ret = String.Format("{0} {1}", _id, nextDirection);
            nextDirection = "f";
            return ret;
        }
    }
    public class Snake
    {
        public Texture2D _texture;
        public Vector2 position;
        public Queue<Vector2> tail;
        public int direction; // 0 = up, next clockwise

        private static readonly int[] dx = { 0, 1, 0, -1 };
        private static readonly int[] dy = { -1, 0, 1, 0 };


        public Snake(Vector2 initialPosition, Texture2D texture = null)
        {
            _texture = texture;
            position = initialPosition;
            tail = new Queue<Vector2>();
            tail.Enqueue(new Vector2(1, 2));
            tail.Enqueue(new Vector2(1, 3));
            tail.Enqueue(new Vector2(1, 4));
            tail.Enqueue(new Vector2(1, 5));
            direction = Rand.om.Next(4);
        }
        public void Move(int height, int width)
        {
            tail.Enqueue(new Vector2(position.X, position.Y));
            tail.Dequeue();
            
            position.X = (position.X + width + dx[direction]) % width;
            position.Y = (position.Y + height + dy[direction]) % height;
        }
        public void MoveTo(Vector2 destination)
        {
            tail.Enqueue(position);
            tail.Dequeue();

            position = destination;
        }
        public void TurnRight()
        {
            direction = (direction + 1) % 4;
        }
        public void TurnLeft()
        {
            direction = (direction + 3) % 4;
        }
        public int Length()
        {
            return tail.Count+1;
        }


        public void Draw(SpriteBatch spriteBatch, int cellSize)
        {
            spriteBatch.Draw(_texture, position * cellSize, Color.White);
            foreach (Vector2 v in tail)
                spriteBatch.Draw(_texture, v * cellSize, Color.White);
        }
    }
}
