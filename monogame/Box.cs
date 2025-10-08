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
    private Vector2 _position;
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
        _position = initialPosition;
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
        resolvedSpeed = (float)Math.Sqrt(verticalVelocity*verticalVelocity+horizontalVelocity*horizontalVelocity);
        if (verticalVelocity > 0 || horizontalVelocity > 0)
        { isMoving = true; }
        else
        { isMoving = false; }
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            _position.X = mouse.X;
            _position.Y = mouse.Y;
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

        if (_position.Y + _bounds.Height / 2 < window.ClientBounds.Height && mouse.LeftButton == ButtonState.Released)
        {
            verticalVelocity += mass*(gravity / 60);

            if (verticalVelocity < 0 && (_position.Y - _bounds.Height / 2) < 0)
            {
                verticalVelocity = 0;
            }

            _position.Y += (int)verticalVelocity;
        }
        else
        {
            verticalVelocity = 0;
        }

        if (_position.X + _bounds.Width / 2 < window.ClientBounds.Width &&
            _position.X - _bounds.Width / 2 > 0 &&
            mouse.LeftButton == ButtonState.Released)
        {
            _position.X += (int)horizontalVelocity;
        }
        else
        {
            horizontalVelocity = 0;
        }
        
    }
    public Rectangle BoundingBox
    {
        get
        {
            return new Rectangle(
                (int)(_position.X - _bounds.Width / 2), //left side
                (int)(_position.Y - _bounds.Height / 2),//top side
                _bounds.Width, //right side
                _bounds.Height //bottom side
            );
        }
        
    }



    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            _position,
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



