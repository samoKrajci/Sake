﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameLibrary
{
    public class SnakeUser : Snake
    {
        public string nextDirection = "f";
        public SnakeUser(int id, Vector2 initialPosition, Texture2D texture = null) : base(initialPosition, -1, texture)
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
        public static Texture2D _texture;
        public static List<Color> colors = new List<Color> { Color.Pink, Color.Red, Color.Green, Color.Blue };
        public static Color invincibleColor = Color.White;

        public int _id;
        public Vector2 position;
        public Queue<Vector2> tail;
        public int direction; // 0 = up, next clockwise
       
        public List<string> lethal = new List<string>(){ "0", "1", "2", "3"};
        public bool dead;
        public int lastGrow;
        public int invincible = 0;

        private static readonly int[] dx = { 0, 1, 0, -1 };
        private static readonly int[] dy = { -1, 0, 1, 0 };

        
        public Snake(Vector2 initialPosition, int initialInvincibility, Texture2D texture = null)
        {
            _texture = texture;
            position = initialPosition;
            tail = new Queue<Vector2>();
            direction = Rand.om.Next(4);
            dead = false;
            lastGrow = 0;
            invincible = initialInvincibility;
        }
        public Vector2 NextMove(int height, int width)
        {
            return new Vector2((position.X + width + dx[direction]) % width, (position.Y + height + dy[direction]) % height);
        }
        public List<Vector2> MoveTo(Vector2 destination, int lenDiff = 0)
        {
            List<Vector2> dequeued = new List<Vector2>();
            tail.Enqueue(position);
            int cellsToDequeue = (lenDiff - 1) * (-1);
            for (int i=0; i<cellsToDequeue; i++)
                dequeued.Add(tail.Dequeue());

            for(int i=0; i<4; i++)
            {
                Vector2 adjacent = position + new Vector2(dx[i], dy[i]);
                if (adjacent == destination)
                    direction = i;
            }

            position = destination;

            return dequeued;
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
            Color currentColor = colors[_id];
            
            var origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            if (invincible > 0)
                spriteBatch.Draw(_texture, position * cellSize + origin, null, invincibleColor, (float)Math.PI / 2 * direction, origin, 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_texture, position * cellSize + origin, null, currentColor, (float)Math.PI / 2 * direction, origin, 1f, SpriteEffects.None, 0f);
            try
            {
                foreach (Vector2 v in tail)
                {
                    Color c = colors[_id];
                    if (dead)
                        c = Color.Black;
                    else if (invincible > 0)
                        spriteBatch.Draw(_texture, v * cellSize + origin, null, invincibleColor, 0, origin, 1.4f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(_texture, v * cellSize, c);
                }
            }
            catch(System.InvalidOperationException)
            {
                ; //caused by async process, occasionally skips a frame (?)
            }
        }
    }
}
