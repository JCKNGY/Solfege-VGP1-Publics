using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class WaveManager
    {
        public int CurrentWave = 0;
        public bool WaveActive = false;
        public int TotalKills = 0;
        public int CoinsEarned = 0;

        public List<Enemy> enemies = new List<Enemy>();
        public List<EnemyProjectile> projectiles = new List<EnemyProjectile>();
        public List<Shockwave> shockwaves = new List<Shockwave>();
        public List<DroppedCoin> coins = new List<DroppedCoin>();

        public Boss boss = null;
        public float bossAnnounceTimer = 0f;

        public int BossesKilled = 0;
        public bool BossJustDied = false;

        public int enemiesToSpawn = 0;
        public int spawnedThisWave = 0;
        public float spawnTimer = 0f;
        public float spawnInterval = 2.0f;

        public const int SpawnMargin = 100;
        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;

        public Texture2D pixel;
        public GraphicsDevice graphicsDevice;
        public Random rng = new Random();

        public Texture2D meleeIdleTex;
        public Texture2D meleeAttackTex;
        public Texture2D rangeIdleTex;
        public Texture2D rangeAttackTex;
        public Texture2D aoeIdleTex;
        public Texture2D aoeAttackTex;
        public Texture2D noteProjectileTex;
        public Texture2D bossProjectileTex;


        public bool BossActive
        {
            get
            {
                return boss != null && boss.IsAlive;
            }
        }


        public List<Enemy> Enemies
        {
            get
            {
                return enemies;
            }
        }


        public void ClearBossJustDied()
        {
            BossJustDied = false;
        }


        public WaveManager(GraphicsDevice gd, ContentManager content)
        {
            graphicsDevice = gd;
            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });

            meleeIdleTex = content.Load<Texture2D>("sprites/Zombies/MZombieMain");
            meleeAttackTex = content.Load<Texture2D>("sprites/Zombies/MZombieHit");
            rangeIdleTex = content.Load<Texture2D>("sprites/Zombies/ZombieRangeIdle");
            rangeAttackTex = content.Load<Texture2D>("sprites/Zombies/ZombieRangeAttack");
            aoeIdleTex = content.Load<Texture2D>("sprites/Zombies/ZombieAOEIdle");
            aoeAttackTex = content.Load<Texture2D>("sprites/Zombies/ZombieAOEAttack");
            noteProjectileTex = content.Load<Texture2D>("sprites/Projectiles/EigthNote");
            bossProjectileTex = content.Load<Texture2D>("sprites/Projectiles/TrebleClef");
        }


        public void StartNextWave(Vector2 playerPosition)
        {
            CurrentWave++;
            spawnedThisWave = 0;
            spawnTimer = 1.0f;
            enemiesToSpawn = 6 + CurrentWave * 2;
            spawnInterval = Math.Max(0.5f, 2.0f - CurrentWave * 0.12f);
            WaveActive = true;
        }


        // main wave loop: spawn enemies, update everything, check wave end and boss spawn
        public void Update(GameTime gameTime, Vector2 playerPosition, Conductor conductor)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (WaveActive && spawnedThisWave < enemiesToSpawn)
            {
                spawnTimer -= elapsed;

                if (spawnTimer <= 0f)
                {
                    SpawnEnemy(playerPosition);
                    spawnTimer = spawnInterval;
                }
            }

            // push zombies away from each other so they don't stack on the same spot
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsAlive)
                {
                    continue;
                }

                for (int j = i + 1; j < enemies.Count; j++)
                {
                    if (!enemies[j].IsAlive)
                    {
                        continue;
                    }

                    Vector2 diff = enemies[i].Position - enemies[j].Position;
                    float dist = diff.Length();
                    float minD = (enemies[i].Size.X + enemies[j].Size.X) / 2f + 4f;

                    if (dist < minD && dist > 0.01f)
                    {
                        Vector2 push = diff / dist * (minD - dist) * 3f;
                        enemies[i].SeparationForce += push;
                        enemies[j].SeparationForce -= push;
                    }
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy e = enemies[i];

                Vector2 playerCenter = playerPosition + conductor.Size / 2f;
                EnemyProjectile proj = e.Update(gameTime, playerCenter);

                if (proj != null)
                {
                    proj.Sprite = noteProjectileTex;
                    projectiles.Add(proj);
                }

                if (e.JustSlammed)
                {
                    shockwaves.Add(new Shockwave(e.Position, 160f, 0.6f));
                }

                int contactDmg = e.CheckContactDamage(conductor.Position, conductor.Size);

                if (contactDmg > 0)
                {
                    conductor.TakeDamage(contactDmg);
                }

                if (!e.IsAlive)
                {
                    SpawnCoinDrop(e.Position, CoinValueForType(e.Type));
                    TotalKills++;
                    enemies.RemoveAt(i);
                }
            }

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                EnemyProjectile p = projectiles[i];
                p.Update(gameTime);

                int dmg = p.CheckHit(playerPosition, conductor.Size);

                if (dmg > 0)
                {
                    conductor.TakeDamage(dmg);
                }

                if (!p.IsAlive)
                {
                    projectiles.RemoveAt(i);
                }
            }

            for (int i = shockwaves.Count - 1; i >= 0; i--)
            {
                shockwaves[i].Update(elapsed);

                if (!shockwaves[i].IsAlive)
                {
                    shockwaves.RemoveAt(i);
                }
            }

            for (int i = coins.Count - 1; i >= 0; i--)
            {
                coins[i].Update(elapsed, playerPosition);

                if (coins[i].Collected)
                {
                    CoinsEarned += coins[i].Value;
                    coins.RemoveAt(i);
                }
                else if (coins[i].Expired)
                {
                    coins.RemoveAt(i);
                }
            }

            conductor.Spell.HasFlute = conductor.HasFlute;
            conductor.Spell.HasPiano = conductor.HasPiano;
            conductor.Spell.UpdateWithEnemies(gameTime, enemies, boss, conductor.Position + conductor.Size / 2f);

            // wave ended, every 3 waves we spawn a boss
            if (WaveActive && spawnedThisWave >= enemiesToSpawn && enemies.Count == 0)
            {
                WaveActive = false;

                if (CurrentWave % 3 == 0)
                {
                    BossType nextType;

                    if (BossesKilled == 0)
                    {
                        nextType = BossType.Flute;
                    }
                    else
                    {
                        nextType = BossType.Piano;
                    }

                    SpawnBoss(playerPosition, nextType);
                }
            }

            if (bossAnnounceTimer > 0f)
            {
                bossAnnounceTimer -= elapsed;
            }

            if (BossActive)
            {
                boss.Update(gameTime, playerPosition, projectiles, shockwaves);

                int bossDmg = boss.CheckContactDamage(conductor.Position, conductor.Size);

                if (bossDmg > 0)
                {
                    conductor.TakeDamage(bossDmg);
                }

                if (boss.Type == BossType.Piano)
                {
                    foreach (PianoDrop drop in boss.PianoDrops)
                    {
                        int dropDmg = drop.CheckHit(conductor.Position, conductor.Size, conductor.MaxHealth);

                        if (dropDmg > 0)
                        {
                            conductor.TakeDamage(dropDmg);
                        }
                    }
                }
            }

            // boss death detection is OUTSIDE the BossActive block, because the boss can
            // get killed by the spell or melee earlier in the frame
            if (boss != null && !boss.IsAlive)
            {
                SpawnCoinDrop(boss.Position, 10);
                TotalKills++;
                BossesKilled++;
                BossJustDied = true;
                boss = null;
            }
        }


        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (DroppedCoin c in coins)
            {
                c.Draw(spriteBatch, camera, pixel);
            }

            foreach (Enemy e in enemies)
            {
                e.Draw(spriteBatch, camera);
            }

            if (BossActive)
            {
                boss.Draw(spriteBatch, camera);
            }

            foreach (EnemyProjectile p in projectiles)
            {
                p.Draw(spriteBatch, camera, pixel);
            }

            foreach (Shockwave s in shockwaves)
            {
                s.Draw(spriteBatch, camera, pixel);
            }

            DrawOffscreenIndicators(spriteBatch, camera);
            DrawBossOffscreenIndicator(spriteBatch, camera);
        }


        public void SpawnBoss(Vector2 playerPos, BossType type = BossType.Flute)
        {
            Vector2 spawnPos = ChooseSpawnPosition(playerPos);
            boss = new Boss(spawnPos, graphicsDevice, type, bossProjectileTex);
            bossAnnounceTimer = 3f;
        }


        public void SpawnEnemy(Vector2 playerPos)
        {
            Vector2 spawnPos = ChooseSpawnPosition(playerPos);
            EnemyType type = ChooseEnemyType();

            Texture2D idle;
            Texture2D attack;

            switch (type)
            {
                case EnemyType.Projectile:
                    idle = rangeIdleTex;
                    attack = rangeAttackTex;
                    break;
                case EnemyType.Slam:
                    idle = aoeIdleTex;
                    attack = aoeAttackTex;
                    break;
                default:
                    idle = meleeIdleTex;
                    attack = meleeAttackTex;
                    break;
            }

            enemies.Add(new Enemy(spawnPos, graphicsDevice, type, CurrentWave, idle, attack));
            spawnedThisWave++;
        }


        public Vector2 ChooseSpawnPosition(Vector2 playerPos)
        {
            int side = rng.Next(4);
            int spread = 400;

            float sx;
            float sy;
            float halfW = ScreenWidth / 2f + SpawnMargin;
            float halfH = ScreenHeight / 2f + SpawnMargin;

            switch (side)
            {
                case 0:
                    sx = playerPos.X + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    sy = playerPos.Y - halfH;
                    break;
                case 1:
                    sx = playerPos.X + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    sy = playerPos.Y + halfH;
                    break;
                case 2:
                    sx = playerPos.X - halfW;
                    sy = playerPos.Y + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    break;
                default:
                    sx = playerPos.X + halfW;
                    sy = playerPos.Y + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    break;
            }

            sx = Math.Max(0, Math.Min(Map.MapTilesWide * Map.TileWidth - 32, sx));
            sy = Math.Max(0, Math.Min(Map.MapTilesTall * Map.TileHeight - 48, sy));

            return new Vector2(sx, sy);
        }


        // pool of zombie types gets bigger as the player kill more bosses
        public EnemyType ChooseEnemyType()
        {
            List<EnemyType> pool = new List<EnemyType> { EnemyType.Melee };

            if (BossesKilled >= 1)
            {
                pool.Add(EnemyType.Projectile);
                pool.Add(EnemyType.Projectile);
            }

            if (BossesKilled >= 2)
            {
                pool.Add(EnemyType.Slam);
            }

            return pool[rng.Next(pool.Count)];
        }


        public void DrawOffscreenIndicators(SpriteBatch spriteBatch, Camera camera)
        {
            const int Pad = 24;

            foreach (Enemy e in enemies)
            {
                Vector2 screenPos = e.Position - camera.Position;
                bool onScreen = screenPos.X > -e.Size.X && screenPos.X < ScreenWidth + e.Size.X && screenPos.Y > -e.Size.Y && screenPos.Y < ScreenHeight + e.Size.Y;

                if (onScreen)
                {
                    continue;
                }

                float ax = Math.Max(Pad, Math.Min(ScreenWidth - Pad, screenPos.X));
                float ay = Math.Max(Pad, Math.Min(ScreenHeight - Pad, screenPos.Y));

                Color arrowColor = Color.Red;

                if (e.Type == EnemyType.Projectile)
                {
                    arrowColor = Color.Orange;
                }

                if (e.Type == EnemyType.Slam)
                {
                    arrowColor = Color.Purple;
                }

                float angle = (float)Math.Atan2(screenPos.Y - ay, screenPos.X - ax);
                DrawArrow(spriteBatch, new Vector2(ax, ay), angle, arrowColor);
            }
        }


        public void DrawBossOffscreenIndicator(SpriteBatch spriteBatch, Camera camera)
        {
            if (!BossActive)
            {
                return;
            }

            const int Pad = 24;
            Vector2 screenPos = boss.Position - camera.Position;
            bool onScreen = screenPos.X > -boss.Size.X && screenPos.X < ScreenWidth + boss.Size.X && screenPos.Y > -boss.Size.Y && screenPos.Y < ScreenHeight + boss.Size.Y;

            if (onScreen)
            {
                return;
            }

            float ax = Math.Max(Pad, Math.Min(ScreenWidth - Pad, screenPos.X));
            float ay = Math.Max(Pad, Math.Min(ScreenHeight - Pad, screenPos.Y));
            float angle = (float)Math.Atan2(screenPos.Y - ay, screenPos.X - ax);
            DrawArrow(spriteBatch, new Vector2(ax, ay), angle, new Color(160, 0, 160));
        }


        public void DrawArrow(SpriteBatch spriteBatch, Vector2 pos, float angle, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X - 6, (int)pos.Y - 6, 12, 12), null, color, angle, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0f);
        }


        public void SpawnCoinDrop(Vector2 pos, int value)
        {
            int count = Math.Max(1, value / 2);

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2((float)(rng.NextDouble() - 0.5) * 40, (float)(rng.NextDouble() - 0.5) * 40);
                coins.Add(new DroppedCoin(pos + offset, value));
            }
        }


        public void SpendCoins(int amount)
        {
            CoinsEarned -= amount;

            if (CoinsEarned < 0)
            {
                CoinsEarned = 0;
            }
        }


        public int CoinValueForType(EnemyType t)
        {
            switch (t)
            {
                case EnemyType.Melee:
                    return 1;
                case EnemyType.Projectile:
                    return 2;
                case EnemyType.Slam:
                    return 3;
                default:
                    return 1;
            }
        }
    }
}
