using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameLibrary
{
    public class Powerup
    {
        public static Texture2D _texture;

        public Vector2 position;
        public string type;

        public Powerup(Vector2 _position, string _type)
        {
            position = _position;
            type = _type;
        }
        public void Draw(SpriteBatch spriteBatch, int cellSize)
        {
            spriteBatch.Draw(_texture, position * cellSize, Color.White);
        }
    }
}
