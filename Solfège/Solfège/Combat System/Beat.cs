using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfège
{
    class Beat
    {
        Texture2D whitetexture;
        Rectangle beatRect;


        // load the beat stuff
        public Beat(ContentManager content)
        {
            whitetexture = content.Load<Texture2D>("sprites/white");
            beatRect = new Rectangle(0, 0, 150, 150);
        }
        // update the beat, dont do anything yet
        public void Update(GameTime gameTime)
        {

        }
    }
}
