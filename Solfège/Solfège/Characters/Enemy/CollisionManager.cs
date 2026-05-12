using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Solfège
{

    public class CollisionManager
    {
        public static Rectangle GetRect(Vector2 position, Vector2 size)
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public static bool Overlaps(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB)
        {
            return GetRect(posA, sizeA).Intersects(GetRect(posB, sizeB));
        }

        public static bool CircleOverlapsRect(Vector2 circleCenter, float radius, Vector2 rectPos, Vector2 rectSize)
        {

            float closestX = MathHelper.Clamp(circleCenter.X, rectPos.X, rectPos.X + rectSize.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectPos.Y, rectPos.Y + rectSize.Y);

            float dx = circleCenter.X - closestX;
            float dy = circleCenter.Y - closestY;

            return (dx * dx + dy * dy) <= (radius * radius);
        }


        public static Vector2 ClampToWorld(Vector2 position, Vector2 size)
        {
            float maxX = Map.MapTilesWide * Map.TileWidth - size.X;
            float maxY = Map.MapTilesTall * Map.TileHeight - size.Y;

            return new Vector2(
                MathHelper.Clamp(position.X, 0, maxX),
                MathHelper.Clamp(position.Y, 0, maxY)
            );
        }


        public static void Update(Conductor conductor, WaveManager waveManager)
        {

            conductor.Position = ClampToWorld(conductor.Position, conductor.Size);

        }
    }
}
