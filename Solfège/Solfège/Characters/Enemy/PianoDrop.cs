using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class PianoDrop
    {
        public Vector2 TargetPosition;
        public bool IsAlive = true;

        const float WarningDuration = 1.5f;
        const float ImpactDuration = 0.35f;
        const float FlashRate = 0.12f;

        float timer = WarningDuration;
        bool impactPhase = false;
        bool damageDealt = false;
        float flashTimer = 0f;
        bool flashOn = true;

        static readonly Vector2 DropSize = new Vector2(80, 80);

        // make the piano drop
        public PianoDrop(Vector2 targetPosition)
        {
            TargetPosition = targetPosition;
        }

        // warning phase and then it hit
        public void Update(float elapsed)
        {
            if (!IsAlive) return;

            timer -= elapsed;

            if (!impactPhase)
            {
                flashTimer -= elapsed;
                if (flashTimer <= 0f)
                {
                    flashTimer = FlashRate;
                    flashOn = !flashOn;
                }

                if (timer <= 0f)
                {
                    impactPhase = true;
                    timer = ImpactDuration;
                    flashOn = true;
                }
            }
            else
            {
                if (timer <= 0f)
                    IsAlive = false;
            }
        }

        // deal damage if player is in the box
        public int CheckHit(Vector2 playerPos, Vector2 playerSize, int playerMaxHealth)
        {
            if (!impactPhase || damageDealt) return 0;

            Rectangle dropRect   = new Rectangle((int)(TargetPosition.X - DropSize.X / 2), (int)(TargetPosition.Y - DropSize.Y / 2), (int)DropSize.X, (int)DropSize.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (dropRect.Intersects(playerRect))
            {
                damageDealt = true;
                return (int)(playerMaxHealth * 0.20f);
            }
            return 0;
        }

        // get where the piano land
        public Rectangle GetImpactRect()
        {
            return new Rectangle((int)(TargetPosition.X - DropSize.X / 2), (int)(TargetPosition.Y - DropSize.Y / 2), (int)DropSize.X, (int)DropSize.Y);
        }

        // check if the impact is happening right now
        public bool IsImpactingNow()
        {
            return impactPhase && !damageDealt;
        }

        // mark that damage was already done
        public void MarkDamageApplied()
        {
            damageDealt = true;
        }

        // draw the warning or the impact box
        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (!IsAlive) return;

            Vector2 screenPos = TargetPosition - camera.Position;
            int x = (int)(screenPos.X - DropSize.X / 2);
            int y = (int)(screenPos.Y - DropSize.Y / 2);

            if (impactPhase)
            {
                spriteBatch.Draw(pixel, new Rectangle(x, y, (int)DropSize.X, (int)DropSize.Y), new Color(101, 67, 33));
            }
            else if (flashOn)
            {
                float fade = 1f - (timer / WarningDuration);
                Color warningColor = Color.Lerp(Color.Yellow, Color.OrangeRed, fade);
                spriteBatch.Draw(pixel, new Rectangle(x, y, (int)DropSize.X, (int)DropSize.Y), warningColor * 0.75f);
            }
        }
    }
}
