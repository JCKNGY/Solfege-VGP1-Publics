using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class InstrumentSpell
    {
        Texture2D fluteSprite;
        Texture2D noteSprite;

        public List<Projectile> activeNotes = new List<Projectile>();

        public int perfectHitCount = 0;


        float orbitAngle = 0f;
        const float OrbitRadius = 150f;
        const float OrbitSpeed = 2.8f;
        const int FluteSize = 56;

        public InstrumentSpell(ContentManager content)
        {
            fluteSprite = content.Load<Texture2D>("sprites/Weapon Sprites/Flute");
            noteSprite = content.Load<Texture2D>("sprites/Projectiles/2Eights");
        }


        public void ProcessHit(BeatRating rating, Vector2 pos, WaveManager wave)
        {
            //System.Diagnostics.Debug.WriteLine($"Hit: {rating}, count before: {perfectHitCount}");

            if (rating == BeatRating.Perfect)
            {
                perfectHitCount++;
            }
            else
            {
                perfectHitCount = 0;
            }

            //System.Diagnostics.Debug.WriteLine($"count after: {perfectHitCount}");

            if (perfectHitCount >= 3)
            {
                perfectHitCount = 0;
                SpawnMusicBlast(pos, wave);
                //System.Diagnostics.Debug.WriteLine("BLAST FIRED");
            }
        }


        public void SpawnMusicBlast(Vector2 pos, WaveManager wave)
        {
            wave.shockwaves.Add(new Shockwave(pos, 300f, 1.0f));

            int noteCount = 24;
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / noteCount));
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                activeNotes.Add(new Projectile(noteSprite, pos, dir));
            }
        }


        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            orbitAngle += OrbitSpeed * elapsed;
        }

 
        public void UpdateWithEnemies(GameTime gameTime, List<Enemy> enemies, Boss boss = null)
        {
            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                activeNotes[i].Update(gameTime);

                foreach (Enemy e in enemies)
                {
                    activeNotes[i].CheckEnemyHit(e);
                }

                if (boss != null && boss.IsAlive)
                {
                    activeNotes[i].CheckBossHit(boss);
                }

                if (activeNotes[i].IsActive == false)
                {
                    activeNotes.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch sb, Camera camera, Vector2 playerPos, Vector2 playerSize)
        {
            Vector2 playerCenter = playerPos + playerSize / 2f;
            float worldX = playerCenter.X + (float)Math.Cos(orbitAngle) * OrbitRadius;
            float worldY = playerCenter.Y + (float)Math.Sin(orbitAngle) * OrbitRadius;


            Vector2 screenCenter = new Vector2(worldX, worldY) - camera.Position;

            Vector2 pivot = new Vector2(FluteSize / 2f, FluteSize / 2f);
            float scale = FluteSize / (float)fluteSprite.Width;


            sb.Draw(fluteSprite, screenCenter, null, Color.White,orbitAngle, pivot,scale, SpriteEffects.None,0f);

            foreach (Projectile note in activeNotes)
            {
                note.Draw(sb, camera);
            }
        }
    }
}
