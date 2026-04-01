using System;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame;


public class BoxObject
{

    protected Texture2D _texture;
    protected Rectangle _bounds;
    public Vector2 _centreOfBox;
    protected Vector2 _mousePointBeforeRelease;
    protected Vector2 _mousePointAfterRelease;
    public float angularVelocity;
    public float momentOfInertia;
    protected float gravity;
    public bool affectOfGravity;
    public float forceFromGravity;
    protected float gravityScale;
 
    public Vector2 boxVelocity;
    public float maxForceFromFriction;
    public float coefficientOfFriction;

    public float rotation;
    protected bool mouseHeld;
    protected bool isMoving;
    private float resolvedSpeed;
    protected float normalReactionForce;
    public float mass;
    public float boxScale { get; set; } = 1f;
    public float angularAcceleration;
    public float torque;
    protected float accelerationFromGravity;
    public float displayedForceFromGravity;
    protected bool boxHeld;
    protected Color colour;


    public float GetGravity() {return gravity;}
    public void SetGravity(float inputGrav) {gravity = inputGrav;}
    public float GetMass() {return mass;}
    public void SetMass(float inputMass) {mass = inputMass;}
     public int GetWidth() {return _bounds.Width;}
    public int GetHeight() {return _bounds.Height;}
    public float GetNormalReactionForce() {return normalReactionForce;}
    public void SetNormalReactionForce(float inputForce) {normalReactionForce = inputForce;}
        
    
    public BoxObject(Texture2D texture, Vector2 initialPosition, Rectangle sourceRectangle)
    {
        _texture = texture;
        _bounds = sourceRectangle;
        _centreOfBox = initialPosition;
        gravity = 9.8f; // when gravity is 1, g=10m/s^2
        gravityScale = 0.1f;
        mouseHeld = false;
        rotation = 0f;
        isMoving = false;
        resolvedSpeed = 0;
        normalReactionForce = 0f;
        coefficientOfFriction = 0.5f;
        mass = 1f;
        torque = 0f;
        angularVelocity = 0f;
        boxVelocity = new Vector2(0,0);
        affectOfGravity = true;
        boxHeld = false;
        accelerationFromGravity = gravityScale * gravity;
        colour = Color.Black;
    }

    public void Update(GameWindow window)
    {
        boxHeld = false;
        displayedForceFromGravity = mass*gravity;
        momentOfInertia = mass*(_bounds.Width*_bounds.Width + _bounds.Height*_bounds.Height)/12;
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();
        resolvedSpeed = boxVelocity.Length();
        _centreOfBox = new Vector2(_centreOfBox.X, _centreOfBox.Y);
        if (boxVelocity.Y> 0 || boxVelocity.X > 0 || boxVelocity.Length() > 0)
        { isMoving = true; }
        else
        { isMoving = false; }
        if (mouse.LeftButton == ButtonState.Pressed && (BoundingBox.Contains(mouse.Position) || mouseHeld) )
        {
            _centreOfBox = mouse.Position.ToVector2();
            _mousePointBeforeRelease.X = mouse.X;
            _mousePointBeforeRelease.Y = mouse.Y;
            mouseHeld = true;
            rotation = 0f;
            boxHeld = true;
        }
        if (mouse.LeftButton == ButtonState.Released && mouseHeld)
        {
            _mousePointAfterRelease.X = mouse.X;
            _mousePointAfterRelease.Y = mouse.Y;
            boxVelocity+= new Vector2((_mousePointAfterRelease.X - _mousePointBeforeRelease.X)/5,(_mousePointAfterRelease.Y - _mousePointBeforeRelease.Y)/5);
            mouseHeld = false;
            angularVelocity=0;
            angularAcceleration=0;
            torque=0;
            affectOfGravity = true;
        }

        if (_centreOfBox.Y + _bounds.Height / 2 < window.ClientBounds.Height && !mouseHeld)
        {
            {
                forceFromGravity = mass * (gravity)*gravityScale;
                if (affectOfGravity)
                {
                boxVelocity.Y += accelerationFromGravity;
            }
            }
            if (boxVelocity.Y < 0 && (_centreOfBox.Y - _bounds.Height / 2) < 0)
            {
                boxVelocity.Y = 0;
            }
           if (affectOfGravity)
           {
            _centreOfBox.Y += boxVelocity.Y;
           }
        }
        else
        {
            boxVelocity.Y = 0;
        }

        if (_centreOfBox.X + _bounds.Width / 2 < window.ClientBounds.Width &&
            _centreOfBox.X - _bounds.Width / 2 > 0 &&
            !boxHeld)
        {
            _centreOfBox.X += boxVelocity.X;
        }
        else
        {
            boxVelocity.X = 0;
        }
    angularVelocity *= 0.9f; 
    rotation-= angularVelocity;
    
    }
    public void Translate(Vector2 vectorToTranslateBy)
    {
        _centreOfBox += vectorToTranslateBy;
    }
    public Vector2[] GetCorners()
    {
        float halfWidth = _bounds.Width *boxScale / 2f;
        float halfHeight = _bounds.Height *boxScale / 2f;
        float cos = (float)Math.Cos(rotation);
        float sin = (float)Math.Sin(rotation);

        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-halfWidth, -halfHeight), // Top-left
            new Vector2(halfWidth, -halfHeight),  // Top-right
            new Vector2(halfWidth, halfHeight),   // Bottom-right
            new Vector2(-halfWidth, halfHeight),  // Bottom-left
        };
        Vector2[] worldCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = _centreOfBox + new Vector2(
                localCorners[i].X * cos - localCorners[i].Y * sin,
                localCorners[i].X * sin + localCorners[i].Y * cos 
            );
        }
        return worldCorners;
    }
    public Rectangle BoundingBox
    {
        get
        {
            return new Rectangle(
                (int)(_centreOfBox.X - _bounds.Width / 2), //left side
                (int)(_centreOfBox.Y - _bounds.Height / 2),//top side
                _bounds.Width, //right side
                _bounds.Height //bottom side
            );
        }
        
    }
    public class BrassBox : BoxObject
    {
        public BrassBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.Gold;
        }
        public int GetIndexOfMaterialInMatrix()
        {
            return 0;
        }
    }
    public class CastIronBox : BoxObject
    {
        public CastIronBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.Gray;
        }
        public int GetIndexOfMaterialInMatrix()
        {
            return 1;
        }
    }
    public class IceBox : BoxObject
    {
        public IceBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.LightBlue;
        }
        public int GetIndexOfMaterialInMatrix()
        {
            return 2;
        }
    }
    public class SteelBox : BoxObject
    {
        public SteelBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.LightGray;
        }
        public int GetIndexOfMaterialInMatrix()
        {
            return 3;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            _centreOfBox,
            _bounds,
            colour,
            rotation,
            new Vector2(_bounds.Width / 2, _bounds.Height / 2),
            1.0f,
            SpriteEffects.None,
            0f
        );
    }
}



