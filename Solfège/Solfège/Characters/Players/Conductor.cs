using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solfège
{
    public class Conductor
    {
        private Texture2D texture;
        public Vector2 Position;
        public Vector2 Size;

        public const float MoveSpeed = 200f;
        public int BaseDamage = 5;

        public int MaxHealth = 100;
        public int Health = 100;
        public bool IsAlive = true;

        public float attackCooldown = 0f;
        public const float AttackRate = 0.20f;

        public const float AttackRange = 140f;

        public bool HasFlute = false;
        public bool HasPiano = false;

        public BeatRating LastAttackRating { get; private set; } = BeatRating.None;

        public float attackFlash = 0f;

        public KeyboardState prevKb;

        public InstrumentSpell Spell;

        // make the player
        public Conductor(ContentManager content, GraphicsDevice graphicsDevice)
        {
            texture = content.Load<Texture2D>("sprites/Character Sprite/ConductorFront");
            Size = new Vector2(40, 60);

            Spell = new InstrumentSpell(content, graphicsDevice);
        }

        // player take damage
        public void TakeDamage(int amount)
        {
            Health -= amount;

            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
            }
        }

        // move the player and attack on the beat
        public void Update(GameTime gameTime, GamePadState gp, KeyboardState kb, Map map, MetronomeSystem metronome, WaveManager waveManager)
        {
            if (!IsAlive)
            {
                return;
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 input = Vector2.Zero;

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
            {
                input.Y = -1f;
            }
            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
            {
                input.Y = 1f;
            }
            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
            {
                input.X = -1f;
            }
            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
            {
                input.X = 1f;
            }
            if (input.LengthSquared() > 1f)
            {
                input.Normalize();
            }

            Position += input * MoveSpeed * elapsed;

            if (attackCooldown > 0f) 
            {
                attackCooldown -= elapsed;
            }
            if (attackFlash > 0f)
            {
                attackFlash -= elapsed;
            }

            bool attackPressed = (kb.IsKeyDown(Keys.Space) && !prevKb.IsKeyDown(Keys.Space))
                              || (kb.IsKeyDown(Keys.J) && !prevKb.IsKeyDown(Keys.J))
                              || gp.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A);

            if (attackPressed && !metronome.HasAttackedThisCycle)
            {
                BeatRating rating = metronome.RegisterAction();
                LastAttackRating = rating;

                Vector2 center = Position + Size / 2f;

                if (HasFlute)
                    Spell.ProcessHit(rating, center, waveManager);

                if (attackCooldown <= 0f)
                {
                    float damageMultiplier = metronome.GetDamageMultiplier(rating);
                    int finalDamage = (int)(BaseDamage * damageMultiplier);

                    float force = 80f;

                    if (rating == BeatRating.Perfect)
                    {
                        force = 350f;
                    }
                    else if (rating == BeatRating.Good)
                    {
                        force = 220f;
                    }

                    foreach (Enemy e in waveManager.Enemies)
                    {
                        if (!e.IsAlive)
                        {
                            continue;
                        }

                        Vector2 enemyCenter = e.Position + e.Size / 2f;
                        float dist = Vector2.Distance(center, enemyCenter);

                        if (dist <= AttackRange)
                        {
                            Vector2 knockDir = enemyCenter - center;
                            e.TakeDamage(finalDamage, knockDir, force);
                        }
                    }

                    if (waveManager.boss != null && waveManager.boss.IsAlive)
                    {
                        Vector2 bossCenter = waveManager.boss.Position + waveManager.boss.Size / 2f;
                        if (Vector2.Distance(center, bossCenter) <= AttackRange)
                        {
                            Vector2 knockDir = bossCenter - center;
                            waveManager.boss.TakeDamage(finalDamage, knockDir, force);
                        }
                    }

                    attackCooldown = AttackRate;
                    attackFlash = 0.12f;
                }
            }

            Spell.Update(gameTime);

            prevKb = kb;
        }

        // just movement, no attack
        public void Update(GameTime gameTime, GamePadState gp, KeyboardState kb, Map map)
        {
            if (!IsAlive)
            {
                return;
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 input = Vector2.Zero;

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) 
            {
                input.Y = -1f;
            }
            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
            {
                input.Y = 1f;
            }
            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
            {
                input.X = -1f;
            }
            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
            {
                input.X = 1f;
            }

            
            if (input.LengthSquared() > 1f)
            {
                input.Normalize();
            }

            Position += input * MoveSpeed * elapsed;

            Spell.Update(gameTime); 

            prevKb = kb;
        }

        // draw player and the flute
        public void Draw(SpriteBatch spriteBatch, Camera camera, SpriteFont font)
        {
            Vector2 spriteOffset = new Vector2((Size.X - texture.Width) / 2f,(Size.Y - texture.Height) / 2f);
            Vector2 screenPos = Position + spriteOffset - camera.Position;

            Color tint = Color.White;
            if (attackFlash > 0f)
            {
                tint = Color.White * 1.8f;
            }

            spriteBatch.Draw(texture, screenPos, tint);

            Spell.HasFlute = HasFlute;
            Spell.HasPiano = HasPiano;
            Spell.Draw(spriteBatch, camera, Position, Size);
        }
    }
}
