using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class MusicNoteProjectile
    {
        public Vector2 Position;
        public bool IsAlive = true;

        Vector2 velocity;
        float timeAlive = 0f;
        const float Lifetime = 2.5f;
        const float Speed = 300f;

        public static readonly Vector2 Size = new Vector2(20, 20);
        public int Damage = 25;

        Texture2D sprite;

        public MusicNoteProjectile(Vector2 origin, Vector2 direction, Texture2D sprite)
        {
            Position = origin;
            this.sprite = sprite;

            Vector2 dir = direction;
            if (dir.Length() > 0f)
            {
                dir.Normalize();
            }

            velocity = dir * Speed;
        }

        public void Update(float elapsed)
        {
            if (!IsAlive)
            {
                return;
            }

            Position += velocity * elapsed;
            timeAlive += elapsed;

            if (timeAlive >= Lifetime)
            {
                IsAlive = false;
            }
        }


        public bool CheckEnemyHit(Enemy e)
        {
            if (!IsAlive || !e.IsAlive)
            {
                return false;
            }

            Rectangle noteRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle enemyRect = new Rectangle((int)e.Position.X, (int)e.Position.Y, (int)e.Size.X, (int)e.Size.Y);

            if (noteRect.Intersects(enemyRect))
            {
                Vector2 knockDir = e.Position - Position;
                e.TakeDamage(Damage, knockDir, 220f);
                IsAlive = false;
                return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsAlive)
            {
                return;
            }

            Vector2 screenPos = Position - camera.Position;
            spriteBatch.Draw(sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)Size.X, (int)Size.Y), Color.White);
        }
    }


    public class FluteWeapon
    {
        Texture2D fluteSprite;
        Texture2D noteSprite;

        public List<MusicNoteProjectile> Notes = new List<MusicNoteProjectile>();


        const int NotesPerShockwave = 12;

        const float OrbitRadius = 60f;
        const float OrbitSpeed = 2.5f;
        float orbitAngle = 0f;


        const int FluteSize = 56;

        public FluteWeapon(ContentManager content)
        {
            fluteSprite = content.Load<Texture2D>("sprites/Weapon Sprites/Flute");
            noteSprite = content.Load<Texture2D>("sprites/Projectiles/2Eights");
        }


        public void FireShockwave(Vector2 origin)
        {
            for (int i = 0; i < NotesPerShockwave; i++)
            {
                float angle = (float)(Math.PI * 2.0 / NotesPerShockwave * i);
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Notes.Add(new MusicNoteProjectile(origin, dir, noteSprite));
            }
        }

        public void Update(float elapsed, List<Enemy> enemies)
        {
            orbitAngle += OrbitSpeed * elapsed;

            for (int i = Notes.Count - 1; i >= 0; i--)
            {
                Notes[i].Update(elapsed);

                foreach (Enemy e in enemies)
                {
                    Notes[i].CheckEnemyHit(e);
                }

                if (!Notes[i].IsAlive)
                {
                    Notes.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Vector2 playerPos, Vector2 playerSize)
        {

            Vector2 playerCenter = playerPos + playerSize / 2f;
            float px = playerCenter.X + (float)Math.Cos(orbitAngle) * OrbitRadius;
            float py = playerCenter.Y + (float)Math.Sin(orbitAngle) * OrbitRadius;
            Vector2 fluteWorld = new Vector2(px - FluteSize / 2f, py - FluteSize / 2f);
            Vector2 fluteScreen = fluteWorld - camera.Position;

            Vector2 origin = new Vector2(FluteSize / 2f, FluteSize / 2f);
            Rectangle dest = new Rectangle((int)(fluteScreen.X + origin.X), (int)(fluteScreen.Y + origin.Y), FluteSize, FluteSize);

            spriteBatch.Draw(fluteSprite, dest, null, Color.White, orbitAngle, origin, SpriteEffects.None, 0f);

            foreach (MusicNoteProjectile note in Notes)
            {
                note.Draw(spriteBatch, camera);
            }
        }
    }
}
