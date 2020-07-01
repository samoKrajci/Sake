using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary;
using protocolLibrary;
using Microsoft.Xna.Framework;

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
            snakes.Add(newSnake);
        }
        public void AddSnakeRandomPosition(Texture2D texture = null)
        {
            Vector2 position = new Vector2(Rand.om.Next() % _width, Rand.om.Next() % _height);
            Snake snake = new Snake(position);
            snakes.Add(snake);
        }
        public void AutoUpdate()
        {
            foreach (Snake s in snakes)
                s.Move(_height, _width);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Snake s in snakes)
                s.Draw(spriteBatch, _cellSize);
        }

        public MapUpdatePacket CreateMapUpdatePacket()
        {
            List<Vector2> snakesPositions = new List<Vector2>();
            foreach (Snake s in snakes)
                snakesPositions.Add(s.position);
            return new MapUpdatePacket(snakes.Count, snakesPositions);
        }

        public void UpdateFromMapUpdatePacket(MapUpdatePacket mapUpdatePacket, Texture2D[] textures)
        {
            for(int i=0; i < mapUpdatePacket.snakeCount; i++)
            {
                if (i < snakes.Count)
                    snakes[i].MoveTo(mapUpdatePacket.snakes[i]);
                else
                    snakes.Add(new Snake(mapUpdatePacket.snakes[i], textures[i]));
            }
        }
    }
}
