using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    // piano that falls from the sky, warning then big hit box
    public class PianoDrop
    {
        public Vector2 TargetPosition;
        public bool IsAlive = true;

        public const float WarningDuration = 1.5f;
        public const float ImpactDuration = 0.35f;
        public const float FlashRate = 0.12f;

        public float timer = WarningDuration;
        public bool impactPhase = false;
        public bool damageDealt = false;
        public float flashTimer = 0f;
        public bool flashOn = true;

        public static readonly Vector2 DropSize = new Vector2(80, 80);


        public PianoDrop(Vector2 targetPosition)
        {
            TargetPosition = targetPosition;
        }


        public void Update(float elapsed)
        {
            if (!IsAlive)
            {
                return;
            }

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
                {
                    IsAlive = false;
                }
            }
        }


        // damage the player for 20% of max hp when piano land on them
        public int CheckHit(Vector2 playerPos, Vector2 playerSize, int playerMaxHealth)
        {
            if (!impactPhase || damageDealt)
            {
                return 0;
            }

            Rectangle dropRect = new Rectangle((int)(TargetPosition.X - DropSize.X / 2), (int)(TargetPosition.Y - DropSize.Y / 2), (int)DropSize.X, (int)DropSize.Y);
            Rectangle playerRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, (int)playerSize.X, (int)playerSize.Y);

            if (dropRect.Intersects(playerRect))
            {
                damageDealt = true;
                return (int)(playerMaxHealth * 0.20f);
            }

            return 0;
        }


        public Rectangle GetImpactRect()
        {
            return new Rectangle((int)(TargetPosition.X - DropSize.X / 2), (int)(TargetPosition.Y - DropSize.Y / 2), (int)DropSize.X, (int)DropSize.Y);
        }


        public bool IsImpactingNow()
        {
            return impactPhase && !damageDealt;
        }


        public void MarkDamageApplied()
        {
            damageDealt = true;
        }


        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (!IsAlive)
            {
                return;
            }

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
