using System;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame;

public class Game1 : Game
{
    //screen is 760x420
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private BoxObject _box;
    Texture2D _BlackTexture;
    LineObject _line;
    float tempVelocity;
    float lineRotation;
    Vector2 positionOfLine;
    Vector2[] axes;
    

  public static class SATHelper
    {
        public static float[] ProjectOntoAxis(Vector2[] worldCorners, Vector2 axis)
        {
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            foreach (var corner in worldCorners)
            {
                float projection = Vector2.Dot(corner, axis);
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }
            return new float[] { min, max };
        }
        public static (bool IsColliding, Vector2 MTV) CollisionData(Vector2[] CornersA, Vector2[] CornersB, Vector2[] axes)
        {
            foreach (var axis in axes)
            {
                float smallestOverlap = float.PositiveInfinity;
                Vector2 smallestAxis = Vector2.Zero;
                float[] projectionA = ProjectOntoAxis(CornersA, axis);
                float[] projectionB = ProjectOntoAxis(CornersB, axis);

                if (projectionA[1] < projectionB[0] || projectionB[1] < projectionA[0])
                {
                    return (false, Vector2.Zero); // Found a separating axis
                }
                float overlap = Math.Min(projectionA[1], projectionB[1]) - Math.Max(projectionA[0], projectionB[0]);
                if (overlap < smallestOverlap)
                {
                    smallestOverlap = overlap;
                    smallestAxis = axis;
                }
            }
            Vector2 direction = CornersB[0] - CornersA[0];
            if (Vector2.Dot(direction, smallestAxis) < 0)
            {
                smallestAxis *= -1;
            }
            Vector2 mtv = smallestAxis * smallestOverlap;
            return (true, mtv);
        }
    }
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        positionOfLine=new Vector2(200, 400);
        lineRotation = -MathHelper.ToRadians(0);
        axes = new Vector2[]
        {
            new Vector2((float)Math.Cos(lineRotation), (float)Math.Sin(lineRotation)),
            new Vector2(-(float)Math.Sin(lineRotation), (float)Math.Cos(lineRotation)),
            new Vector2(1,0),
            new Vector2(0,1)
        };
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BlackTexture = Content.Load<Texture2D>("blacksquare");
        _box = new BoxObject(_BlackTexture, new Vector2(0, 0));
        _line = new LineObject(_BlackTexture, new Vector2(200, 400), new Rectangle(0, 0, 400, 10), lineRotation);
        _line.Origin = new Vector2(_line.SourceRect.Width / 2f, _line.SourceRect.Height / 2f);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
       _box.Update(Window);

        if (SATHelper.IsColliding(_box.GetCorners(),_line.GetCorners(), axes))
        {
            _box.verticalVelocity -= _box.verticalVelocity*0.5f+ _box.verticalVelocity;
            _box.normalReactionForce = (float)(_box.gravity*_box.mass*Math.Abs(Math.Cos(lineRotation)));
            _box.forceFromFriction = (_box.normalReactionForce * _box.coefficientOfFriction);
            tempVelocity = Math.Max(0, Math.Abs(_box.horizontalVelocity) - _box.forceFromFriction);
            _box.horizontalVelocity = Math.Sign(_box.horizontalVelocity) * tempVelocity;
        }
       
        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        //_spriteBatch.Draw(_BlackTexture, _LineObject, Color.Black);
        _line.Draw(_spriteBatch);
       // _spriteBatch.Draw(
           //  _BlackTexture,
           // positionOfLine,
           // _line.SourceRect,
           // Color.Black,
           // lineRotation,
           // new Vector2(0,0),
           // 1.0f,
           // SpriteEffects.None,
           // 0f);
       _box.Draw(_spriteBatch);
        _spriteBatch.End();
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
