using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class Beat
    {
        public Texture2D whitetexture;
        public Rectangle beatRect;


        public Beat(ContentManager content)
        {
            whitetexture = content.Load<Texture2D>("sprites/white");
            beatRect = new Rectangle(0, 0, 150, 150);
        }


        public void Update(GameTime gameTime)
        {
        }
    }
}
