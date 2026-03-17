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

    private Texture2D _texture;
    private Rectangle _bounds;
    public Vector2 _centreOfBox;
    private Vector2 _mousePointBeforeRelease;
    private Vector2 _mousePointAfterRelease;
    public float angularVelocity;
    public float momentOfInertia;
    public float gravity;
    public bool affectOfGravity;
    public float forceFromGravity;
  public float verticalVelocity;
    public float horizontalVelocity;
    public Vector2 directionalVelocity;
    public float maxForceFromFriction;
    public float coefficientOfFriction;

    public float rotation;
    private bool mouseHeld;
    private bool isMoving;
    private float resolvedSpeed;
    public float normalReactionForce;
    public float mass;
    public float boxScale { get; set; } = 1f;
    public float angularAcceleration;
    public float torque;


    public BoxObject(Texture2D texture, Vector2 initialPosition)
    {
        _texture = texture;
        _bounds = new Rectangle(0, 0, 100, 100);
        _centreOfBox = initialPosition;
        gravity = 1; // when gravity is 1, g=10m/s^2
        verticalVelocity = 0;
        horizontalVelocity = 0;
        mouseHeld = false;
        rotation = 0f;
        isMoving = false;
        resolvedSpeed = 0;
        normalReactionForce = 0f;
        coefficientOfFriction = 0.5f;
        mass = 10;
        torque = 0f;
        angularVelocity = 0f;
        directionalVelocity = new Vector2(0,0);
        momentOfInertia = (mass*(_bounds.Width*_bounds.Width + _bounds.Height*_bounds.Height)/12);
        affectOfGravity = true;
    }

    public void Update(GameWindow window)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();
        resolvedSpeed = directionalVelocity.Length();
        _centreOfBox = new Vector2(_centreOfBox.X, _centreOfBox.Y);
        if (verticalVelocity > 0 || horizontalVelocity > 0 || directionalVelocity.Length() > 0)
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
        }
        if (mouse.LeftButton == ButtonState.Released && mouseHeld)
        {
            _mousePointAfterRelease.X = mouse.X;
            _mousePointAfterRelease.Y = mouse.Y;
            directionalVelocity+= new Vector2((_mousePointAfterRelease.X - _mousePointBeforeRelease.X)/5,(_mousePointAfterRelease.Y - _mousePointBeforeRelease.Y)/5);
            mouseHeld = false;
            angularVelocity=0;
            angularAcceleration=0;
            torque=0;
        }

        if (_centreOfBox.Y + _bounds.Height / 2 < window.ClientBounds.Height && !mouseHeld)
        {
            {
                forceFromGravity = mass * (gravity);
                if (affectOfGravity)
                {
                directionalVelocity.Y += gravity ;
            }
            }
            if (directionalVelocity.Y < 0 && (_centreOfBox.Y - _bounds.Height / 2) < 0)
            {
                directionalVelocity.Y = 0;
            }
           if (affectOfGravity)
           {
            _centreOfBox.Y += directionalVelocity.Y;
           }
        }
        else
        {
            directionalVelocity.Y = 0;
        }

        if (_centreOfBox.X + _bounds.Width / 2 < window.ClientBounds.Width &&
            _centreOfBox.X - _bounds.Width / 2 > 0 &&
            mouse.LeftButton == ButtonState.Released)
        {
            _centreOfBox.X += directionalVelocity.X;
        }
        else
        {
            directionalVelocity.X = 0;
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
    public int GetWidth()
    {
        return _bounds.Width;
    }
    public int GetHeight()
    {
        return _bounds.Height;
    }



    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            _centreOfBox,
            _bounds,
            Color.Black,
            rotation,
            new Vector2(_bounds.Width / 2, _bounds.Height / 2),
            1.0f,
            SpriteEffects.None,
            0f
        );
    }
}



