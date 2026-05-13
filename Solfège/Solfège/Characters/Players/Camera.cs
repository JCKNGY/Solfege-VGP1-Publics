using Microsoft.Xna.Framework;

namespace Solfège
{
    public class Camera
    {
        public Vector2 Position;

        public readonly int screenWidth;
        public readonly int screenHeight;


        // make the camera
        public Camera(int screenW, int screenH, int mapW, int mapH)
        {
            screenWidth = screenW;
            screenHeight = screenH;
        }

        // put camera on the player
        public void CenterOn(Vector2 playerPos, Vector2 playerSize)
        {
            Position.X = playerPos.X + playerSize.X / 2f - screenWidth / 2f;
            Position.Y = playerPos.Y + playerSize.Y / 2f - screenHeight / 2f;
        }

        // camera follow the player
        public void Update(Vector2 playerPos, Vector2 playerSize)
        {
            CenterOn(playerPos, playerSize);
        }
    }
}
