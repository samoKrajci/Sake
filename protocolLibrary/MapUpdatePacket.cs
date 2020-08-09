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
        public List<SnakeInfo> snakes;
        public List<PowerupInfo> powerupsPlus;
        public List<PowerupInfo> powerupsMinus;
        public string serialized;

        public MapUpdatePacket(List<SnakeInfo> _snakes, List<PowerupInfo> _powerupsPlus, List<PowerupInfo> _powerupsMinus)
        {
            snakes = _snakes;
            powerupsPlus = _powerupsPlus;
            powerupsMinus = _powerupsMinus;

            serialized = "";
            serialized += String.Format("{0} ", snakes.Count);
            foreach (SnakeInfo s in snakes)
                serialized += s.serialized;
            serialized += String.Format("{0} ", powerupsPlus.Count);
            foreach (PowerupInfo p in powerupsPlus)
                serialized += p.serialized;
            serialized += String.Format("{0} ", powerupsMinus.Count);
            foreach (PowerupInfo p in powerupsMinus)
                serialized += p.serialized;
        }
        public MapUpdatePacket(string _serialized)
        {
            serialized = _serialized;

            string[] cells = _serialized.Split(' ');
            int index = 0;

            snakes = new List<SnakeInfo>();
            int snakeCount = Convert.ToInt32(cells[index]);
            index += 1;
            for (int i = 0; i < snakeCount; i++)
            {
                if (cells[index] == "dead")
                {
                    snakes.Add(new SnakeInfo(true));
                    index += 1;
                    continue;
                }
                int buffCount = Convert.ToInt32(cells[index + 3]);
                string snakeSerialized = "";
                for (int j = 0; j < buffCount + 4; j++)
                    snakeSerialized += String.Format("{0} ", cells[index + j]);

                snakes.Add(new SnakeInfo(snakeSerialized));
                index += buffCount + 4;
            }

            powerupsPlus = new List<PowerupInfo>();
            int powerupsPlusCount = Convert.ToInt32(cells[index]);
            index += 1;
            for (int i = 0; i < powerupsPlusCount; i++)
            {
                string powerupSerialized = String.Format("{0} {1} {2} ", cells[index], cells[index + 1], cells[index + 2]);

                powerupsPlus.Add(new PowerupInfo(powerupSerialized));
                index += 3;
            }

            powerupsMinus = new List<PowerupInfo>();
            int powerupsMinusCount = Convert.ToInt32(cells[index]);
            index += 1;
            for (int i = 0; i < powerupsMinusCount; i++)
            {
                string powerupSerialized = String.Format("{0} {1} {2} ", cells[index], cells[index + 1], cells[index + 2]);

                powerupsMinus.Add(new PowerupInfo(powerupSerialized));
                index += 3;
            }
        }
    }
    public class SnakeInfo
    {
        public Vector2 newPosition;
        public int lenDiff;
        public List<string> buffs;
        public bool dead;
        public string serialized;

        public SnakeInfo(Vector2 _newPosition, int _lenDiff, List<string> _buffs, bool _dead)
        {
            newPosition = _newPosition;
            lenDiff = _lenDiff;
            buffs = _buffs;
            dead = _dead;
            if (dead)
            {
                serialized = "dead ";
                return;
            }
            serialized = String.Format("{0} {1} {2} {3} ", newPosition.X, newPosition.Y, lenDiff, buffs.Count);
            foreach (string buff in buffs)
                serialized += String.Format("{0} ", buff);
        }
        public SnakeInfo(bool _dead)
        {
            dead = _dead;
        }
        public SnakeInfo(string _serialized)
        {
            serialized = _serialized;

            string[] cells = _serialized.Split(' ');

            if(cells[0] == "dead")
            {
                dead = true;
                return;
            }

            int buffCount = Convert.ToInt32(cells[3]);

            newPosition = new Vector2(Convert.ToInt32(cells[0]), Convert.ToInt32(cells[1]));
            lenDiff = Convert.ToInt32(cells[2]);
            buffs = new List<string>();
            for (int i = 0; i < buffCount; i++)
                buffs.Add(cells[4 + i]);
            dead = false;
        }
    }
    public class PowerupInfo
    {
        public Vector2 position;
        public string type;
        public string serialized;

        public PowerupInfo(Vector2 _position, string _type)
        {
            position = _position;
            type = _type;
            serialized = String.Format("{0} {1} {2} ", position.X, position.Y, type);
        }
        public PowerupInfo(string _serialized)
        {
            string[] cells = _serialized.Split(' ');

            position = new Vector2(Convert.ToInt32(cells[0]), Convert.ToInt32(cells[1]));
            type = cells[2];
            serialized = _serialized;
        }
    }
}
