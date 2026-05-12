using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Solfège
{
    public enum EnemyType
    {
        Melee,      
        Projectile, 
        Slam      
    }

    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool IsAlive = true;
        public EnemyType Type;

        public int health;
        public int maxHealth;
        public int damage;
        public float moveSpeed;

        public Texture2D texture;
        public Texture2D healthBarBg;
        public Texture2D healthBarFill;


        public float damageCooldown = 0f;
        public const float DamageCooldownMax = 1.0f;


        public Vector2 knockbackVelocity = Vector2.Zero;
        public const float KnockbackDecay = 8f;


        public float shootTimer = 0f;
        public float shootCooldown = 2.5f;
        public const float ShootRange = 350f;


        public float slamTimer = 0f;
        public float slamCooldown = 3.0f;
        public const float SlamRange = 60f;
        public bool JustSlammed = false;


        public float spawnScale = 0.1f;
        public bool spawning = true;


        public float hitFlash = 0f;


        public Vector2 SeparationForce = Vector2.Zero;

        public Enemy(Vector2 spawnPosition, GraphicsDevice graphicsDevice, EnemyType type, int wave = 1)
        {
            Position = spawnPosition;
            Type = type;


            float waveScale = 1f + (wave - 1) * 0.25f;

            switch (type)
            {
                case EnemyType.Melee:
                    maxHealth = (int)(30 * waveScale);
                    damage = (int)(10 * waveScale);
                    moveSpeed = 90f + wave * 5f;
                    Size = new Vector2(32, 48);
                    break;

                case EnemyType.Projectile:
                    maxHealth = (int)(20 * waveScale);
                    damage = (int)(6 * waveScale);
                    moveSpeed = 70f + wave * 3f;
                    Size = new Vector2(30, 44);
                    shootTimer = (float)new Random().NextDouble() * 1.5f;
                    break;

                case EnemyType.Slam:
                    maxHealth = (int)(50 * waveScale);
                    damage = (int)(18 * waveScale);
                    moveSpeed = 55f + wave * 2f;
                    Size = new Vector2(36, 52);
                    slamTimer = (float)new Random().NextDouble() * 1.5f;
                    break;
            }

            health = maxHealth;


            Color bodyColor = type == EnemyType.Melee ? new Color(220, 60, 60) :type == EnemyType.Projectile ? new Color(230, 140, 40) :new Color(120, 60, 200);

            int w = (int)Size.X, h = (int)Size.Y;
            texture = new Texture2D(graphicsDevice, w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bodyColor;
            }
                texture.SetData(pixels);


            healthBarBg = new Texture2D(graphicsDevice, 1, 1);
            healthBarBg.SetData(new[] { Color.DarkRed });

            healthBarFill = new Texture2D(graphicsDevice, 1, 1);
            healthBarFill.SetData(new[] { Color.LimeGreen });
        }


        public int Damage 
        {
            get; 
            private set; 
        }
        public int Health 
        { 
            get; 
            private set;
        }
        public int MaxHealth { 
            get;
            private set; 
        }
        public EnemyType EnemyKind { 
            get; 
            private set; 
        }


        public EnemyProjectile Update(GameTime gameTime, Vector2 playerPosition)
        {
            if (!IsAlive)
            {
                return null;
            }
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            JustSlammed = false;


            if (spawning)
            {
                spawnScale = Math.Min(1f, spawnScale + elapsed * 6f);
                if (spawnScale >= 1f)
                {
                    spawning = false;
                }
            }


            if (hitFlash > 0f)
            {
                hitFlash -= elapsed;
            }


            if (damageCooldown > 0f)
            {
                damageCooldown -= elapsed;
            }


            if (knockbackVelocity != Vector2.Zero)
            {
                Position += knockbackVelocity * elapsed;
                knockbackVelocity -= knockbackVelocity * KnockbackDecay * elapsed;
                if (knockbackVelocity.Length() < 2f)
                {
                    knockbackVelocity = Vector2.Zero;
                }
            }


            Vector2 direction = playerPosition - (Position + Size / 2f);
            float dist = direction.Length();


            float stopDist = (Type == EnemyType.Projectile) ? ShootRange * 0.75f : 0f;

            if (dist > stopDist + 2f)
            {
                direction.Normalize();
                Position += (direction * moveSpeed + SeparationForce) * elapsed;
            }
            else
            {
                Position += SeparationForce * elapsed;
            }


            EnemyProjectile fired = null;

            if (Type == EnemyType.Projectile)
            {
                shootTimer -= elapsed;
                if (shootTimer <= 0f && dist < ShootRange)
                {
                    shootTimer = shootCooldown;
                    fired = new EnemyProjectile(Position, playerPosition, damage);
                }
            }

            if (Type == EnemyType.Slam)
            {
                slamTimer -= elapsed;
                if (slamTimer <= 0f && dist < SlamRange + Size.X)
                {
                    slamTimer = slamCooldown;
                    JustSlammed = true;
                }
            }


            SeparationForce = Vector2.Zero;

            return fired;
        }



        public int CheckContactDamage(Vector2 playerPos, Vector2 playerSize)
        {
            if (!IsAlive || damageCooldown > 0f){
                return 0;
            }
            if (Type == EnemyType.Projectile)
            {
                return 0; 
            }

            Rectangle enemyRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (enemyRect.Intersects(playerRect))
            {
                damageCooldown = DamageCooldownMax;
                return damage;
            }
            return 0;
        }


        public void TakeDamage(int amount, Vector2 knockbackDir, float knockbackForce = 200f)
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

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, Vector2.Zero, 0f);
        }


        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsAlive)
            {
                return;
            }

            Vector2 screenPos = Position - camera.Position;


            if (spawning && spawnScale < 1f)
            {
                Vector2 origin = Size / 2f;
                spriteBatch.Draw(texture,screenPos + origin,null,hitFlash > 0f ? Color.White : Color.White,0f,origin,spawnScale,SpriteEffects.None, 0f);
            }
            else
            {
                Color tint = hitFlash > 0f ? Color.White * 2f : Color.White; 
                spriteBatch.Draw(texture, screenPos, tint);
            }


            int barW = (int)Size.X;
            int filled = (int)(barW * ((float)health / maxHealth));
            int barY = (int)screenPos.Y - 10;

            spriteBatch.Draw(healthBarBg, new Rectangle((int)screenPos.X, barY, barW, 6), Color.White);
            spriteBatch.Draw(healthBarFill, new Rectangle((int)screenPos.X, barY, filled, 6), Color.White);
        }
    }


    
}
