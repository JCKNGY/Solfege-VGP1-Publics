using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class DroppedCoin
    {
        public bool Collected = false;
        public bool Expired = false;
        public int Value;

        public Vector2 Position;

        public float lifetime = 10f;
        public float timeAlive = 0f;
        public const float PickupRadius = 60f;
        public const float AttractRadius = 90f;
        public const float AttractSpeed = 300f;


        public DroppedCoin(Vector2 pos, int value)
        {
            Position = pos;
            Value = value;
        }


        // coin floats to the player when nearby, expires if not picked up
        public void Update(float elapsed, Vector2 playerPos)
        {
            timeAlive += elapsed;

            if (timeAlive >= lifetime)
            {
                Expired = true;
                return;
            }

            Vector2 dir = playerPos - Position;
            float dist = dir.Length();

            if (dist < AttractRadius)
            {
                dir.Normalize();
                Position += dir * AttractSpeed * elapsed;
            }

            if (dist < 20f)
            {
                Collected = true;
            }
        }


        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (Collected || Expired)
            {
                return;
            }

            Vector2 screenPos = Position - camera.Position;

            spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X - 6, (int)screenPos.Y - 6, 12, 12), Color.Gold);
            spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X - 3, (int)screenPos.Y - 3, 6, 6), Color.DarkGoldenrod);
        }
    }
}
