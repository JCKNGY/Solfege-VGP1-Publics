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
    public class EnemyProjectile
    {
        public Vector2 Position;
        public bool IsAlive = true;

        public Vector2 velocity;
        public const float Speed = 200f;
        public const float Lifetime = 3.5f;
        public float timeAlive = 0f;
        public int damage;

        public static readonly Vector2 Size = new Vector2(20, 20);

        public Texture2D Sprite;

        // make the projectile
        public EnemyProjectile(Vector2 origin, Vector2 target, int dmg)
        {
            Position = origin;
            damage = dmg;

            Vector2 dir = target - origin;
            if (dir.Length() > 0f)
            {
                dir.Normalize();
            }
            velocity = dir * Speed;
        }

        // move the projectile
        public void Update(GameTime gameTime)
        {
            if (!IsAlive) 
            {
                return;
            }
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += velocity * elapsed;
            timeAlive += elapsed;
            if (timeAlive >= Lifetime) 
            {
                IsAlive = false;
            }
        }


        // check if it hit the player
        public int CheckHit(Vector2 playerPos, Vector2 playerSize)
        {
            if (!IsAlive)
            {
                return 0;
            }
            Rectangle projRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (projRect.Intersects(playerRect))
            {
                IsAlive = false;
                return damage;
            }
            return 0;
        }

        // draw the music note rotated to face direction
        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (!IsAlive)
            {
                return;
            }

            Vector2 screenPos = Position - camera.Position;
            Vector2 center = screenPos + Size / 2f;

            if (Sprite != null)
            {
                float angle = (float)Math.Atan2(velocity.Y, velocity.X);
                Vector2 origin = new Vector2(Sprite.Width / 2f, Sprite.Height / 2f);
                float scale = Size.X / (float)Sprite.Width;
                spriteBatch.Draw(Sprite, center, null, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)Size.X, (int)Size.Y), Color.Red);
            }
        }
    }
}
