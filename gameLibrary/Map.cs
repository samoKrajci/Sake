using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using protocolLibrary;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.CodeDom.Compiler;

namespace gameLibrary
{
    public class MasterMap : Map
    {
        public Dictionary<Vector2, string> grid;
        public List<Powerup> addedPowerups = new List<Powerup>();
        public List<Powerup> removedPowerups = new List<Powerup>();


        public MasterMap(int height, int width, int cellSize) : base(height, width, cellSize)
        {
            grid = new Dictionary<Vector2, string>();
            for (int i=0; i<width; i++)
                for(int j=0; j<height; j++)
                    grid.Add(new Vector2(i, j), "");

        }
        public void AddPowerupWithChance(int chance, string type)
        {
            int roll = Rand.om.Next(chance);
            if (roll == 0)
            {
                Vector2 position = new Vector2(Rand.om.Next(_width), Rand.om.Next(_height));
                if (grid[position] != "") return;
                AddPowerup(new Powerup(position, type));
            }
        }
        public void AddPowerup(Powerup powerup)
        {
            powerups.Add(powerup);
            addedPowerups.Add(powerup);
            grid[powerup.position] = powerup.type;
        }
        public override void AddSnake(Snake newSnake)
        {
            newSnake._id = snakes.Count;
            base.AddSnake(newSnake);
            grid[newSnake.position] = newSnake._id.ToString();
        }
        public void AutoUpdate()
        {
            foreach (Snake s in snakes)
            {
                if (s.dead)
                    continue;

                s.lastGrow = 0;

                Vector2 nextMove = s.NextMove(_height, _width);
                string field = grid[nextMove];
                if ((s.lethal.Contains(field)) && (s.invincible <= 0))
                {
                    foreach (Snake ss in snakes)
                        if (ss.position == nextMove)
                            ss.dead = true;
                    s.dead = true;
                    continue;
                }

                List<Vector2> freedFields;

                List<string> pws = new List<string>{ "food", "invincibility" };
                if (pws.Contains(field))
                {
                    if(field == "food")
                        s.lastGrow = 1;
                    if (field == "invincibility")
                        s.invincible = 20;
                    
                    freedFields = s.MoveTo(nextMove);
                    grid[nextMove] = s._id.ToString();

                    foreach (Powerup p in powerups)
                        if (p.position == nextMove)
                        {
                            powerups.Remove(p);
                            removedPowerups.Add(p);
                            break;
                        }
                }
                else
                {
                    freedFields = s.MoveTo(nextMove);
                    grid[nextMove] = s._id.ToString();
                }

                foreach (Vector2 f in freedFields)
                {
                    grid[f]="";
                }
            }

            foreach (Snake s in snakes)
            {
                if (s.invincible > 0)
                    s.invincible--;
            }

            generatePowerups();
        }
        public void generatePowerups()
        {
            AddPowerupWithChance(20, "food");
            AddPowerupWithChance(20, "invincibility");
            AddPowerupWithChance(20, "stone");

        }
        public MapUpdatePacket CreateMapUpdatePacket()
        {
            List<SnakeInfo> snakeInfos = new List<SnakeInfo>();
            foreach(Snake s in snakes)
            {
                List<string> buffs = new List<string>();
                if (s.invincible > 0) buffs.Add("invincible");
                SnakeInfo snakeInfo = new SnakeInfo(s.position, s.lastGrow, buffs, s.dead);
                snakeInfos.Add(snakeInfo);
            }
            List<PowerupInfo> powerupsPlus = new List<PowerupInfo>();
            foreach (Powerup p in addedPowerups)
                powerupsPlus.Add(new PowerupInfo(p.position, p.type));

            addedPowerups = new List<Powerup>();
            
            List<PowerupInfo> powerupsMinus = new List<PowerupInfo>();
            foreach (Powerup p in removedPowerups)
                powerupsMinus.Add(new PowerupInfo(p.position, p.type));

            removedPowerups = new List<Powerup>();

            return new MapUpdatePacket(snakeInfos, powerupsPlus, powerupsMinus);
        }

    }
    public class Map
    {
        public readonly int _height;
        public readonly int _width;
        public readonly int _cellSize;
        public List<Snake> snakes = new List<Snake>();
        public List<Powerup> powerups = new List<Powerup>();

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
        public virtual void AddSnake(Snake newSnake)
        {
            newSnake._id = snakes.Count;
            snakes.Add(newSnake);
        }
        public virtual void AddSnakeRandomPosition(int initialInvincibility, Texture2D texture = null)
        {
            Vector2 position = new Vector2(Rand.om.Next() % _width, Rand.om.Next() % _height);
            Snake snake = new Snake(position, initialInvincibility, texture);
            AddSnake(snake);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Snake s in snakes)
                s.Draw(spriteBatch, _cellSize);
            try
            {
                foreach (Powerup p in powerups)
                    p.Draw(spriteBatch, _cellSize);
            }
            catch(InvalidOperationException e){
                // mi to hadze exception, lebo neviem paralelne programovat... realne to ale az tak nevadi, obcas proste nevykresli frame
                Debug.WriteLine(e);
            }
        }

        public void UpdateFromMapUpdatePacket(MapUpdatePacket mapUpdatePacket)
        {
            for(int i=0; i < mapUpdatePacket.snakes.Count; i++)
            {
                SnakeInfo sInfo = mapUpdatePacket.snakes[i];
                if (sInfo.dead)
                {
                    snakes[i].dead = true;
                    continue;
                }
                else
                {
                    snakes[i].lastGrow = sInfo.lenDiff;
                    snakes[i].MoveTo(sInfo.newPosition);
                }
                if (sInfo.buffs.Contains("invincible"))
                    snakes[i].invincible = 10;
                else
                    snakes[i].invincible = 0;

            }
            foreach (PowerupInfo pInfo in mapUpdatePacket.powerupsMinus)
                foreach (Powerup p in powerups)
                    if (p.position == pInfo.position)
                    {
                        powerups.Remove(p);
                        break;
                    }
            foreach (PowerupInfo pInfo in mapUpdatePacket.powerupsPlus)
                powerups.Add(new Powerup(pInfo.position, pInfo.type));
        }
    }
}
