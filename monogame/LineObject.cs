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
    const float scale = 1f;
    protected Texture2D _texture;
    protected Vector2 _position;
    protected Rectangle _bounds;
    protected float _rotation= 0f;
    protected Vector2 origin = Vector2.Zero;
    protected Color _colour;
    public Vector2 GetPosition() {return _position;}
    public Rectangle GetBounds() {return _bounds;}
    public float GetRotation() {return _rotation;}
    public void SetRotation(float inputRotation) {_rotation = inputRotation;}
    public Vector2 GetOrigin() {return origin;}
    public void SetOrigin(Vector2 inputOrigin) {origin = inputOrigin;}


    public LineObject(Texture2D texture, Vector2 position, float rotation, Rectangle sourceRectangle)
    {
        _texture = texture;
        _position = position;
        _bounds = sourceRectangle;
        _rotation = -MathHelper.ToRadians(rotation);
        _colour = Color.Black;
    }
  public void Update(GameWindow window)
    {
         KeyboardState keyboardState = Keyboard.GetState();
         if (keyboardState.IsKeyDown(Keys.O))
        {
            _rotation +=MathHelper.ToRadians(1f);
        }
         if (keyboardState.IsKeyDown(Keys.I))
        {
            _rotation -=MathHelper.ToRadians(1f);
        }
         if (keyboardState.IsKeyDown(Keys.Right))
        {
         _position.X+=10;
        }
        if (keyboardState.IsKeyDown(Keys.Left))
        {
         _position.X-=10;
        }
        if (keyboardState.IsKeyDown(Keys.Up))
        {
         _position.Y-=10;
        }
        if (keyboardState.IsKeyDown(Keys.Down))
        {
         _position.Y+=10;
        }
    }


    public Rectangle BoundingBox
    {
        get
        {
              float halfWidth = _bounds.Width * scale / 2f;
              float halfHeight = _bounds.Height * scale / 2f;
            return new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                (int)(halfWidth * scale),
                (int)(halfHeight * scale)
            );
        }
    }

    public Vector2[] GetCorners()
    {
        float halfWidth = _bounds.Width * scale / 2f;
        float halfHeight = _bounds.Height * scale / 2f;
        float cos = (float)Math.Cos(_rotation);
        float sin = (float)Math.Sin(_rotation);

        // Local corners relative to origin (center)
        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-halfWidth, -halfHeight),
            new Vector2(halfWidth, -halfHeight),
            new Vector2(halfWidth, halfHeight),
            new Vector2(-halfWidth, halfHeight)
        };

        Vector2 centre = _position;
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
        spriteBatch.Draw(_texture, _position, _bounds, _colour, _rotation, origin, scale, SpriteEffects.None, 0f);
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
            _colour = Color.Gold;
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
            _colour = Color.DarkGray;
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
            _colour = Color.LightBlue;
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
            _colour = Color.Gray;
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
            _colour = Color.Black;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 4;
        }
    }