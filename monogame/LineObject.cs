using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using monogame;

namespace monogame;
public class LineObject
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position;
    public Rectangle SourceRect { get; private set; }
    public float Rotation {get; set;} = 0f;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public float Scale { get; set; } = 1f;
    public Color Color { get; set; } = Color.Black;

    public LineObject(Texture2D texture, Vector2 position, Rectangle sourceRect, float rotation)
    {
        Texture = texture;
        Position = position;
        SourceRect = sourceRect;
        Rotation = -MathHelper.ToRadians(rotation);

    }
  public void Update(GameWindow window)
    {
         KeyboardState keyboard = Keyboard.GetState();
         if (keyboard.IsKeyDown(Keys.O))
        {
            Rotation +=MathHelper.ToRadians(1f);
        }
         if (keyboard.IsKeyDown(Keys.I))
        {
            Rotation -=MathHelper.ToRadians(1f);
        }
         if (keyboard.IsKeyDown(Keys.Right))
        {
         Position.X+=10;
        }
        if (keyboard.IsKeyDown(Keys.Left))
        {
         Position.X-=10;
        }
        if (keyboard.IsKeyDown(Keys.Up))
        {
         Position.Y-=10;
        }
        if (keyboard.IsKeyDown(Keys.Down))
        {
         Position.Y+=10;
        }
    }


    public Rectangle BoundingBox
    {
        get
        {
              float halfWidth = SourceRect.Width * Scale / 2f;
              float halfHeight = SourceRect.Height * Scale / 2f;
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)(halfWidth * Scale),
                (int)(halfHeight * Scale)
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

        Vector2 centre = Position;
        Vector2[] worldCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = centre + new Vector2(
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