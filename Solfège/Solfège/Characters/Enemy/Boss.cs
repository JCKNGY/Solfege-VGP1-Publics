using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public enum BossType
    {
        Flute,
        Piano
    }

    public class Boss
    {
        public Vector2 Position;
        public Vector2 Size = new Vector2(80, 100);
        public bool IsAlive = true;
        public BossType Type;

        public int health;
        public int maxHealth = 500;
        public int damage = 15;
        public float moveSpeed = 65f;

        // Circle / flute attack
        float circleAttackTimer = 5f;
        const float CircleAttackInterval = 5f;
        const int CircleProjectileDamage = 12;
        const int CircleProjectileCount = 24;

        // Boss 2 piano drops
        public List<PianoDrop> PianoDrops = new List<PianoDrop>();
        bool pianoPhaseActive = false;
        float pianoDropTimer = 0f;
        const float PianoDropInterval = 10f;

        float damageCooldown = 0f;
        const float DamageCooldownMax = 1.0f;

        public Vector2 knockbackVelocity = Vector2.Zero;
        const float KnockbackDecay = 5f;

        float hitFlash = 0f;

        float spawnScale = 0.1f;
        bool spawning = true;

        Texture2D texture;
        Texture2D healthBarBg;
        Texture2D healthBarFill;
        Texture2D pixel;
        Texture2D projectileSprite;

        // make the boss
        public Boss(Vector2 spawnPosition, GraphicsDevice graphicsDevice, BossType type = BossType.Flute, Texture2D projectileSprite = null)
        {
            Position = spawnPosition;
            Type = type;
            health = maxHealth;
            this.projectileSprite = projectileSprite;

            // Boss 1 (Flute) = teal, Boss 2 (Piano) = dark gold
            Color bossColor = (type == BossType.Flute)
                ? new Color(0, 160, 200)
                : new Color(180, 130, 20);

            int w = (int)Size.X, h = (int)Size.Y;
            texture = new Texture2D(graphicsDevice, w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bossColor;
            texture.SetData(pixels);

            healthBarBg = new Texture2D(graphicsDevice, 1, 1);
            healthBarBg.SetData(new[] { Color.DarkRed });

            healthBarFill = new Texture2D(graphicsDevice, 1, 1);
            healthBarFill.SetData(new[] { Color.Gold });

            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        // boss move and shoot stuff
        public void Update(GameTime gameTime, Vector2 playerPosition, List<EnemyProjectile> projectiles, List<Shockwave> shockwaves)
        {
            if (!IsAlive) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (spawning)
            {
                spawnScale = Math.Min(1f, spawnScale + elapsed * 4f);
                if (spawnScale >= 1f) spawning = false;
            }

            if (hitFlash > 0f) hitFlash -= elapsed;
            if (damageCooldown > 0f) damageCooldown -= elapsed;

            if (knockbackVelocity != Vector2.Zero)
            {
                Position += knockbackVelocity * elapsed;
                knockbackVelocity -= knockbackVelocity * KnockbackDecay * elapsed;
                if (knockbackVelocity.Length() < 2f)
                    knockbackVelocity = Vector2.Zero;
            }

            Vector2 toPlayer = playerPosition - (Position + Size / 2f);
            float dist = toPlayer.Length();
            if (dist > 2f)
            {
                toPlayer.Normalize();
                Position += toPlayer * moveSpeed * elapsed;
            }

            // Both bosses use the circle attack
            circleAttackTimer -= elapsed;
            if (circleAttackTimer <= 0f)
            {
                circleAttackTimer = CircleAttackInterval;
                FireCircleAttack(projectiles, shockwaves);
            }

            // Piano boss: piano drops when below 50% HP
            if (Type == BossType.Piano)
            {
                if (!pianoPhaseActive && health <= maxHealth / 2)
                {
                    pianoPhaseActive = true;
                    pianoDropTimer = 0f;
                }

                if (pianoPhaseActive)
                {
                    pianoDropTimer -= elapsed;
                    if (pianoDropTimer <= 0f)
                    {
                        pianoDropTimer = PianoDropInterval;
                        PianoDrops.Add(new PianoDrop(playerPosition));
                    }
                }

                for (int i = PianoDrops.Count - 1; i >= 0; i--)
                {
                    PianoDrops[i].Update(elapsed);
                    if (!PianoDrops[i].IsAlive)
                        PianoDrops.RemoveAt(i);
                }
            }
        }

        // shoot the music notes in a circle around boss
        void FireCircleAttack(List<EnemyProjectile> projectiles, List<Shockwave> shockwaves)
        {
            Vector2 center = Position + Size / 2f;
            shockwaves.Add(new Shockwave(center, 300f, 1.0f));

            for (int i = 0; i < CircleProjectileCount; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / CircleProjectileCount));
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                EnemyProjectile p = new EnemyProjectile(center, center + dir * 100f, CircleProjectileDamage);
                p.Sprite = projectileSprite;
                projectiles.Add(p);
            }
        }

        // check if boss hit the player
        public int CheckContactDamage(Vector2 playerPos, Vector2 playerSize)
        {
            if (!IsAlive || damageCooldown > 0f) return 0;

            Rectangle bossRect   = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (bossRect.Intersects(playerRect))
            {
                damageCooldown = DamageCooldownMax;
                return damage;
            }
            return 0;
        }

        // boss take damage and get knocked back
        public void TakeDamage(int amount, Vector2 knockbackDir, float knockbackForce = 150f)
        {
            health -= amount;
            hitFlash = 0.12f;

            if (knockbackDir != Vector2.Zero)
            {
                Vector2 kb = knockbackDir;
                kb.Normalize();
                knockbackVelocity += kb * knockbackForce;
            }

            if (health <= 0)
            {
                health = 0;
                IsAlive = false;
            }
        }

        // damage but no knockback
        public void TakeDamage(int amount) => TakeDamage(amount, Vector2.Zero, 0f);

        // draw the boss and piano stuff
        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsAlive) return;

            Vector2 screenPos = Position - camera.Position;

            if (spawning && spawnScale < 1f)
            {
                Vector2 origin = Size / 2f;
                spriteBatch.Draw(texture, screenPos + origin, null,
                    hitFlash > 0f ? Color.White * 2f : Color.White,
                    0f, origin, spawnScale, SpriteEffects.None, 0f);
            }
            else
            {
                Color tint = hitFlash > 0f ? Color.White * 2f : Color.White;
                spriteBatch.Draw(texture, screenPos, tint);
            }

            int barW   = (int)Size.X;
            int filled = (int)(barW * ((float)health / maxHealth));
            int barY   = (int)screenPos.Y - 12;
            spriteBatch.Draw(healthBarBg,   new Rectangle((int)screenPos.X, barY, barW,   8), Color.White);
            spriteBatch.Draw(healthBarFill, new Rectangle((int)screenPos.X, barY, filled, 8), Color.White);

            if (Type == BossType.Piano)
            {
                foreach (PianoDrop drop in PianoDrops)
                    drop.Draw(spriteBatch, camera, pixel);
            }
        }
    }
}
