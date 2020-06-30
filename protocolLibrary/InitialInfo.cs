using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ProtocolLibrary
{
    public class InitialInfo
    {
        public readonly int height, width, cellSize, players, id;
        public readonly List<Vector2> snakesPositions;
        // in format "heigth width cellSize players id s1.x s1.y s2.x s2.y ..."
        public readonly string serialized;
        public InitialInfo(int _height, int _width, int _cellSize, int _players, int _id, List<Vector2> _snakesPositions)
        {
            height = _height;
            width = _width;
            cellSize = _cellSize;
            players = _players;
            id = _id;
            snakesPositions = _snakesPositions;
            serialized = String.Format("{0} {1} {2} {3} {4} ", _height, _width, _cellSize, _players, _id);
            foreach (Vector2 v in _snakesPositions)
                serialized += String.Format("{0} {1} ", v.X, v.Y);
        }
        public InitialInfo(string _serialized)
        {
            string[] cells = _serialized.Split(' ');
            List<int> cellsInt = new List<int>();
            foreach (string s in cells)
            {
                if (s == "")
                    continue;
                cellsInt.Add(Convert.ToInt32(s));
            }
            serialized = _serialized;
            height = cellsInt[0];
            width = cellsInt[1];
            cellSize = cellsInt[2];
            players = cellsInt[3];
            id = cellsInt[4];
            snakesPositions = new List<Vector2>();
            for (int i = 0; i < players; i++)
                snakesPositions.Add(new Vector2(cellsInt[5 + 2 * i], cellsInt[6 + 2 * i]));
        }
    }
}
