using System;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Xml;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;

namespace monogame;

public class Game1 : Game
{
    //screen is 760x420
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static ImGuiRenderer GuiRenderer;
    private BoxObject _box;
    Texture2D _blackTexture;
    LineObject _line;
    Texture2D _arrowTexture;
    Rectangle[] _arrow;
    float lineRotation;
    Vector2 positionOfLine;
    Vector2[] axes;
     bool justCollided = true;
    SpriteFont _font;
    float dt = 0.016f;
    private bool _guiActive;
    

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
                    return (false, Vector2.Zero);
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
        _graphics.PreferredBackBufferHeight = 800;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        positionOfLine=new Vector2(200, 400);
        GuiRenderer = new ImGuiRenderer(this);
        _guiActive = true;
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _font = Content.Load<SpriteFont>("File");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _blackTexture = Content.Load<Texture2D>("blacksquare");
        _arrowTexture = Content.Load<Texture2D>("redarrow");
        _arrow = new Rectangle[3];
        for (int i = 0; i < 3; i++)
        {
        _arrow[i] = new Rectangle(0,0,50,100);
        }
        _box = new BoxObject(_blackTexture, new Vector2(0, 0));
        _line = new LineObject(_blackTexture, new Vector2(200, 400), new Rectangle(0, 0, 400, 10), 0);
        _line.Origin = new Vector2(_line.SourceRect.Width / 2f, _line.SourceRect.Height / 2f);
        GuiRenderer.RebuildFontAtlas();

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        for (int i = 0; i < _arrow.Length; i++ )
        {
           _arrow[i].X = (int)_box._centreOfBox.X; 
        }
        _arrow[0].Y = (int)_box._centreOfBox.Y + _box.GetHeight();  
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.M))
        {
            _guiActive = true;
        }
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
        if (_box.boxVelocity.Length()<0.5)
        {
            justCollided = false;
        }
        if (result.IsColliding)
        {

            Vector2 mtv = result.MTV;
             if (Vector2.Dot(mtv, _box._centreOfBox - _line.Position) < 0)
            {
                mtv = -mtv;
            }
              Vector2 normal = mtv;
              if (mtv.Length() > 0.001f )
              {
                normal = Vector2.Normalize(mtv);
              }
              Vector2 tangent = new Vector2(-normal.Y,normal.X);
              float velAlongNormal = Vector2.Dot(_box.boxVelocity, normal);

            bool flatOnSurface = Math.Abs(velAlongNormal) < 0.05f &&  Math.Abs(_box.angularVelocity) < 0.1f;
              
              if (mtv.LengthSquared() < 10000) // Prevent huge translations from NaN
              {
                  _box.Translate(mtv);
              }
            //_box.directionalVelocity*= -0.1f;
                if (velAlongNormal < 0)
                {
                    _box.boxVelocity -= normal * velAlongNormal;
                }
                _box.normalReactionForce = (float)(_box.gravity * _box.mass * Math.Abs(Math.Cos(lineRotation)));
                 _box.maxForceFromFriction = _box.normalReactionForce * _box.coefficientOfFriction;
                 float forceDownSlope = (float)(_box.mass * _box.gravity * Math.Abs(Math.Sin(lineRotation)));
                 float velAlongTangent = Vector2.Dot(_box.boxVelocity, tangent);
                 if (forceDownSlope <= _box.maxForceFromFriction && Math.Abs(velAlongTangent) < 0.01f)
                {
                 // cancel all motion along surface
                 _box.boxVelocity -= tangent * velAlongTangent;
                 _box.affectOfGravity = false; // Disable gravity when friction holds the box  
                }           
else
{
    _box.affectOfGravity = true; // Re-enable gravity when friction can't hold the box
    if (!float.IsNaN(velAlongTangent))
    {
        float frictionForce = _box.maxForceFromFriction;
        float frictionAcceleration = frictionForce / _box.mass;
        _box.boxVelocity -= tangent * Math.Sign(velAlongTangent) * frictionAcceleration;
    }
}
           Vector2 contactPoint;
           if (flatOnSurface)
            {
                contactPoint = _box._centreOfBox;
            }
            else{
             Vector2[] corners = _box.GetCorners();
             contactPoint = corners[0];

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
            }
           Vector2 collisionForce = normal * _box.normalReactionForce;
            Vector2 leverArm = contactPoint - _box._centreOfBox;
            _box.torque = leverArm.X * collisionForce.Y - leverArm.Y * collisionForce.X;
            if (flatOnSurface)
            {
                _box.angularVelocity = 0f;
                _box.torque = 0f;
            } 
            float angleDifference = Math.Abs(_box.rotation - lineRotation);
            _box.angularAcceleration = _box.torque*2 / _box.momentOfInertia;
            if (justCollided == false)
            {
                if (angleDifference > MathHelper.ToRadians(3f))
                {
               _box.angularVelocity += _box.angularAcceleration;
            }

                justCollided = true;
            }
            // Stop rotation if the box is nearly aligned with the slope
            if (angleDifference < MathHelper.ToRadians(3f))
            {
                 _box.rotation = lineRotation; 
                _box.angularVelocity = 0f;
            }
else
{
           _box.angularVelocity += _box.angularAcceleration; 
        } 
            
        }
        else
        {
            _box.affectOfGravity = true; // Re-enable gravity when not colliding
        }
            // TODO: Add your update logic here

            base.Update(gameTime);
        
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _line.Draw(_spriteBatch);
       _spriteBatch.DrawString(_font, $"Speed: {_box.boxVelocity.Length():F2}", new Vector2(0,0),Color.Black);
       _box.Draw(_spriteBatch);
       _spriteBatch.Draw(
         _arrowTexture,
           new Vector2(_arrow[0].X, _arrow[0].Y),
           null, 
          Color.Red,
          MathHelper.ToRadians(180f),
          new Vector2(_arrowTexture.Width / 2f, _arrowTexture.Height / 2f),
             0.05f,
          SpriteEffects.None,
            0f
            );
        _spriteBatch.DrawString(_font, $"{_box.forceFromGravity:F2}N",new Vector2(_arrow[0].X,_arrow[0].Y),Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);

        GuiRenderer.BeginLayout(gameTime);
        if (_guiActive)
{
    if (ImGui.BeginMainMenuBar())
    {
        if (ImGui.BeginMenu("Box properties"))
        {
           // if (ImGui.MenuItem("Open..", "Ctrl+O")) { /* Do stuff */ }
           // if (ImGui.MenuItem("Save", "Ctrl+S")) { /* Do stuff */ }
           // if (ImGui.MenuItem("Close", "Ctrl+W")) { _guiActive = false; }
            ImGui.SliderFloat("Coefficient of friction", ref _box.coefficientOfFriction, 0.0f, 1.0f);
            ImGui.SliderFloat("Mass", ref _box.mass, 0.0f, 100f);
            ImGui.EndMenu();
        }
        ImGui.EndMenuBar();
    }
    ImGui.End();
}
        GuiRenderer.EndLayout();
    }
}
