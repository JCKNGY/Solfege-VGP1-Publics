using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Solfège
{
    public class Shockwave
    {
        public bool IsAlive = true;

        public Vector2 center;
        public float currentRadius;
        public float maxRadius;
        public float lifetime;
        public float timeAlive;

        // make the shockwave
        public Shockwave(Vector2 origin, float radius, float life)
        {
            center = origin;
            maxRadius = radius;
            lifetime = life;
            currentRadius = 0f;
        }

        // expand the ring out
        public void Update(float elapsed)
        {
            timeAlive += elapsed;
            currentRadius = maxRadius * (timeAlive / lifetime);
            if (timeAlive >= lifetime)
            {
                IsAlive = false;
            }
            }

        // check if player is on the ring
        public bool CheckHit(Vector2 playerPos, Vector2 playerSize)
        {
            Vector2 playerCenter = playerPos + playerSize / 2f;
            float dist = Vector2.Distance(center, playerCenter);

            return Math.Abs(dist - currentRadius) < 20f;
        }

        // draw the shockwave ring
        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (!IsAlive)
            {
                return;
            }

            float alpha = 1f - (timeAlive / lifetime);
            Color ringColor = Color.Purple * alpha;

            Vector2 screenCenter = center - camera.Position;
            int steps = 32;

            for (int i = 0; i < steps; i++)
            {
                float angle = (float)(Math.PI * 2 / steps * i);
                float px = screenCenter.X + (float)Math.Cos(angle) * currentRadius;
                float py = screenCenter.Y + (float)Math.Sin(angle) * currentRadius;

                spriteBatch.Draw(pixel, new Rectangle((int)px - 3, (int)py - 3, 6, 6), null, ringColor, angle, Vector2.Zero, SpriteEffects.None, 0f);
            }
        }
    }
}
