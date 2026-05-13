using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public enum BeatRating
    {
        None,
        Perfect,
        Good,
        Miss
    }


    public class MetronomeSystem
    {
        public Texture2D heartFullTexture;
        public Texture2D heartNearFullTexture;
        public Texture2D heartHalfwayTexture;
        public Texture2D heartNearGoneTexture;
        public Texture2D heartGoneTexture;
        public Texture2D currentTexture;

        public Rectangle heartRect;
        public Rectangle sourceRect;
        public int ogSize = 128;
        public int newSize = 178;

        public SpriteFont font;
        public SoundEffect heartbeatSfx;

        public double BPM = 120;
        public double SPB;
        public double beatTimer;

        public const double PerfectWindow = 0.150;
        public const double GoodWindow = 0.250;

        public int Streak = 0;
        public int BestStreak = 0;
        public int ComboMultiplier = 1;
        public int ConsecutivePerfects = 0;

        public BeatRating LastRating = BeatRating.None;
        public float ratingTimer = 0f;

        public bool HasAttackedThisCycle = false;
        public const float RatingDuration = 0.65f;

        public Texture2D pixel;
        public Rectangle barBg;
        public float beatPulse;

        public static readonly Color PerfectColor = new Color(255, 215, 0);
        public static readonly Color GoodColor = new Color(100, 220, 100);
        public static readonly Color MissColor = new Color(230, 60, 60);

        public int screenW;
        public int screenH;


        public MetronomeSystem(ContentManager content, GraphicsDevice gd)
        {
            screenW = gd.Viewport.Width;
            screenH = gd.Viewport.Height;

            heartFullTexture = content.Load<Texture2D>("sprites/Ui/HeartFull");
            heartNearFullTexture = content.Load<Texture2D>("sprites/Ui/Heart75");
            heartHalfwayTexture = content.Load<Texture2D>("sprites/Ui/Heart50");
            heartNearGoneTexture = content.Load<Texture2D>("sprites/Ui/Heart25");
            heartGoneTexture = content.Load<Texture2D>("sprites/Ui/HeartEmpty");
            currentTexture = heartFullTexture;

            font = content.Load<SpriteFont>("Font");
            heartbeatSfx = content.Load<SoundEffect>("HeartBeat");

            heartRect = new Rectangle(screenW / 2, screenH - 80, ogSize, ogSize);
            sourceRect = new Rectangle(0, 0, 64, 64);

            int barW = 420;
            int barH = 22;
            barBg = new Rectangle(screenW / 2 - barW / 2, screenH - 140, barW, barH);

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });

            SPB = 60.0 / BPM;
            beatTimer = 0;
        }


        // rates the press as perfect, good, or miss based on distance from beat center
        public BeatRating RegisterAction()
        {
            HasAttackedThisCycle = true;

            double halfSPB = SPB / 2.0;
            double distToMiddle = Math.Abs(beatTimer - halfSPB);

            if (distToMiddle <= PerfectWindow)
            {
                ApplyRating(BeatRating.Perfect);
                return BeatRating.Perfect;
            }
            else if (distToMiddle <= GoodWindow)
            {
                ApplyRating(BeatRating.Good);
                return BeatRating.Good;
            }
            else
            {
                ApplyRating(BeatRating.Miss);
                return BeatRating.Miss;
            }
        }


        public float GetDamageMultiplier(BeatRating rating)
        {
            float baseMulti = 1.0f;

            if (rating == BeatRating.Perfect)
            {
                baseMulti = 1.5f;
            }
            else if (rating == BeatRating.Good)
            {
                baseMulti = 1.0f;
            }
            else
            {
                baseMulti = 0.5f;
            }

            return baseMulti * ComboMultiplier;
        }


        public void Update(GameTime gameTime, Conductor player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SPB = 60.0 / BPM;
            beatTimer += elapsed;

            if (beatTimer >= SPB && player.IsAlive)
            {
                heartbeatSfx.Play();
                beatPulse = 1f;
                beatTimer -= SPB;
                heartRect.Width = newSize;
                heartRect.Height = newSize;
                HasAttackedThisCycle = false;
            }
            else
            {
                heartRect.Width = ogSize;
                heartRect.Height = ogSize;
            }

            beatPulse = Math.Max(0f, beatPulse - elapsed * 5f);

            if (ratingTimer > 0f)
            {
                ratingTimer -= elapsed;
            }

            if (player.Health >= 100)
            {
                currentTexture = heartFullTexture;
            }
            else if (player.Health > 75)
            {
                currentTexture = heartNearFullTexture;
            }
            else if (player.Health > 50)
            {
                currentTexture = heartHalfwayTexture;
            }
            else if (player.Health > 0)
            {
                currentTexture = heartNearGoneTexture;
            }
            else
            {
                currentTexture = heartGoneTexture;
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBeatBar(spriteBatch);
            DrawHeart(spriteBatch);
            DrawStreakUI(spriteBatch);
            DrawRatingPopup(spriteBatch);
        }


        // updates the streak, combo multiplier go up at 5/10/20 streak
        public void ApplyRating(BeatRating rating)
        {
            LastRating = rating;
            ratingTimer = RatingDuration;

            if (rating == BeatRating.Miss)
            {
                Streak = 0;
                ComboMultiplier = 1;
                ConsecutivePerfects = 0;
            }
            else
            {
                Streak++;

                if (rating == BeatRating.Perfect)
                {
                    ConsecutivePerfects++;
                }
                else
                {
                    ConsecutivePerfects = 0;
                }

                if (Streak > BestStreak)
                {
                    BestStreak = Streak;
                }

                if (Streak >= 20)
                {
                    ComboMultiplier = 4;
                }
                else if (Streak >= 10)
                {
                    ComboMultiplier = 3;
                }
                else if (Streak >= 5)
                {
                    ComboMultiplier = 2;
                }
                else
                {
                    ComboMultiplier = 1;
                }
            }
        }


        public void DrawBeatBar(SpriteBatch spriteBatch)
        {
            int bx = barBg.X;
            int by = barBg.Y;
            int bw = barBg.Width;
            int bh = barBg.Height;

            spriteBatch.Draw(pixel, barBg, Color.Black * 0.65f);

            int centerX = bx + bw / 2;
            int perfectPx = (int)(PerfectWindow / SPB * bw);
            int goodPx = (int)(GoodWindow / SPB * bw);

            spriteBatch.Draw(pixel, new Rectangle(centerX - goodPx, by, goodPx * 2, bh), GoodColor * 0.30f);
            spriteBatch.Draw(pixel, new Rectangle(centerX - perfectPx, by, perfectPx * 2, bh), PerfectColor * 0.45f);

            float phase = (float)(beatTimer / SPB);
            int cursorX = bx + (int)(phase * bw);
            float brightness = 0.70f + beatPulse * 0.30f;
            spriteBatch.Draw(pixel, new Rectangle(cursorX - 2, by - 3, 4, bh + 6), Color.White * brightness);

            spriteBatch.Draw(pixel, new Rectangle(bx, by, bw, 2), Color.White * 0.80f);
            spriteBatch.Draw(pixel, new Rectangle(bx, by + bh - 2, bw, 2), Color.White * 0.80f);
            spriteBatch.Draw(pixel, new Rectangle(bx, by, 2, bh), Color.White * 0.80f);
            spriteBatch.Draw(pixel, new Rectangle(bx + bw - 2, by, 2, bh), Color.White * 0.80f);
        }


        public void DrawHeart(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(currentTexture, heartRect, sourceRect, Color.White, 0f, new Vector2(32f, 32f), SpriteEffects.None, 0f);
        }


        public void DrawStreakUI(SpriteBatch spriteBatch)
        {
            if (Streak <= 0)
            {
                return;
            }

            if (ComboMultiplier > 1)
            {
                string comboText = $"x{ComboMultiplier} COMBO";
                Vector2 csz = font.MeasureString(comboText);
                spriteBatch.DrawString(font, comboText, new Vector2(screenW / 2f - csz.X / 2f, barBg.Y - 36), PerfectColor);
            }

            string streakText = $"Streak  {Streak}";
            Vector2 ssz = font.MeasureString(streakText);
            Color streakColor = Color.White * 0.85f;

            if (ComboMultiplier >= 4)
            {
                streakColor = PerfectColor;
            }
            else if (ComboMultiplier >= 2)
            {
                streakColor = GoodColor;
            }

            spriteBatch.DrawString(font, streakText, new Vector2(screenW / 2f - ssz.X / 2f, barBg.Y - 18), streakColor);
        }


        public void DrawRatingPopup(SpriteBatch spriteBatch)
        {
            if (ratingTimer <= 0f || LastRating == BeatRating.None)
            {
                return;
            }

            string text = "MISS";
            Color color = MissColor;

            if (LastRating == BeatRating.Perfect)
            {
                text = "PERFECT!";
                color = PerfectColor;
            }
            else if (LastRating == BeatRating.Good)
            {
                text = "GOOD";
                color = GoodColor;
            }

            float alpha = ratingTimer / RatingDuration;
            float offsetY = (1f - alpha) * -20f;

            Vector2 sz = font.MeasureString(text);
            spriteBatch.DrawString(font, text, new Vector2(screenW / 2f - sz.X / 2f, barBg.Y - 60 + offsetY), color * alpha);
        }


        public void ResetStreak()
        {
            Streak = 0;
            BestStreak = 0;
            ComboMultiplier = 1;
            ConsecutivePerfects = 0;
            LastRating = BeatRating.None;
        }
    }
}
