using Microsoft.Xna.Framework;
using protocolLibrary;
using System;
using System.Collections.Generic;

namespace protocolLibrary
{
    public class InitialInfoPacket
    {
        public readonly int height, width, cellSize, snakeCount, id;
        public readonly List<Vector2> snakes;
        // in format "heigth width cellSize players id s1.x s1.y s2.x s2.y ..."
        public readonly string serialized;
        public InitialInfoPacket(int _height, int _width, int _cellSize, int _snakeCount, int _id, List<Vector2> _snakes)
        {
            height = _height;
            width = _width;
            cellSize = _cellSize;
            snakeCount = _snakeCount;
            id = _id;
            snakes = _snakes;
            serialized = String.Format("{0} {1} {2} {3} {4} ", _height, _width, _cellSize, _snakeCount, _id);
            foreach (Vector2 v in _snakes)
                serialized += String.Format("{0} {1} ", v.X, v.Y);
        }
        public InitialInfoPacket(string _serialized)
        {
            List<int> c = Utils.DeserializeToInt(_serialized);
            serialized = _serialized;
            height = c[0];
            width = c[1];
            cellSize = c[2];
            snakeCount = c[3];
            id = c[4];
            snakes = new List<Vector2>();
            for (int i = 0; i < snakeCount; i++)
                snakes.Add(new Vector2(c[5 + 2 * i], c[6 + 2 * i]));
        }
    }
}
