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
        Texture2D pixel;

        public List<Projectile> activeNotes = new List<Projectile>();
        public List<PianoDrop> activePianos = new List<PianoDrop>();

        public int perfectHitCount = 0;

        public bool HasFlute = false;
        public bool HasPiano = false;

        float orbitAngle = 0f;
        const float OrbitRadius = 150f;
        const float OrbitSpeed = 2.8f;
        const int FluteSize = 56;

        float pianoCooldown = 0f;
        const float PianoCooldownMax = 8f;
        const int PianoDamage = 40;
        const float PianoTargetRadius = 600f;

        Random rng = new Random();

        // set up the flute spell
        public InstrumentSpell(ContentManager content, GraphicsDevice gd)
        {
            fluteSprite = content.Load<Texture2D>("sprites/Weapon Sprites/Flute");
            noteSprite = content.Load<Texture2D>("sprites/Projectiles/2Eights");

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });
        }


        // count how many perfect player got in a row
        public void ProcessHit(BeatRating rating, Vector2 pos, WaveManager wave)
        {
            if (rating == BeatRating.Perfect)
            {
                perfectHitCount++;
            }
            else
            {
                perfectHitCount = 0;
            }

            if (perfectHitCount >= 3)
            {
                perfectHitCount = 0;
                SpawnMusicBlast(pos, wave);
            }
        }


        // shoot music notes in a ring
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


        // spin the flute around player
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            orbitAngle += OrbitSpeed * elapsed;
        }


        // update notes and pianos and check the hits
        public void UpdateWithEnemies(GameTime gameTime, List<Enemy> enemies, Boss boss, Vector2 playerCenter)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Note projectile updates and hits
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

            // Piano drop auto-fire on cooldown
            if (HasPiano)
            {
                pianoCooldown -= elapsed;
                if (pianoCooldown <= 0f)
                {
                    Vector2? target = PickPianoTarget(enemies, boss, playerCenter);
                    if (target.HasValue)
                    {
                        activePianos.Add(new PianoDrop(target.Value));
                        pianoCooldown = PianoCooldownMax;
                    }
                    else
                    {
                        pianoCooldown = 0.5f;
                    }
                }
            }

            // Update piano drops and apply impact damage
            for (int i = activePianos.Count - 1; i >= 0; i--)
            {
                activePianos[i].Update(elapsed);

                if (activePianos[i].IsImpactingNow())
                {
                    Rectangle impact = activePianos[i].GetImpactRect();

                    foreach (Enemy e in enemies)
                    {
                        if (!e.IsAlive) continue;
                        Rectangle enemyRect = new Rectangle((int)e.Position.X, (int)e.Position.Y, (int)e.Size.X, (int)e.Size.Y);
                        if (impact.Intersects(enemyRect))
                        {
                            Vector2 knockDir = e.Position - new Vector2(impact.X + impact.Width / 2f, impact.Y + impact.Height / 2f);
                            e.TakeDamage(PianoDamage, knockDir, 150f);
                        }
                    }

                    if (boss != null && boss.IsAlive)
                    {
                        Rectangle bossRect = new Rectangle((int)boss.Position.X, (int)boss.Position.Y, (int)boss.Size.X, (int)boss.Size.Y);
                        if (impact.Intersects(bossRect))
                        {
                            Vector2 knockDir = boss.Position - new Vector2(impact.X + impact.Width / 2f, impact.Y + impact.Height / 2f);
                            boss.TakeDamage(PianoDamage, knockDir, 80f);
                        }
                    }

                    activePianos[i].MarkDamageApplied();
                }

                if (!activePianos[i].IsAlive)
                {
                    activePianos.RemoveAt(i);
                }
            }
        }

        // find the closest enemy or boss to drop piano on
        Vector2? PickPianoTarget(List<Enemy> enemies, Boss boss, Vector2 playerCenter)
        {
            Vector2? best = null;
            float bestDist = PianoTargetRadius * PianoTargetRadius;

            foreach (Enemy e in enemies)
            {
                if (!e.IsAlive) continue;
                Vector2 c = e.Position + e.Size / 2f;
                float d = Vector2.DistanceSquared(playerCenter, c);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }

            if (boss != null && boss.IsAlive)
            {
                Vector2 c = boss.Position + boss.Size / 2f;
                float d = Vector2.DistanceSquared(playerCenter, c);
                if (d < bestDist)
                {
                    best = c;
                }
            }

            return best;
        }

        // draw flute and notes and piano stuff
        public void Draw(SpriteBatch sb, Camera camera, Vector2 playerPos, Vector2 playerSize)
        {
            // Friendly piano drops (drawn under flute)
            foreach (PianoDrop drop in activePianos)
            {
                drop.Draw(sb, camera, pixel);
            }

            if (HasFlute)
            {
                Vector2 playerCenter = playerPos + playerSize / 2f;
                float worldX = playerCenter.X + (float)Math.Cos(orbitAngle) * OrbitRadius;
                float worldY = playerCenter.Y + (float)Math.Sin(orbitAngle) * OrbitRadius;

                Vector2 screenCenter = new Vector2(worldX, worldY) - camera.Position;

                Vector2 pivot = new Vector2(FluteSize / 2f, FluteSize / 2f);
                float scale = FluteSize / (float)fluteSprite.Width;

                sb.Draw(fluteSprite, screenCenter, null, Color.White, orbitAngle, pivot, scale, SpriteEffects.None, 0f);
            }

            foreach (Projectile note in activeNotes)
            {
                note.Draw(sb, camera);
            }
        }
    }
}
