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
    const float SCALE = 1f;
    protected Texture2D texture;
    protected Vector2 position;
    protected Rectangle bounds;
    protected float rotation= 0f;
    protected Vector2 origin = Vector2.Zero;
    protected Color colour;
    public Vector2 GetPosition() {return position;}
    public Rectangle GetBounds() {return bounds;}
    public float GetRotation() {return rotation;}
    public void SetRotation(float inputRotation) {rotation = inputRotation;}
    public Vector2 GetOrigin() {return origin;}
    public void SetOrigin(Vector2 inputOrigin) {origin = inputOrigin;}


    public LineObject(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle)
    {
        this.texture = texture;
        this.position = position;
        bounds = sourceRectangle;
        this.rotation = -MathHelper.ToRadians(rotation);
        colour = Color.Black;
    }
  public void Update(GameWindow window)
    {
         KeyboardState keyboardState = Keyboard.GetState();
         if (keyboardState.IsKeyDown(Keys.O))
        {
            rotation +=MathHelper.ToRadians(1f);
        }
         if (keyboardState.IsKeyDown(Keys.I))
        {
            rotation -=MathHelper.ToRadians(1f);
        }
         if (keyboardState.IsKeyDown(Keys.Right))
        {
         position.X+=10;
        }
        if (keyboardState.IsKeyDown(Keys.Left))
        {
         position.X-=10;
        }
        if (keyboardState.IsKeyDown(Keys.Up))
        {
         position.Y-=10;
        }
        if (keyboardState.IsKeyDown(Keys.Down))
        {
         position.Y+=10;
        }
    }


    public Rectangle BoundingBox
    {
        get
        {
              float halfWidth = bounds.Width * SCALE / 2f;
              float halfHeight = bounds.Height * SCALE / 2f;
            return new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)(halfWidth * SCALE),
                (int)(halfHeight * SCALE)
            );
        }
    }

    public Vector2[] GetCorners()
    {
        float halfWidth = bounds.Width * SCALE / 2f;
        float halfHeight = bounds.Height * SCALE / 2f;
        float cos = (float)Math.Cos(rotation);
        float sin = (float)Math.Sin(rotation);

        // Local corners relative to origin (center)
        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-halfWidth, -halfHeight),
            new Vector2(halfWidth, -halfHeight),
            new Vector2(halfWidth, halfHeight),
            new Vector2(-halfWidth, halfHeight)
        };

        Vector2 centre = position;
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
        spriteBatch.Draw(texture, position, bounds, colour, rotation, origin, SCALE, SpriteEffects.None, 0f);
    }
     public virtual int GetIndexOfMaterialInMatrix()
        {
            return -1;
        }
}

 public class BrassLine : LineObject
    {
        public BrassLine(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle) 
        : base(texture, position, rotation, sourceRectangle)
        {
            colour = Color.Gold;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 0;
        }
    }
    public class CastIronLine : LineObject
    {
        public CastIronLine(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle) 
        : base(texture, position, rotation, sourceRectangle)
        {
            colour = Color.DarkGray;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 1;
        }
    }
    public class IceLine : LineObject
    {
        public IceLine(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle) 
        : base(texture, position, rotation, sourceRectangle)
        {
            colour = Color.LightBlue;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 2;
        }
    }
    public class SteelLine : LineObject
    {
        public SteelLine(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle) 
        : base(texture, position, rotation, sourceRectangle)
        {
            colour = Color.Gray;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 3;
        }
    }
    public class DefaultLine : LineObject
    {
        public DefaultLine(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle) 
        : base(texture, position, rotation, sourceRectangle)
        {
            colour = Color.Black;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 4;
        }
    }