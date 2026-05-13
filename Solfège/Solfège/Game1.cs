using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfège
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;

        public TitleScreen titleScreen;
        public Shop shop;
        public GameScreen currentScreen = GameScreen.Title;

        public Map map;
        public Conductor Conductor;
        public Camera camera;
        public MetronomeSystem metronome;
        public WaveManager waveManager;
        public Texture2D texture;
        public Texture2D pixel;

        SpriteFont font;
        SpriteFont titleFont;
        SpriteFont menuFont;

        public Song titleMusic;
        public Song gameMusic;

        KeyboardState oldKb;

        public static readonly Color ColGold = new Color(201, 168, 76);
        public static readonly Color ColGold2 = new Color(232, 201, 122);
        public static readonly Color ColMuted = new Color(107, 102, 88);
        public static readonly Color ColWhite = new Color(232, 228, 217);
        public static readonly Color ColDeep = new Color(8, 8, 16);

        public string[] pauseLabels = { "Resume", "Settings", "Quit to Title" };
        public int pauseIndex = 0;
        public float pauseGlowTimer = 0f;
        public Rectangle[] pauseRects = new Rectangle[3];
        public MouseState oldMouse;





        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Font");
            titleFont = font;
            menuFont = font;

            titleScreen = new TitleScreen(GraphicsDevice, titleFont, menuFont, font, Content);

            titleScreen.OnStartGame += StartGame;
            titleScreen.OnNewGame += NewGame;
            titleScreen.OnExitGame += ExitGame;

            map = new Map(Content, GraphicsDevice);
            camera = new Camera(ScreenWidth, ScreenHeight, map.MapWidthPixels, map.MapHeightPixels);
            Conductor = new Conductor(Content, GraphicsDevice);
            metronome = new MetronomeSystem(Content, GraphicsDevice);
            waveManager = new WaveManager(GraphicsDevice, Content);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            shop = new Shop(font, pixel, ScreenWidth, ScreenHeight);

            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
            camera.CenterOn(Conductor.Position, Conductor.Size);

            texture = Content.Load<Texture2D>("sprites/Ui/solfegeTitle");

            titleMusic = Content.Load<Song>("Music/TitleMusic");
            gameMusic = Content.Load<Song>("Music/120");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1f;
            MediaPlayer.Play(titleMusic);
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        public void StartGame()
        {
            currentScreen = GameScreen.Playing;
            waveManager.StartNextWave(Conductor.Position);
            ApplyAudioSettings();
            MediaPlayer.Play(gameMusic);
        }

        public void NewGame()
        {
            currentScreen = GameScreen.Playing;
            Conductor.BaseDamage = 5;
            Conductor.MaxHealth = 100;
            Conductor.Health = Conductor.MaxHealth;
            Conductor.IsAlive = true;
            Conductor.HasFlute = false;
            Conductor.HasPiano = false;
            Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
            waveManager = new WaveManager(GraphicsDevice, Content);
            metronome.ResetStreak();
            waveManager.StartNextWave(Conductor.Position);
            ApplyAudioSettings();
            MediaPlayer.Play(gameMusic);
        }




        public void ExitGame()
        {
            this.Exit();
        }

        public void ApplyAudioSettings()
        {
            if (titleScreen == null)
            {
               return;
            }
            MediaPlayer.Volume = titleScreen.MusicVolume * titleScreen.MasterVolume;
            SoundEffect.MasterVolume = titleScreen.SfxVolume * titleScreen.MasterVolume;
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gp = GamePad.GetState(PlayerIndex.One);
            KeyboardState kb = Keyboard.GetState();
            IsMouseVisible = (currentScreen != GameScreen.Playing);
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // TODO: Add your update logic here
            if (currentScreen == GameScreen.Title || currentScreen == GameScreen.Settings)
            {
                titleScreen.Update(gameTime);
                currentScreen = titleScreen.CurrentScreen;
            }
            else if (currentScreen == GameScreen.Playing)
            {
                if (!oldKb.IsKeyDown(Keys.Escape) && kb.IsKeyDown(Keys.Escape))
                {
                    currentScreen = GameScreen.Paused;
                    pauseIndex = 0;
                    MediaPlayer.Pause();
                }
                else
                {
                    Conductor.Update(gameTime, gp, kb, map, metronome, waveManager);
                    camera.Update(Conductor.Position, Conductor.Size);
                    metronome.Update(gameTime, Conductor);
                    //System.Diagnostics.Debug.WriteLine("Conductor Size: " + Conductor.Size + " Center: " + (Conductor.Position + Conductor.Size / 2f));
                    waveManager.Update(gameTime, Conductor.Position + Conductor.Size / 2f, Conductor);
                    if (!waveManager.WaveActive && !waveManager.BossActive && !waveManager.BossJustDied)
                    {
                        waveManager.StartNextWave(Conductor.Position);
                    }
                    if (waveManager.BossJustDied)
                    {
                        waveManager.ClearBossJustDied();
                        if (waveManager.BossesKilled == 1)
                        {
                            Conductor.HasFlute = true;
                            currentScreen = GameScreen.FluteBanner;
                        }
                        else if (waveManager.BossesKilled == 2)
                        {
                            Conductor.HasPiano = true;
                            currentScreen = GameScreen.PianoBanner;
                        }
                        else
                        {
                            shop.OnEnter();
                            currentScreen = GameScreen.Shop;
                        }
                    }
                        CollisionManager.Update(Conductor, waveManager);
                    if (!Conductor.IsAlive)
                    {
                        currentScreen = GameScreen.GameOver;
                    }
                    }
                }
            else if (currentScreen == GameScreen.Paused)
            {
                pauseGlowTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                UpdatePauseInput(kb);
            }
            else if (currentScreen == GameScreen.Shop)
            {
                shop.Update(gameTime, kb, oldKb, waveManager, Conductor, ref currentScreen);
            }
            else if (currentScreen == GameScreen.FluteBanner)
            {
                MouseState mouse = Mouse.GetState();
                bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
                bool dismiss = (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter))
                            || (!oldKb.IsKeyDown(Keys.Space) && kb.IsKeyDown(Keys.Space))
                            || (!oldKb.IsKeyDown(Keys.E)     && kb.IsKeyDown(Keys.E))
                            || mouseClicked;
                if (dismiss)
                {
                    shop.OnEnter();
                    currentScreen = GameScreen.Shop;
                }
                oldMouse = mouse;
            }
            else if (currentScreen == GameScreen.PianoBanner)
            {
                MouseState mouse = Mouse.GetState();
                bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
                bool dismiss = (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter))
                            || (!oldKb.IsKeyDown(Keys.Space) && kb.IsKeyDown(Keys.Space))
                            || (!oldKb.IsKeyDown(Keys.E)     && kb.IsKeyDown(Keys.E))
                            || mouseClicked;
                if (dismiss)
                {
                    shop.OnEnter();
                    currentScreen = GameScreen.Shop;
                }
                oldMouse = mouse;
            }
            else if (currentScreen == GameScreen.GameOver)
            {
                MouseState mouse = Mouse.GetState();
                bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
                bool enterPressed = !oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter);
                oldMouse = mouse;
                if (enterPressed || mouseClicked)
                {
                    currentScreen = GameScreen.Title;
                    Conductor.Health = Conductor.MaxHealth;
                    Conductor.IsAlive = true;
                    Conductor.BaseDamage = 5;
                    Conductor.MaxHealth = 100;
                    Conductor.HasFlute = false;
                    Conductor.HasPiano = false;
                    Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
                    waveManager = new WaveManager(GraphicsDevice, Content);
                    metronome.ResetStreak();
                    MediaPlayer.Play(titleMusic);
                }
            }

            oldKb = kb;
            oldMouse = Mouse.GetState();
            base.Update(gameTime);
        }

        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (currentScreen == GameScreen.Title || currentScreen == GameScreen.Settings)
            {
                GraphicsDevice.Clear(new Color(10, 10, 15));
                spriteBatch.Begin();
                titleScreen.Draw(spriteBatch, gameTime);
                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.Playing)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                metronome.Draw(spriteBatch);
                waveManager.Draw(spriteBatch, camera);

                spriteBatch.DrawString(font, "Wave: " + waveManager.CurrentWave, new Vector2(10, 35), Color.Black);
                spriteBatch.DrawString(font, "Coins: " + waveManager.CoinsEarned, new Vector2(10, 55), Color.DarkGoldenrod);

                if (waveManager.BossActive)
                {
                    int barW = 400;
                    int barX = ScreenWidth / 2 - barW / 2;
                    int barH = 16;
                    int filled = (int)(barW * ((float)waveManager.boss.health / waveManager.boss.maxHealth));
                    spriteBatch.Draw(pixel, new Rectangle(barX, 8, barW, barH), Color.DarkRed);
                    spriteBatch.Draw(pixel, new Rectangle(barX, 8, filled, barH), Color.Gold);
                    string bossLabel = "BOSS";
                    Vector2 labelSz = font.MeasureString(bossLabel);
                    spriteBatch.DrawString(font, bossLabel, new Vector2(ScreenWidth / 2f - labelSz.X / 2f, 26), new Color(160, 0, 160));
                }

                if (waveManager.bossAnnounceTimer > 0f)
                {
                    float alpha = Math.Min(1f, waveManager.bossAnnounceTimer);
                    string bossText = "BOSS INCOMING!";
                    Vector2 bossTextSz = font.MeasureString(bossText);
                    spriteBatch.DrawString(font, bossText,
                        new Vector2(ScreenWidth / 2f - bossTextSz.X / 2f, 90),
                        Color.Crimson * alpha);
                }

                Conductor.Draw(spriteBatch, camera, font);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.Paused)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                metronome.Draw(spriteBatch);
                waveManager.Draw(spriteBatch, camera);
                Conductor.Draw(spriteBatch, camera, font);

                DrawPauseOverlay(spriteBatch);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.Shop)
            {
                GraphicsDevice.Clear(ColDeep);
                spriteBatch.Begin();
                shop.Draw(spriteBatch, Conductor, waveManager);
                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.FluteBanner)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                waveManager.Draw(spriteBatch, camera);
                Conductor.Draw(spriteBatch, camera, font);

                DrawFluteBannerOverlay(spriteBatch);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.PianoBanner)
            {
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                map.Draw(spriteBatch, camera);
                waveManager.Draw(spriteBatch, camera);
                Conductor.Draw(spriteBatch, camera, font);

                DrawPianoBannerOverlay(spriteBatch);

                spriteBatch.End();
            }
            else if (currentScreen == GameScreen.GameOver)
            {
                GraphicsDevice.Clear(new Color(10, 10, 15));
                spriteBatch.Begin();

                string msg = "GAME OVER";
                Vector2 sz = font.MeasureString(msg);
                spriteBatch.DrawString(font, msg,new Vector2(ScreenWidth / 2f - sz.X / 2f, ScreenHeight / 2f - sz.Y / 2f),Color.Red);

                string sub = "Press ENTER to return to title";
                Vector2 subSz = font.MeasureString(sub);
                spriteBatch.DrawString(font, sub,new Vector2(ScreenWidth / 2f - subSz.X / 2f, ScreenHeight / 2f + 40), new Color(107, 102, 88));

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
        public void UpdatePauseInput(KeyboardState kb)
        {
            if (!oldKb.IsKeyDown(Keys.Escape) && kb.IsKeyDown(Keys.Escape))
            {
                currentScreen = GameScreen.Playing;
                MediaPlayer.Resume();
                return;
            }

            bool up = (!oldKb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.Up)) || (!oldKb.IsKeyDown(Keys.W) && kb.IsKeyDown(Keys.W));
            bool down = (!oldKb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.Down)) || (!oldKb.IsKeyDown(Keys.S) && kb.IsKeyDown(Keys.S));
            bool enter = (!oldKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter)) || (!oldKb.IsKeyDown(Keys.Space) && kb.IsKeyDown(Keys.Space));

            if (up)
            {
                pauseIndex = (pauseIndex - 1 + pauseLabels.Length) % pauseLabels.Length;
            }
            if (down)
            {
                pauseIndex = (pauseIndex + 1) % pauseLabels.Length;
            }
            if (enter)
            {
                ActivatePauseMenu();
            }

            MouseState mouse = Mouse.GetState();
            bool clicked = mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
            for (int i = 0; i < pauseLabels.Length; i++)
            {
                if (pauseRects[i] != Rectangle.Empty && pauseRects[i].Contains(mouse.X, mouse.Y))
                {
                    pauseIndex = i;
                    if (clicked)
                    {
                        ActivatePauseMenu();
                    }
                    break;
                }
            }
            oldMouse = mouse;
        }

        public void ActivatePauseMenu()
        {
            if (pauseIndex == 0)
            {
                currentScreen = GameScreen.Playing;
                MediaPlayer.Resume();
            }
            else if (pauseIndex == 1)
            {
                titleScreen.ForceScreen(GameScreen.Settings);
                currentScreen = GameScreen.Settings;
            }
            else if (pauseIndex == 2)
            {
                titleScreen.ForceScreen(GameScreen.Title);
                currentScreen = GameScreen.Title;
                Conductor.Health = Conductor.MaxHealth;
                Conductor.IsAlive = true;
                Conductor.Position = new Vector2(map.MapWidthPixels / 2f, map.MapHeightPixels / 2f);
                waveManager = new WaveManager(GraphicsDevice, Content);
                MediaPlayer.Play(titleMusic);
            }
        }



        public void DrawPauseOverlay(SpriteBatch sb)
        {
            sb.Draw(pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.Black * 0.60f);

            int panelW = 400;
            int panelH = 320;
            int panelX = ScreenWidth / 2 - panelW / 2;
            int panelY = ScreenHeight / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);

            float cx = panelX + panelW / 2f;

            string header = "PAUSED";
            Vector2 headerSz = menuFont.MeasureString(header);
            sb.DrawString(menuFont, header, new Vector2(cx - headerSz.X / 2f, panelY + 36), ColWhite);

            DrawHorizontalRule(sb, new Vector2(cx, panelY + 90), 80, 1f);

            float menuTop = panelY + 110f;
            float lineH = 56f;

            for (int i = 0; i < pauseLabels.Length; i++)
            {
                bool selected = (i == pauseIndex);

                float itemAlpha = 0.45f;
                if (selected)
                {
                    itemAlpha = 1f;
                }


                Color labelColor = ColMuted;
                if (selected)
                {
                    labelColor = ColWhite;
                }


                float scale = 1.0f;
                if (selected)
                {
                    scale = 1.05f;
                }


                string label = pauseLabels[i].ToUpper();
                Vector2 labelSz = menuFont.MeasureString(label) * scale;
                float x = cx - labelSz.X / 2f;
                float y = menuTop + i * lineH;

                pauseRects[i] = new Rectangle((int)(x - 30), (int)y, (int)labelSz.X + 60, (int)labelSz.Y + 8);

                if (selected)
                {
                    float glow = 0.6f + 0.4f * (float)Math.Sin(pauseGlowTimer * 3f);
                    float dotX = x - 22;
                    float dotY = y + labelSz.Y / 2f;
                    int dotSize = 7;
                    sb.Draw(pixel, new Rectangle((int)(dotX - dotSize / 2), (int)(dotY - dotSize / 2), dotSize, dotSize), ColGold * glow);
                }

                sb.DrawString(menuFont, label, new Vector2(x, y), labelColor * itemAlpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            if (font != null)
            {
                string hint = "ESC to resume     ENTER to select";
                Vector2 hSz = font.MeasureString(hint);
                sb.DrawString(font, hint, new Vector2(cx - hSz.X / 2f, panelY + panelH - 30), ColMuted * 0.55f);
            }
        }

        public void DrawHorizontalRule(SpriteBatch sb, Vector2 center, int halfWidth, float alpha)
        {
            int thickness = 1;
            sb.Draw(pixel, new Rectangle((int)(center.X - halfWidth), (int)center.Y, halfWidth * 2, thickness), ColGold * 0.35f * alpha);
            int ds = 5;
            sb.Draw(pixel,new Rectangle((int)center.X - ds / 2, (int)center.Y - ds / 2 + thickness / 2, ds, ds), ColGold * 0.6f * alpha);
        }

        public void DrawFluteBannerOverlay(SpriteBatch sb)
        {
            sb.Draw(pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.Black * 0.70f);

            int panelW = 560;
            int panelH = 280;
            int panelX = ScreenWidth / 2 - panelW / 2;
            int panelY = ScreenHeight / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);

            float cx = panelX + panelW / 2f;

            string header = "FLUTE ACQUIRED!";
            Vector2 headerSz = menuFont.MeasureString(header);
            sb.DrawString(menuFont, header, new Vector2(cx - headerSz.X / 2f, panelY + 36), ColGold);

            DrawHorizontalRule(sb, new Vector2(cx, panelY + 95), 100, 1f);

            string[] lines = {
                "The flute now orbits you.",
                "Land 3 PERFECT beats to unleash",
                "a ring of music note projectiles."
            };
            float lineY = panelY + 112f;
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 lsz = font.MeasureString(lines[i]);
                sb.DrawString(font, lines[i], new Vector2(cx - lsz.X / 2f, lineY + i * 26f), ColWhite * 0.90f);
            }

            string hint = "ENTER / SPACE to continue";
            Vector2 hSz = font.MeasureString(hint);
            sb.DrawString(font, hint, new Vector2(cx - hSz.X / 2f, panelY + panelH - 28), ColMuted * 0.55f);
        }

        public void DrawPianoBannerOverlay(SpriteBatch sb)
        {
            sb.Draw(pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.Black * 0.70f);

            int panelW = 560;
            int panelH = 280;
            int panelX = ScreenWidth / 2 - panelW / 2;
            int panelY = ScreenHeight / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);

            float cx = panelX + panelW / 2f;

            string header = "PIANO ACQUIRED!";
            Vector2 headerSz = menuFont.MeasureString(header);
            sb.DrawString(menuFont, header, new Vector2(cx - headerSz.X / 2f, panelY + 36), ColGold);

            DrawHorizontalRule(sb, new Vector2(cx, panelY + 95), 100, 1f);

            string[] lines = {
                "Pianos now drop from the sky",
                "onto the nearest enemy every",
                "few seconds. Hold your ground."
            };
            float lineY = panelY + 112f;
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 lsz = font.MeasureString(lines[i]);
                sb.DrawString(font, lines[i], new Vector2(cx - lsz.X / 2f, lineY + i * 26f), ColWhite * 0.90f);
            }

            string hint = "ENTER / SPACE to continue";
            Vector2 hSz = font.MeasureString(hint);
            sb.DrawString(font, hint, new Vector2(cx - hSz.X / 2f, panelY + panelH - 28), ColMuted * 0.55f);
        }
    }
}
