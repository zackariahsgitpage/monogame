using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame;
public class LineObject
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position { get; set; }
    public Rectangle SourceRect { get; private set; }
    public float Rotation { get; set; }
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public float Scale { get; set; } = 1f;
    public Color Color { get; set; } = Color.Black;

    public LineObject(Texture2D texture, Vector2 position, Rectangle sourceRect, float rotation = 0f)
    {
        Texture = texture;
        Position = position;
        SourceRect = sourceRect;
        Rotation = rotation;
    }

    // Axis-aligned bounding box at the line's drawn position (ignores rotation for simplicity,
    // matching the original approach which used a Rectangle for intersection)
    public Rectangle BoundingBox
    {
        get
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)(SourceRect.Width * Scale),
                (int)(SourceRect.Height * Scale)
            );
        }
    }

    public Vector2[] GetCorners()
    {
        float halfWidth = SourceRect.Width * Scale / 2f;
        float halfHeight = SourceRect.Height * Scale / 2f;
        float cos = (float)Math.Cos(Rotation);
        float sin = (float)Math.Sin(Rotation);

        // Local corners relative to origin (center)
        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-halfWidth, -halfHeight),
            new Vector2(halfWidth, -halfHeight),
            new Vector2(halfWidth, halfHeight),
            new Vector2(-halfWidth, halfHeight)
        };

        Vector2 center = Position + Origin;
        Vector2[] worldCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = center + new Vector2(
                localCorners[i].X * cos - localCorners[i].Y * sin,
                localCorners[i].X * sin + localCorners[i].Y * cos
            );
        }
        return worldCorners;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position, SourceRect, Color, Rotation, Origin, Scale, SpriteEffects.None, 0f);
    }
}