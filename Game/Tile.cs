using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
  public enum TileCollision
  {
    Passable = 0,
    Impassable = 1,
    Platform = 2,
  }

  public struct Tile
  {
    public Texture2D Texture;
    public TileCollision Collision;

    public const int Width = 64;
    public const int Height = 64;

    public static readonly Vector2 Size = new Vector2(Width, Height);

    public Tile(Texture2D texture, TileCollision collision)
    {
      Texture = texture;
      Collision = collision;
    }
  }
}
