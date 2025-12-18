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
            axis.Normalize();
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
            float smallestOverlap = float.PositiveInfinity;
                Vector2 smallestAxis = Vector2.Zero;
            foreach (var axis in axes)
            {
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
            if (smallestAxis.LengthSquared() > 0f)
            {
                if (Vector2.Dot(direction, smallestAxis) < 0)
                {
                    smallestAxis *= -1;
                }
                    smallestAxis.Normalize();
            }
            Vector2 mtv = smallestAxis * smallestOverlap;
            return (true, mtv);
        }
    }
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1024; 
        _graphics.PreferredBackBufferHeight = 768;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        positionOfLine=new Vector2(200, 400);
      
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BlackTexture = Content.Load<Texture2D>("blacksquare");
        _box = new BoxObject(_BlackTexture, new Vector2(0, 0));
        _line = new LineObject(_BlackTexture, new Vector2(200, 400), new Rectangle(0, 0, 400, 10), 30);
        _line.Origin = new Vector2(_line.SourceRect.Width / 2f, _line.SourceRect.Height / 2f);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
          lineRotation = _line.Rotation;
        axes = new Vector2[]
        {
            new Vector2((float)Math.Cos(lineRotation), (float)Math.Sin(lineRotation)),
            new Vector2(-(float)Math.Sin(lineRotation), (float)Math.Cos(lineRotation)),
            new Vector2(1,0),
            new Vector2(0,1)
        };
        _box.Update(Window);
        _line.Update(Window);
        axes[0] = new Vector2((float)Math.Cos(lineRotation), (float)Math.Sin(lineRotation));   
        axes[1] = new Vector2(-(float)Math.Sin(lineRotation), (float)Math.Cos(lineRotation));  
        axes[2] = new Vector2(1, 0);  
        axes[3] = new Vector2(0, 1);  
        var result = SATHelper.CollisionData(_box.GetCorners(), _line.GetCorners(), axes);
        
        if (result.IsColliding)
        {

            Vector2 mtv = result.MTV;
             if (Vector2.Dot(mtv, _box._centreOfBox - _line.Position) < 0)
            {
                mtv = -mtv;
            }
              Vector2 normal = Vector2.Normalize(mtv);
              _box.Translate(mtv);
                _box.directionalVelocity.Y-= _box.directionalVelocity.Y + _box.directionalVelocity.Y* 0.1f;
                if (Vector2.Dot(normal, axes[3]) <= 0){ _box.gravityEffectOnBox = true;}
                else {_box.gravityEffectOnBox = false;}// if the collision occurs above the line
                _box.normalReactionForce = (float)(_box.gravity * _box.mass * Math.Abs(Math.Cos(lineRotation)));
                 _box.forceFromFriction = _box.normalReactionForce * _box.coefficientOfFriction;
 
                 tempVelocity = Math.Max(0, Math.Abs(_box.directionalVelocity.X) - _box.forceFromFriction);
                _box.directionalVelocity.X = Math.Sign(_box.directionalVelocity.X) * tempVelocity;
           
             Vector2[] corners = _box.GetCorners();
             Vector2 contactPoint = corners[0];

    float maxProjection = Vector2.Dot(corners[0], normal);
    foreach (var corner in corners)
    {
      float point = Vector2.Dot(corner, normal);
      if (point > maxProjection)
      {
        maxProjection = point;
        contactPoint = corner;
      }
    }     
           Vector2 collisionForce = normal * _box.normalReactionForce;
            Vector2 leverArm = contactPoint - _box._centreOfBox;
            float torque = leverArm.X * collisionForce.Y - leverArm.Y * collisionForce.X;
            float angularAcceleration = torque / _box.momentOfInertia;
            _box.angularVelocity += angularAcceleration * 0.016f; // scale for 60fps  
        }
        else
            {_box.gravityEffectOnBox = true;}
        


            // TODO: Add your update logic here

            base.Update(gameTime);
        
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _line.Draw(_spriteBatch);
       
       _box.Draw(_spriteBatch);
        _spriteBatch.End();
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
