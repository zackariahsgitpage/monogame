using System;
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
    private Vector2 _centreOfBox;
    private Vector2 _mousePointBeforeRelease;
    private Vector2 _mousePointAfterRelease;

    public float gravity;
    public float horizontalAcceleration;
  public float verticalVelocity;
    public float horizontalVelocity;
    public float forceFromFriction;
    public float coefficientOfFriction;

    private float rotation;
    private bool edgeTrigger;
    private bool isMoving;
    private float resolvedSpeed;
    public float normalReactionForce;
    public float mass;

    public BoxObject(Texture2D texture, Vector2 initialPosition)
    {
        _texture = texture;
        _bounds = new Rectangle(0, 0, 100, 100);
        _centreOfBox = initialPosition;
        gravity = 20;
        verticalVelocity = 0;
        horizontalVelocity = 0;
        edgeTrigger = false;
        rotation = 0f;
        isMoving = false;
        resolvedSpeed = 0;
        normalReactionForce = 0;
        coefficientOfFriction = 0.1f;
        forceFromFriction = 0;
        mass = 1;
    }

    public void Update(GameWindow window)
    {
        MouseState mouse = Mouse.GetState();
        resolvedSpeed = (float)Math.Sqrt(verticalVelocity * verticalVelocity + horizontalVelocity * horizontalVelocity);
        if (verticalVelocity > 0 || horizontalVelocity > 0)
        { isMoving = true; }
        else
        { isMoving = false; }
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            _centreOfBox.X = mouse.X;
            _centreOfBox.Y = mouse.Y;
            _mousePointBeforeRelease.X = mouse.X;
            _mousePointBeforeRelease.Y = mouse.Y;
            edgeTrigger = true;
        }

        if (mouse.LeftButton == ButtonState.Released && edgeTrigger)
        {
            _mousePointAfterRelease.X = mouse.X;
            _mousePointAfterRelease.Y = mouse.Y;
            verticalVelocity += (float)(_mousePointAfterRelease.Y - _mousePointBeforeRelease.Y) / 5;
            horizontalVelocity += (float)(_mousePointAfterRelease.X - _mousePointBeforeRelease.X) / 5;
            edgeTrigger = false;
        }

        if (_centreOfBox.Y + _bounds.Height / 2 < window.ClientBounds.Height && mouse.LeftButton == ButtonState.Released)
        {
            verticalVelocity += mass * (gravity / 60);

            if (verticalVelocity < 0 && (_centreOfBox.Y - _bounds.Height / 2) < 0)
            {
                verticalVelocity = 0;
            }

            _centreOfBox.Y += (int)verticalVelocity;
        }
        else
        {
            verticalVelocity = 0;
        }

        if (_centreOfBox.X + _bounds.Width / 2 < window.ClientBounds.Width &&
            _centreOfBox.X - _bounds.Width / 2 > 0 &&
            mouse.LeftButton == ButtonState.Released)
        {
            _centreOfBox.X += (int)horizontalVelocity;
        }
        else
        {
            horizontalVelocity = 0;
        }

    }
    public Vector2[] GetCorners()
    {
        float halfWidth = _bounds.Width / 2f;
        float halfHeight = _bounds.Height / 2f;
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



