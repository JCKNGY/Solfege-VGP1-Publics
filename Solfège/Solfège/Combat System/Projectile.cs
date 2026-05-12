using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class Projectile
    {
        Texture2D sprite;
        public Vector2 Position;
        Vector2 velocity;
        float timeAlive = 0f;
        const float Lifetime = 2.5f;
        const float Speed = 300f;
        public bool IsActive = true;
        public int Damage = 25;

        public static readonly Vector2 Size = new Vector2(20, 20);

        public Projectile(Texture2D sprite, Vector2 origin, Vector2 direction)
        {
            this.sprite = sprite;
            Position = origin;

            Vector2 dir = direction;
            if (dir.Length() > 0f)
            {
                dir.Normalize();
            }

            velocity = dir * Speed;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += velocity * elapsed;
            timeAlive += elapsed;

            if (timeAlive >= Lifetime)
            {
                IsActive = false;
            }
        }

        public bool CheckEnemyHit(Enemy e)
        {
            if (!IsActive || !e.IsAlive)
            {
                return false;
            }

            Rectangle noteRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle enemyRect = new Rectangle((int)e.Position.X, (int)e.Position.Y, (int)e.Size.X, (int)e.Size.Y);

            if (noteRect.Intersects(enemyRect))
            {
                Vector2 knockDir = e.Position - Position;
                e.TakeDamage(Damage, knockDir, 220f);
                IsActive = false;
                return true;
            }

            return false;
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
            if (!IsActive)
            {
                return;
            }

            Vector2 screenPos = Position - camera.Position;
            sb.Draw(sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)Size.X, (int)Size.Y), Color.White);
        }
    }
}