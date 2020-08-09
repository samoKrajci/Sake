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
        public static Texture2D _foodTexture, _invincibilityTexture;

        public Vector2 position;
        public string type;

        public Powerup(Vector2 _position, string _type)
        {
            position = _position;
            type = _type;
        }
        public void Draw(SpriteBatch spriteBatch, int cellSize)
        {
            Texture2D texture = _foodTexture;
            if (type == "food")
                texture = _foodTexture;
            else if (type == "invincibility")
                texture = _invincibilityTexture;
            spriteBatch.Draw(texture, position * cellSize, Color.White);
        }
    }
}
