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
        Texture2D heartFullTexture;
        Texture2D heartNearFullTexture;
        Texture2D heartHalfwayTexture;
        Texture2D heartNearGoneTexture;
        Texture2D heartGoneTexture;
        Texture2D currentTexture;

        Rectangle heartRect;
        Rectangle sourceRect;
        int ogSize = 128;
        int newSize = 178;

        SpriteFont font;
        SoundEffect heartbeatSfx;

        public double BPM = 120;
        double SPB;
        double beatTimer;

        const double PerfectWindow = 0.150;
        const double GoodWindow = 0.250;

        public int Streak { 
            get;
            private set;
        } = 0;
        public int BestStreak 
        { 
            get; 
            private set; 
        } = 0;
        public int ComboMultiplier 
        {
            get;
            private set;
        } = 1;

        public int ConsecutivePerfects { 
            get; 
            private set; 
        } = 0;

        public BeatRating LastRating {
            get;
            private set;
        } = BeatRating.None;
        float ratingTimer = 0f;

        public bool HasAttackedThisCycle { get; private set; } = false;
        const float RatingDuration = 0.65f;

        Texture2D pixel;
        Rectangle barBg;
        float beatPulse;

        static readonly Color PerfectColor = new Color(255, 215, 0);
        static readonly Color GoodColor = new Color(100, 220, 100);
        static readonly Color MissColor = new Color(230, 60, 60);

        int screenW, screenH;

        // make the metronome
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

        // check how well player hit the beat
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


        // how much extra damage based on the rating
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

        // tick the beat and play the heart sound
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

        // draw the beat ui stuff
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBeatBar(spriteBatch);
            DrawHeart(spriteBatch);
            DrawStreakUI(spriteBatch);
            DrawRatingPopup(spriteBatch);
        }

        // update the streak and combo when player hit
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

        // draw the beat bar with timing windows
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

        // draw the heart for player hp
        public void DrawHeart(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(currentTexture, heartRect, sourceRect, Color.White, 0f, new Vector2(32f, 32f), SpriteEffects.None, 0f);
        }

        // draw the streak and combo text
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

        // draw the rating popup like PERFECT or MISS
        private void DrawRatingPopup(SpriteBatch spriteBatch)
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

        // reset all the streak stuff
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
