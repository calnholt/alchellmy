using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
  class Level
  {
    private Tile[,] tiles;
    private Texture2D[] tileTextures;
    private int width;
    private int height;

    public Level(IServiceProvider serviceProvider, string path)
    {
      LoadTiles(serviceProvider, path);
    }

    private void LoadTiles(IServiceProvider serviceProvider, string path)
    {
      var content = new ContentManager(serviceProvider, "Content");
      tileTextures = new Texture2D[3];
      tileTextures[0] = content.Load<Texture2D>("Tiles/Passable");
      tileTextures[1] = content.Load<Texture2D>("Tiles/Impassable");
      tileTextures[2] = content.Load<Texture2D>("Tiles/Platform");

      string[] lines = File.ReadAllLines(path);
      width = lines[0].Length;
      height = lines.Length;
      tiles = new Tile[width, height];

      for (int y = 0; y < height; ++y)
      {
        for (int x = 0; x < width; ++x)
        {
          char tileType = lines[y][x];
          tiles[x, y] = LoadTile(tileType, x, y);
        }
      }
    }

    private Tile LoadTile(char tileType, int x, int y)
    {
      switch (tileType)
      {
        case '1':
          return new Tile(tileTextures[1], TileCollision.Impassable);
        case '2':
          return new Tile(tileTextures[2], TileCollision.Platform);
        default:
          return new Tile(tileTextures[0], TileCollision.Passable);
      }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      for (int y = 0; y < height; ++y)
      {
        for (int x = 0; x < width; ++x)
        {
          Texture2D texture = tiles[x, y].Texture;
          if (texture != null)
          {
            Vector2 position = new Vector2(x, y) * Tile.Size;
            spriteBatch.Draw(texture, position, Color.White);
          }
        }
      }
    }

    public TileCollision GetCollision(int x, int y)
    {
      if (x < 0 || x >= width || y < 0 || y >= height)
        return TileCollision.Impassable;
      return tiles[x, y].Collision;
    }

    public Rectangle GetBounds(int x, int y)
    {
      return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
    }
  }
}
