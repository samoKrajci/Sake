using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace protocolLibrary
{
    public class MapUpdatePacket
    {
        public int snakeCount;
        public List<Vector2> snakes;
        public string serialized;

        public MapUpdatePacket(int _snakeCount, List<Vector2> _snakes)
        {
            snakeCount = _snakeCount;
            snakes = _snakes;
            serialized = String.Format("{0} ", snakeCount);
            foreach (Vector2 v in snakes)
                serialized += String.Format("{0} {1} ", v.X, v.Y);
        }
        public MapUpdatePacket(string _serialized)
        {
            List<int> c = Utils.DeserializeToInt(_serialized);

            serialized = _serialized;
            snakeCount = c[0];
            snakes = new List<Vector2>();
            for (int i = 0; i < snakeCount; i++)
                snakes.Add(new Vector2(c[1 + 2 * i], c[2 + 2 * i]));
        }

    }
}
