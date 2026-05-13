using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solfège
{
    public class ShopItem
    {
        public string Name;
        public string Description;
        public int Cost;
        public Action<Conductor> Apply;

        // make a shop item
        public ShopItem(string name, string description, int cost, Action<Conductor> apply)
        {
            Name = name;
            Description = description;
            Cost = cost;
            Apply = apply;
        }
    }

    public class Shop
    {
        private List<ShopItem> items;
        private int selectedIndex = 0;

        private SpriteFont font;
        private Texture2D pixel;

        private float feedbackTimer = 0f;
        private string feedbackMsg = "";
        private bool feedbackGood = false;
        private const float FeedbackDuration = 1.2f;

        private float glowTimer = 0f;

        private int screenW, screenH;

        private Rectangle[] itemRects = new Rectangle[3];
        private Rectangle leaveRect = Rectangle.Empty;
        private MouseState prevMouse;

        static readonly Color ColDeep  = new Color(8, 8, 16);
        static readonly Color ColPanel = new Color(18, 18, 30);
        static readonly Color ColWhite = new Color(232, 228, 217);
        static readonly Color ColGold  = new Color(201, 168, 76);
        static readonly Color ColGold2 = new Color(255, 215, 0);
        static readonly Color ColMuted = new Color(107, 102, 88);
        static readonly Color ColGreen = new Color(100, 220, 100);
        static readonly Color ColRed   = new Color(230, 60, 60);

        // set up the shop and make items
        public Shop(SpriteFont font, Texture2D pixel, int screenW, int screenH)
        {
            this.font    = font;
            this.pixel   = pixel;
            this.screenW = screenW;
            this.screenH = screenH;

            items = new List<ShopItem>
            {
                new ShopItem(
                    "HP UPGRADE",
                    "+20 Max HP. Current HP unchanged.",
                    8,
                    c => { c.MaxHealth += 20; }
                ),
                new ShopItem(
                    "ATK UPGRADE",
                    "+5 Attack Damage per hit.",
                    10,
                    c => { c.BaseDamage += 5; }
                ),
                new ShopItem(
                    "HEALTH POTION",
                    "Restore 30 HP instantly.",
                    5,
                    c => { c.Health = Math.Min(c.Health + 30, c.MaxHealth); }
                ),
            };
        }

        // reset the shop when entering
        public void OnEnter()
        {
            selectedIndex = 0;
            feedbackTimer = 0f;
            prevMouse = Mouse.GetState();
        }

        // handle keyboard and mouse for shop
        public void Update(GameTime gameTime, KeyboardState kb, KeyboardState prevKb, WaveManager waveManager, Conductor conductor, ref GameScreen screen)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            feedbackTimer -= elapsed;
            glowTimer += elapsed;

            bool up    = (!prevKb.IsKeyDown(Keys.Up)    && kb.IsKeyDown(Keys.Up))
                      || (!prevKb.IsKeyDown(Keys.W)     && kb.IsKeyDown(Keys.W));
            bool down  = (!prevKb.IsKeyDown(Keys.Down)  && kb.IsKeyDown(Keys.Down))
                      || (!prevKb.IsKeyDown(Keys.S)     && kb.IsKeyDown(Keys.S));
            bool buy   = (!prevKb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.Enter));
            bool leave = (!prevKb.IsKeyDown(Keys.E)     && kb.IsKeyDown(Keys.E))
                      || (!prevKb.IsKeyDown(Keys.Tab)   && kb.IsKeyDown(Keys.Tab));

            if (up)   selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
            if (down) selectedIndex = (selectedIndex + 1) % items.Count;

            MouseState mouse = Mouse.GetState();
            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;
            int hoverIndex = -1;
            for (int i = 0; i < items.Count && i < itemRects.Length; i++)
            {
                if (itemRects[i] != Rectangle.Empty && itemRects[i].Contains(mouse.X, mouse.Y))
                {
                    hoverIndex = i;
                    break;
                }
            }
            if (hoverIndex >= 0)
            {
                selectedIndex = hoverIndex;
                if (mouseClicked)
                {
                    buy = true;
                }
            }
            if (mouseClicked && leaveRect != Rectangle.Empty && leaveRect.Contains(mouse.X, mouse.Y))
            {
                leave = true;
            }
            prevMouse = mouse;

            if (buy)
            {
                ShopItem item = items[selectedIndex];
                if (waveManager.CoinsEarned >= item.Cost)
                {
                    waveManager.SpendCoins(item.Cost);
                    item.Apply(conductor);
                    feedbackMsg  = "Purchased!";
                    feedbackGood = true;
                    feedbackTimer = FeedbackDuration;
                }
                else
                {
                    feedbackMsg  = "Not enough coins!";
                    feedbackGood = false;
                    feedbackTimer = FeedbackDuration;
                }
            }

            if (leave)
            {
                screen = GameScreen.Playing;
                waveManager.StartNextWave(conductor.Position);
            }
        }

        // draw the shop panel
        public void Draw(SpriteBatch sb, Conductor conductor, WaveManager waveManager)
        {
            sb.Draw(pixel, new Rectangle(0, 0, screenW, screenH), ColDeep);

            int panelW = 640;
            int panelH = 500;
            int panelX = screenW / 2 - panelW / 2;
            int panelY = screenH / 2 - panelH / 2;

            sb.Draw(pixel, new Rectangle(panelX, panelY, panelW, panelH), ColPanel);

            DrawBorder(sb, panelX, panelY, panelW, panelH, ColGold * 0.55f);

            float cx = panelX + panelW / 2f;

            // Header
            string header = "SHOP";
            Vector2 hsz = font.MeasureString(header);
            sb.DrawString(font, header, new Vector2(cx - hsz.X / 2f, panelY + 22), ColGold);

            // Wave label
            string waveLbl = "After Wave " + waveManager.CurrentWave;
            Vector2 wlsz = font.MeasureString(waveLbl);
            sb.DrawString(font, waveLbl, new Vector2(cx - wlsz.X / 2f, panelY + 50), ColMuted * 0.7f);

            DrawRule(sb, panelX + 30, panelY + 74, panelW - 60);

            // Player stats bar
            string coinStr = "Coins: " + waveManager.CoinsEarned;
            string hpStr   = "HP: " + conductor.Health + " / " + conductor.MaxHealth;
            string atkStr  = "ATK: " + conductor.BaseDamage;
            sb.DrawString(font, coinStr, new Vector2(panelX + 24, panelY + 84), ColGold2);
            sb.DrawString(font, hpStr,   new Vector2(panelX + 200, panelY + 84), ColGreen);
            sb.DrawString(font, atkStr,  new Vector2(panelX + 420, panelY + 84), ColWhite * 0.8f);

            DrawRule(sb, panelX + 30, panelY + 108, panelW - 60);

            // Items
            float itemTop = panelY + 120f;
            float itemH   = 88f;

            for (int i = 0; i < items.Count; i++)
            {
                bool selected = (i == selectedIndex);
                ShopItem item = items[i];

                int rowY  = (int)(itemTop + i * itemH);
                int rowH  = (int)itemH - 8;
                bool canAfford = waveManager.CoinsEarned >= item.Cost;

                if (i < itemRects.Length)
                {
                    itemRects[i] = new Rectangle(panelX + 10, rowY - 2, panelW - 20, rowH);
                }

                if (selected)
                {
                    sb.Draw(pixel, new Rectangle(panelX + 10, rowY - 2, panelW - 20, rowH), ColGold * 0.10f);
                    sb.Draw(pixel, new Rectangle(panelX + 10, rowY - 2, 3, rowH), ColGold * 0.9f);
                }

                Color nameColor = selected ? ColWhite   : ColMuted;
                Color descColor = selected ? ColMuted    : ColMuted * 0.55f;
                Color costColor = !canAfford ? ColRed * 0.8f : (selected ? ColGold2 : ColGold * 0.7f);

                sb.DrawString(font, item.Name,        new Vector2(panelX + 26, rowY + 4),  nameColor);
                sb.DrawString(font, item.Description, new Vector2(panelX + 26, rowY + 28), descColor);

                string costStr = item.Cost + " coins";
                Vector2 csz   = font.MeasureString(costStr);
                sb.DrawString(font, costStr, new Vector2(panelX + panelW - 28 - csz.X, rowY + 14), costColor);

                if (i < items.Count - 1)
                {
                    DrawRule(sb, panelX + 30, rowY + rowH + 4, panelW - 60, 0.18f);
                }
            }

            DrawRule(sb, panelX + 30, panelY + panelH - 72, panelW - 60);

            // Feedback
            if (feedbackTimer > 0f)
            {
                float alpha = Math.Min(1f, feedbackTimer / FeedbackDuration);
                Color fc  = feedbackGood ? ColGreen : ColRed;
                Vector2 fsz = font.MeasureString(feedbackMsg);
                sb.DrawString(font, feedbackMsg, new Vector2(cx - fsz.X / 2f, panelY + panelH - 62), fc * alpha);
            }

            // leave button that player can click
            string leaveLabel = "LEAVE SHOP";
            Vector2 leaveSz = font.MeasureString(leaveLabel);
            int btnW = (int)leaveSz.X + 40;
            int btnH = (int)leaveSz.Y + 12;
            int btnX = (int)cx - btnW / 2;
            int btnY = panelY + panelH - 50;
            leaveRect = new Rectangle(btnX, btnY, btnW, btnH);
            sb.Draw(pixel, leaveRect, ColGold * 0.15f);
            DrawBorder(sb, btnX, btnY, btnW, btnH, ColGold * 0.6f);
            sb.DrawString(font, leaveLabel, new Vector2(btnX + (btnW - leaveSz.X) / 2f, btnY + (btnH - leaveSz.Y) / 2f), ColWhite);
        }

        // draw a border around the panel
        private void DrawBorder(SpriteBatch sb, int x, int y, int w, int h, Color c)
        {
            sb.Draw(pixel, new Rectangle(x,         y,         w, 2), c);
            sb.Draw(pixel, new Rectangle(x,         y + h - 2, w, 2), c);
            sb.Draw(pixel, new Rectangle(x,         y,         2, h), c);
            sb.Draw(pixel, new Rectangle(x + w - 2, y,         2, h), c);
        }

        // draw a divider line
        private void DrawRule(SpriteBatch sb, int x, int y, int w, float alpha = 0.30f)
        {
            sb.Draw(pixel, new Rectangle(x, y, w, 1), ColGold * alpha);
        }
    }
}
