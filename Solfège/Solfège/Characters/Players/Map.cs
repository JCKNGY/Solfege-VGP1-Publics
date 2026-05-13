using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Solfège
{
    public class Map
    {
        public const int TileWidth = 32;
        public const int TileHeight = 32;

        public const int MapTilesWide = 500;
        public const int MapTilesTall = 500;

        public int MapWidthPixels = MapTilesWide * TileWidth;
        public int MapHeightPixels = MapTilesTall * TileHeight;

        public Texture2D floorTile;


        // generate a checkered tile texture at startup
        public Map(ContentManager content, GraphicsDevice graphicsDevice)
        {
            floorTile = new Texture2D(graphicsDevice, TileWidth, TileHeight);
            Color[] pixels = new Color[TileWidth * TileHeight];

            for (int y = 0; y < TileHeight; y++)
            {
                for (int x = 0; x < TileWidth; x++)
                {
                    bool isBorder = (x == 0 || y == 0 || x == TileWidth - 1 || y == TileHeight - 1);

                    if (isBorder)
                    {
                        pixels[y * TileWidth + x] = new Color(150, 150, 150);
                    }
                    else
                    {
                        pixels[y * TileWidth + x] = new Color(220, 220, 220);
                    }
                }
            }

            floorTile.SetData(pixels);
        }


        public bool IsWall(int tileX, int tileY)
        {
            return false;
        }


        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startX = (int)Math.Floor(camera.Position.X / TileWidth);
            int startY = (int)Math.Floor(camera.Position.Y / TileHeight);
            int tilesX = (1280 / TileWidth) + 2;
            int tilesY = (720 / TileHeight) + 2;

            for (int y = startY; y < startY + tilesY; y++)
            {
                for (int x = startX; x < startX + tilesX; x++)
                {
                    Vector2 worldPos = new Vector2(x * TileWidth, y * TileHeight);
                    Vector2 screenPos = worldPos - camera.Position;
                    spriteBatch.Draw(floorTile, screenPos, Color.White);
                }
            }
        }
    }
}
