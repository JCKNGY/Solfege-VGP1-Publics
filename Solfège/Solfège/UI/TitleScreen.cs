using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Solfège
{
    public enum GameScreen
    {
        Title,
        Settings,
        Playing,
        Paused,
        GameOver
    }

    public class TitleScreen
    {
        public GameScreen CurrentScreen
        { 
            get; 
            private set;
        } = GameScreen.Title;

        public int musicPct = 100;
        public int sfxPct = 100;
        public int masterPct = 100;

        public float MusicVolume 
        { 
            get 
            { 
                return musicPct / 100f;
            } 
        }
        public float SfxVolume { 
            get 
            { 
                return sfxPct / 100f;
            } 
        }
        public float MasterVolume
        {
            get { 
                return masterPct / 100f;
            }
        }

        public bool ScreenShake 
        {
            get; 
            private set;
        } = true;
        
        public bool MetronomePulse 
        {
            get;
            private set; 
        } = true;

        public event Action OnStartGame;
        public event Action OnNewGame;
        public event Action OnExitGame;

        public SpriteFont titleFont;
        public SpriteFont menuFont;
        public SpriteFont uiFont;
        public Texture2D pixel;
        public Texture2D logoTexture;

        public GraphicsDevice gd;
        public int SW, SH;

        public string[] menuLabels = { "New Performance", "Continue", "Settings", "Exit" };
        public int menuIndex = 0;
        public KeyboardState prevKb = default;

        public float holdTimer = 0f;
        public float holdDelay = 0.4f;
        public float holdRepeat = 0.08f;
        public bool holdingLeft = false;
        public bool holdingRight = false;

        public MouseState prevMouse = default;

        public float fadeIn = 0f;
        public const float FadeSpeed = 1.2f;

        public float glowTimer = 0f;

        public int settingsFocus = -1;

        public Rectangle[] sliderRects = new Rectangle[3];

        public float time = 0f;

        public static readonly Color ColGold = new Color(201, 168, 76);
        public static readonly Color ColGold2 = new Color(232, 201, 122);
        public static readonly Color ColMuted = new Color(107, 102, 88);
        public static readonly Color ColWhite = new Color(232, 228, 217);
        public static readonly Color ColInk = new Color(10, 10, 15);
        public static readonly Color ColDeep = new Color(8, 8, 16);

        public TitleScreen(GraphicsDevice graphicsDevice, SpriteFont titleFont, SpriteFont menuFont, SpriteFont uiFont, ContentManager content)
        {
            gd = graphicsDevice;
            this.titleFont = titleFont;
            this.menuFont = menuFont;
            this.uiFont = uiFont;
            SW = gd.Viewport.Width;
            SH = gd.Viewport.Height;

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });

            logoTexture = content.Load<Texture2D>("sprites/Ui/solfegeTitle");
        }


        //Lock the screen so nothing start tweaking out
        public void ForceScreen(GameScreen screen)
        {
            CurrentScreen = screen;
            if (screen == GameScreen.Title)
                fadeIn = 0f;
            if (screen == GameScreen.Settings)
                settingsFocus = 0;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();
            time += dt;
            glowTimer += dt;
            fadeIn = Math.Min(1f, fadeIn + dt * FadeSpeed);

            if (CurrentScreen == GameScreen.Title)
            {
                UpdateTitleInput(kb);
            }
            else if (CurrentScreen == GameScreen.Settings)
            {
                UpdateSettingsInput(kb);
            }

            prevKb = kb;
        }

        public void UpdateTitleInput(KeyboardState kb)
        {
            bool up = JustPressed(kb, Keys.Up) || JustPressed(kb, Keys.W);
            bool down = JustPressed(kb, Keys.Down) || JustPressed(kb, Keys.S);
            bool enter = JustPressed(kb, Keys.Enter) || JustPressed(kb, Keys.Space);

            if (up) 
            {
                menuIndex = (menuIndex - 1 + menuLabels.Length) % menuLabels.Length;
            }
            if (down)
            {
                menuIndex = (menuIndex + 1) % menuLabels.Length;
            }



            if (enter)
            {
                ActivateMenu();
            }
        }


        public void ActivateMenu()
        {
            if (menuIndex == 0) // New Performance
            {
                CurrentScreen = GameScreen.Playing;
                if (OnNewGame != null)
                {
                    OnNewGame();
                }
            }
            else if (menuIndex == 1) // Continue
            {
                CurrentScreen = GameScreen.Playing;
                if (OnStartGame != null)
                {
                    OnStartGame();
                }
            }
            else if (menuIndex == 2) 
            {
                CurrentScreen = GameScreen.Settings;
                settingsFocus = -1;
            }
            else if (menuIndex == 3)
            {
                if (OnExitGame != null)
                {
                    OnExitGame();
                }
            }
        }

        public void UpdateSettingsInput(KeyboardState kb)
        {
            float dt = (float)(1.0 / 60.0);

            if (JustPressed(kb, Keys.Down) || JustPressed(kb, Keys.S))
            {
                settingsFocus = Math.Min(settingsFocus + 1, 4);
            }
            if (JustPressed(kb, Keys.Up) || JustPressed(kb, Keys.W))
            {
                settingsFocus = Math.Max(settingsFocus - 1, 0);
            }


            bool leftDown = kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A);
            bool rightDown = kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D);

            bool stepLeft = false;
            bool stepRight = false;

            if (leftDown || rightDown)
            {

                if ((leftDown && !holdingLeft) || (rightDown && !holdingRight))
                {
                    holdTimer = 0f;
                    holdingLeft = leftDown;
                    holdingRight = rightDown;
                    stepLeft = leftDown;
                    stepRight = rightDown;
                }
                else
                {
                    holdTimer += dt;
                    //If statement basically: like if holdTimer < holdDelay is true then threshold = holdDelay else it is holdRepeat
                    float threshold = holdTimer < holdDelay ? holdDelay : holdRepeat;
                    if (holdTimer >= threshold)
                    {
                        holdTimer -= holdRepeat;
                        stepLeft = leftDown;
                        stepRight = rightDown;
                    }
                }
            }
            else
            {
                holdingLeft = false;
                holdingRight = false;
                holdTimer = 0f;
            }

            if (stepLeft || stepRight)
            {
                int delta = stepRight ? 5 : -5;
                if (settingsFocus == 0)
                {
                    musicPct = Math.Max(0, Math.Min(100, musicPct + delta));
                    ApplyVolumes();
                }
                else if (settingsFocus == 1)
                {
                    sfxPct = Math.Max(0, Math.Min(100, sfxPct + delta));
                }
                else if (settingsFocus == 2)
                {
                    masterPct = Math.Max(0, Math.Min(100, masterPct + delta));
                    ApplyVolumes();
                }
                else if (settingsFocus == 3)
                {
                    ScreenShake = !ScreenShake;
                }
                else if (settingsFocus == 4)
                {
                    MetronomePulse = !MetronomePulse;
                }
            }

            MouseState mouse = Mouse.GetState();

            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;

            if (mouseClicked)
            {
                int sliderX = SW / 2 - 240 + 130;
                int sliderW = 280;

                for (int i = 0; i < 3; i++)
                {
                    if (sliderRects[i] != Rectangle.Empty)
                    {
                        Rectangle hit = new Rectangle(sliderRects[i].X, sliderRects[i].Y - 10, sliderRects[i].Width, sliderRects[i].Height + 20);
                        if (hit.Contains(mouse.X, mouse.Y))
                        {
                            float ratio = MathHelper.Clamp((float)(mouse.X - sliderX) / sliderW, 0f, 1f);
                            int pct = (int)Math.Round(ratio * 20f) * 5;
                            pct = Math.Max(0, Math.Min(100, pct));
                            settingsFocus = i;
                            if (i == 0) 
                            { 
                                musicPct = pct; ApplyVolumes();
                            }
                            else if (i == 1) 
                            { 
                                sfxPct = pct;
                            }
                            else if (i == 2) 
                            { 
                                masterPct = pct;
                                ApplyVolumes();
                            }
                        }
                    }
                }
            }
            prevMouse = mouse;
            if (JustPressed(kb, Keys.Enter) || JustPressed(kb, Keys.Space))
            {
                if (settingsFocus == 3) {
                    ScreenShake = !ScreenShake;
                }
                if (settingsFocus == 4)
                {
                    MetronomePulse = !MetronomePulse;
                }

                }

                if (JustPressed(kb, Keys.Escape))
            {
                CurrentScreen = GameScreen.Title;
            }
            }



            public void ApplyVolumes()
            {
                MediaPlayer.Volume = MusicVolume * MasterVolume;
                SoundEffect.MasterVolume = SfxVolume * MasterVolume;
            }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(pixel, new Rectangle(0, 0, SW, SH), ColInk);

            if (CurrentScreen == GameScreen.Title)
            {
                DrawTitle(sb);
            }
            else if (CurrentScreen == GameScreen.Settings)
            {
                DrawSettings(sb);
            }
        }
        public void DrawTitle(SpriteBatch sb)
        {
            float f = fadeIn;

            if (logoTexture != null)
            {
                int logoW = 420;
                int logoH = (int)(logoTexture.Height * (420f / logoTexture.Width));
                int logoX = SW / 2 - logoW / 2;
                int logoY = (int)(SH * 0.10f);
                sb.Draw(logoTexture, new Rectangle(logoX, logoY, logoW, logoH), Color.White * f);
            }

            DrawHorizontalRule(sb, new Vector2(SW / 2f, SH * 0.50f), 120, f);

            float menuTop = SH * 0.54f;
            float lineH = 52f;

            for (int i = 0; i < menuLabels.Length; i++)
            {
                bool selected = (i == menuIndex);

                float itemAlpha = 0.45f;
                if (selected)
                {
                    itemAlpha = 1f;
                }
                itemAlpha *= f;

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

                string label = menuLabels[i].ToUpper();
                Vector2 labelSz = menuFont.MeasureString(label) * scale;
                float x = SW / 2f - labelSz.X / 2f;
                float y = menuTop + i * lineH;

                if (selected)
                {
                    float glow = 0.6f + 0.4f * (float)Math.Sin(glowTimer * 3f);
                    float dotX = x - 22;
                    float dotY = y + labelSz.Y / 2f;
                    int dotSize = 7;
                    sb.Draw(pixel, new Rectangle((int)(dotX - dotSize / 2), (int)(dotY - dotSize / 2), dotSize, dotSize), ColGold * glow * f);
                }

                sb.DrawString(menuFont, label, new Vector2(x, y), labelColor * itemAlpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            if (uiFont != null)
            {
                Vector2 hSz = uiFont.MeasureString("W / S  or  UP / DOWN  to navigate     ENTER to select");
                sb.DrawString(uiFont, "W / S  or  UP / DOWN  to navigate     ENTER to select", new Vector2(SW / 2f - hSz.X / 2f, SH - 32), ColMuted * 0.4f * f);
            }
        }

        public void DrawSettings(SpriteBatch sb)
        {
            sb.Draw(pixel, new Rectangle(0, 0, SW, SH), Color.Black * 0.65f);

            int panelW = 480, panelH = 460;
            int panelX = SW / 2 - panelW / 2;
            int panelY = SH / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColDeep);

            DrawBorder(sb, new Rectangle(panelX, panelY, panelW, panelH), ColGold * 0.35f, 1);

            float cx = panelX + panelW / 2f;
            float cy = panelY + 40;

            if (menuFont != null)
            {
                string st = "SETTINGS";
                Vector2 stSz = menuFont.MeasureString(st);
                sb.DrawString(menuFont, st, new Vector2(cx - stSz.X / 2f, cy), ColWhite);
            }
            cy += 55;

            DrawSectionLabel(sb, "AUDIO", panelX + 30, (int)cy);
            cy += 28;

            DrawSliderRow(sb, "Music", MusicVolume, 0, panelX, panelY, panelW, (int)cy, settingsFocus == 0); cy += 46;
            DrawSliderRow(sb, "SFX", SfxVolume, 1, panelX, panelY, panelW, (int)cy, settingsFocus == 1); cy += 46;
            DrawSliderRow(sb, "Master", MasterVolume, 2, panelX, panelY, panelW, (int)cy, settingsFocus == 2); cy += 54;

            DrawSectionLabel(sb, "DISPLAY", panelX + 30, (int)cy);
            cy += 28;

            //DrawToggleRow(sb, "Screen Shake", ScreenShake, panelX, panelW, (int)cy, settingsFocus == 3); cy += 38;
            //DrawToggleRow(sb, "Metronome Pulse", MetronomePulse, panelX, panelW, (int)cy, settingsFocus == 4); cy += 46;

            if (uiFont != null)
            {
                Vector2 hSz = uiFont.MeasureString("ESC  to go back     ENTER  to toggle");
                sb.DrawString(uiFont, "ESC  to go back     ENTER  to toggle", new Vector2(cx - hSz.X / 2f, panelY + panelH - 32), ColMuted * 0.55f);
            }
        }

        public void DrawSliderRow(SpriteBatch sb, string label, float value, int focusId, int panelX, int panelY, int panelW, int y, bool focused)
        {
            int labelX = panelX + 30;
            int sliderX = panelX + 130;
            int sliderW = panelW - 200;
            int valX = panelX + panelW - 55;

            if (focusId >= 0 && focusId < sliderRects.Length)
            {
                sliderRects[focusId] = new Rectangle(sliderX, y, sliderW, 12);
            }


            Color rowColor = ColMuted;
            if (focused)
            {
                rowColor = ColWhite;
            }


            if (uiFont != null)
            {
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(labelX, y), rowColor);
            }

            sb.Draw(pixel, new Rectangle(sliderX, y + 8, sliderW, 2), Color.White * 0.1f);

            int fillW = (int)(sliderW * value);

            Color fillColor = ColGold * 0.7f;
            if (focused) 
            {
                fillColor = ColGold2;
            }
            sb.Draw(pixel, new Rectangle(sliderX, y + 8, fillW, 2), fillColor);


            int thumbX = sliderX + fillW - 5;
            int thumbY = y + 2;
            float glow = 0.5f;
            if (focused) 
            {
                glow = 0.7f + 0.3f * (float)Math.Sin(glowTimer * 4f);
            }

            Color thumbColor = ColGold * 0.5f;
            if (focused)
            {
                thumbColor = ColGold2 * glow;
            }
            sb.Draw(pixel, new Rectangle(thumbX, thumbY, 10, 12), thumbColor);


            if (uiFont != null)
            {
                sb.DrawString(uiFont, ((int)(value * 100)).ToString(), new Vector2(valX, y), rowColor);
            }
                
        }

        public void DrawToggleRow(SpriteBatch sb, string label, bool value, int panelX, int panelW, int y, bool focused)
        {
            Color rowColor = ColMuted;
            if (focused)
            {
                rowColor = ColWhite;
            }

                if (uiFont != null)
                {
                sb.DrawString(uiFont, label.ToUpper(), new Vector2(panelX + 30, y), rowColor);
                }

            int tx = panelX + panelW - 70;

            Color trackColor = Color.White * 0.08f;
            if (value)
            {
                trackColor = ColGold * 0.3f;
            }
            sb.Draw(pixel, new Rectangle(tx, y, 44, 20), trackColor);

            Color borderColor = ColGold * 0.25f;
            if (focused) borderColor = ColGold * 0.8f;
            DrawBorder(sb, new Rectangle(tx, y, 44, 20), borderColor, 1);

            int knobX = tx + 2;
            if (value)
            {
                knobX = tx + 44 - 18;
            }
                

            Color knobColor = ColMuted * 0.6f;
            if (value) knobColor = ColGold2;
            sb.Draw(pixel, new Rectangle(knobX, y + 3, 14, 14), knobColor);
        }

        public void DrawSectionLabel(SpriteBatch sb, string text, int x, int y)
        {
            if (uiFont == null) 
            {
                return;
            }


            sb.DrawString(uiFont, text, new Vector2(x, y), ColGold * 0.75f);
            sb.Draw(pixel, new Rectangle(x + (int)uiFont.MeasureString(text).X + 10, y + 8, 300, 1), ColGold * 0.2f);
        }

        public void DrawHorizontalRule(SpriteBatch sb, Vector2 center, int halfWidth, float alpha)
        {
            sb.Draw(pixel, new Rectangle((int)(center.X - halfWidth), (int)center.Y, halfWidth * 2, 1),ColGold * 0.5f * alpha);

            sb.Draw(pixel, new Rectangle((int)center.X - 5 / 2, (int)center.Y - 5 / 2 + 1 / 2, 5, 5), ColGold * 0.5f * alpha);
        }

        public void DrawBorder(SpriteBatch sb, Rectangle r, Color c, int thickness)
        {
            sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
            sb.Draw(pixel, new Rectangle(r.X, r.Bottom, r.Width, thickness), c);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
            sb.Draw(pixel, new Rectangle(r.Right, r.Y, thickness, r.Height), c);
        }

        public bool JustPressed(KeyboardState kb, Keys key)
        {
            return kb.IsKeyDown(key) && !prevKb.IsKeyDown(key);
        }
    }
}
