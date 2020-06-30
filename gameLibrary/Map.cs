using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public class Map
    {
        public readonly int _height;
        public readonly int _width;
        public readonly int _cellSize;
        public List<Snake> snakes = new List<Snake>();

        public Map(int height, int width, int cellSize)
        {
            this._height = height;
            this._width = width;
            this._cellSize = cellSize;
        }
        public void ChangeDirectionAllSnakes(string dir)
        {
            foreach (Snake s in snakes)
            {
                if (dir == "r")
                    s.TurnRight();
                else if (dir == "l")
                    s.TurnLeft();
            }
        }
        public void AddSnake(Snake newSnake)
        {
            this.snakes.Add(newSnake);
        }
        public void Update()
        {
            foreach (Snake s in snakes)
                s.Move(_height, _width);

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Snake s in snakes)
                s.Draw(spriteBatch, _cellSize);
        }

    }
}
