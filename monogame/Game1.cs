using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.XPath;
using ImGuiNET;
using Microsoft.VisualBasic;
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
    private float setMassOfBox;
    Vector2 positionOfLine;
    Vector2[] axes;
     bool justCollided = true;
    SpriteFont _font;
    float dt = 0.016f;
    private bool _guiActive;
    private float forceDownSlope;
    private float velAlongTangent;
    private Vector2 tangent;
    private float velAlongNormal;
    private Vector2 normal;
    private Vector2 north;
    private float angleBetweenNorthAndFriction;
    private Vector2 frictionDirection;
    private float radius = 2700f;
    private float displayedForceFromFriction;
    protected List<BoxObject> listOfBoxes;
    protected bool spawnKeyPressed;
    protected float[,] coefficientsMatrix;
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
    public static float GetAngleBetweenVectors(Vector2 vector1, Vector2 vector2)
    {
    vector1.Normalize();
    vector2.Normalize();
    float dotProduct = Vector2.Dot(vector1, vector2);
    dotProduct = MathHelper.Clamp(dotProduct, -1f, 1f);
    float angleRadians = (float)Math.Acos(dotProduct);
    return angleRadians;
    }

    public void SaveFile()
    {

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
        coefficientsMatrix = new float[4,4];
        north = new Vector2 (0,1);
        spawnKeyPressed = false;
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _font = Content.Load<SpriteFont>("File");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _blackTexture = Content.Load<Texture2D>("blacksquare");
        _arrowTexture = Content.Load<Texture2D>("arrow");
        _arrow = new Rectangle[3];
        for (int i = 0; i < 3; i++)
        {
        _arrow[i] = new Rectangle(0,0,50,100);
        }
        coefficientsMatrix[0,0] = 0.5f;
        coefficientsMatrix[0,1] = 0.3f;
        coefficientsMatrix[0,2] = 0.02f;
        coefficientsMatrix[0,3] = 0.44f;
        coefficientsMatrix[1,0] = 0.3f;
        coefficientsMatrix[1,1] = 0.15f;
        coefficientsMatrix[1,2] = 0.03f;
        coefficientsMatrix[1,3] = 0.23f;
        coefficientsMatrix[2,0] = 0.02f;
        coefficientsMatrix[2,1] = 0.04f;
        coefficientsMatrix[2,2] = 0.01f;
        coefficientsMatrix[2,3] = 0.05f;
        coefficientsMatrix[3,0] = 0.44f;
        coefficientsMatrix[3,1] = 0.23f;
        coefficientsMatrix[3,2] = 0.06f;
        coefficientsMatrix[3,3] = 0.42f;
        listOfBoxes = new List<BoxObject>();
       // _box = new BoxObject(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0), new Rectangle(0, 0, 100, 100));
        _line = new LineObject(_blackTexture, new Vector2(200, 400), 0, new Rectangle(0, 0, 800, 10));
        _line.Origin = new Vector2(_line._bounds.Width / 2f, _line._bounds.Height / 2f);
        GuiRenderer.RebuildFontAtlas();

        // TODO: use this.Content to load your game content here
    }
    public void SpawnBox(int inputInt)
    {
        switch (inputInt)
        {
            case 0: 
            listOfBoxes.Add(new BoxObject.BrassBox(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0),
             new Rectangle(0, 0, 100, 100)));
             break;
            case 1:
            listOfBoxes.Add(new BoxObject.CastIronBox(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0),
             new Rectangle(0, 0, 100, 100)));
             break;
            case 2:
            listOfBoxes.Add(new BoxObject.IceBox(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0),
             new Rectangle(0, 0, 100, 100)));
             break;
            case 3: 
            listOfBoxes.Add(new BoxObject.SteelBox(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0),
             new Rectangle(0, 0, 100, 100)));
             break;
            case 4: 
            listOfBoxes.Add(new BoxObject(_blackTexture, new Vector2(_graphics.PreferredBackBufferWidth/2, 0),
             new Rectangle(0, 0, 100, 100)));
             break;
        }
    }
    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (!spawnKeyPressed && keyboardState.IsKeyDown(Keys.G))
        {
            SpawnBox(0);
             spawnKeyPressed = true;
        }
        if (!spawnKeyPressed && keyboardState.IsKeyDown(Keys.H))
        {
            SpawnBox(1);
             spawnKeyPressed = true;
        }
        if (!spawnKeyPressed && keyboardState.IsKeyDown(Keys.J))
        {
            SpawnBox(2);
             spawnKeyPressed = true;
        }
        if (!spawnKeyPressed && keyboardState.IsKeyDown(Keys.K))
        {
            SpawnBox(3);
             spawnKeyPressed = true;
        }
        if (!spawnKeyPressed && keyboardState.IsKeyDown(Keys.L))
        {
            SpawnBox(4);
            spawnKeyPressed = true;
        }
        else if (keyboardState.IsKeyUp(Keys.G) && keyboardState.IsKeyUp(Keys.H) && keyboardState.IsKeyUp(Keys.J) && keyboardState.IsKeyUp(Keys.K) && keyboardState.IsKeyUp(Keys.L))
        {spawnKeyPressed = false;}
         if (keyboardState.IsKeyDown(Keys.M))
        {_guiActive = true;}
        lineRotation = _line._rotation;
       _line.Update(Window);
        foreach (BoxObject _box in listOfBoxes)
        {
        if (_box.maxForceFromFriction > forceDownSlope)
        {
            displayedForceFromFriction = forceDownSlope * 10f;
        }
        else
        {
            displayedForceFromFriction = _box.maxForceFromFriction * 10f;
        }
        for (int i = 0; i < _arrow.Length; i++ )
        {
           _arrow[i].X = (int)_box._centreOfBox.X; 
        }
        _arrow[0].Y = (int)_box._centreOfBox.Y + _box.GetHeight();
        _arrow[1].Y = (int)_box._centreOfBox.Y - _box.GetHeight(); 
        axes = new Vector2[]
        {
            new Vector2((float)Math.Cos(lineRotation), (float)Math.Sin(lineRotation)),
            new Vector2(-(float)Math.Sin(lineRotation), (float)Math.Cos(lineRotation)),
            new Vector2(1,0),
            new Vector2(0,1)
        };
        _box.Update(Window);
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
             if (Vector2.Dot(mtv, _box._centreOfBox - _line._position) < 0)
            {
                mtv = -mtv;
            }
              normal = mtv;
              if (mtv.Length() > 0.001f )
              {
                normal = Vector2.Normalize(mtv);
              }
              tangent = new Vector2(-normal.Y,normal.X);
             velAlongNormal = Vector2.Dot(_box.boxVelocity, normal);

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
                _box.SetNormalReactionForce((float)(_box.forceFromGravity * Math.Abs(Math.Cos(lineRotation))));
                 _box.maxForceFromFriction = _box.GetNormalReactionForce() * _box.coefficientOfFriction;
                 forceDownSlope = (float)(_box.forceFromGravity * Math.Abs(Math.Sin(lineRotation)));
                 velAlongTangent = Vector2.Dot(_box.boxVelocity, tangent);

                if (forceDownSlope <= _box.maxForceFromFriction && Math.Abs(velAlongTangent) < 0.01f
                  && _box.boxVelocity.Length() < 0.5f)
                {
                 // cancel all motion along surface
                 //_box.boxVelocity -= tangent * velAlongTangent;
                 _box.affectOfGravity = false; // Disable gravity when friction holds the box  
                }           
else
{
    _box.affectOfGravity = true; // Re-enable gravity when friction can't hold the box
    if (!float.IsNaN(velAlongTangent))
    {
        float frictionForce = _box.maxForceFromFriction;
        float frictionAcceleration = frictionForce / _box.GetMass();
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
           Vector2 collisionForce = normal * _box.GetNormalReactionForce();
            Vector2 leverArm = contactPoint - _box._centreOfBox;
            _box.torque = leverArm.X * collisionForce.Y - leverArm.Y * collisionForce.X;
         
            if (flatOnSurface)
            {
                _box.angularVelocity = 0f;
                _box.torque = 0f;
            } 
            float angleDifference = Math.Abs(_box.rotation - lineRotation);
            _box.angularAcceleration = _box.torque / (_box.momentOfInertia *2);
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
            _box.affectOfGravity = true;
          _box.angularVelocity += _box.angularAcceleration;
        }
        frictionDirection = -tangent * Math.Sign(velAlongTangent);
        }
        
            // TODO: Add your update logic here
            base.Update(gameTime);
        
    }
    

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _line.Draw(_spriteBatch);
    //    _spriteBatch.DrawString(_font, $"Speed: {_box.boxVelocity.Length():F2}", new Vector2(0,0),Color.Black);
        foreach (BoxObject _box in listOfBoxes)
        {
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
        _spriteBatch.Draw(
            _arrowTexture,
            _box._centreOfBox,
            null,
            Color.Blue,
            lineRotation+MathHelper.ToRadians(90f)*-Math.Sign(velAlongTangent),
            new Vector2(_arrowTexture.Width/2, radius),
            0.05f,
            SpriteEffects.None,
            0f
        );
         _spriteBatch.DrawString(_font, $"{_box.displayedForceFromGravity:F2}N",new Vector2(_arrow[0].X,_arrow[0].Y),Color.White);
         _spriteBatch.DrawString(_font, $"{displayedForceFromFriction:F2}N",new Vector2(_arrow[1].X, _arrow[1].Y),Color.White);
        }
      //  _spriteBatch.DrawString(_font, $"Angular Velocity: {_box.angularVelocity:F4}", new Vector2(0, 20), Color.Black);
//_spriteBatch.DrawString(_font, $"Torque: {_box.torque:F4}", new Vector2(0, 40), Color.Black);
//_spriteBatch.DrawString(_font, $"Angular Accel: {_box.angularAcceleration:F4}", new Vector2(0, 60), Color.Black);
// _spriteBatch.DrawString(_font, $"Normal Force: {_box.GetNormalReactionForce():F4}", new Vector2(0, 80), Color.Black);
// _spriteBatch.DrawString(_font, $"Box Velocity: {_box.boxVelocity:F2}", new Vector2(0, 100), Color.Black);
// _spriteBatch.DrawString(_font, $"Rotation: {MathHelper.ToDegrees(_box.rotation):F2} deg", new Vector2(0, 120), Color.Black);
// _spriteBatch.DrawString(_font, $"Line Rotation: {MathHelper.ToDegrees(_line._rotation):F2} deg", new Vector2(0, 140), Color.Black);
// _spriteBatch.DrawString(_font, $"Angle Diff: {MathHelper.ToDegrees(Math.Abs(_box.rotation - _line._rotation)):F2} deg", new Vector2(0, 160), Color.Black);
// _spriteBatch.DrawString(_font, $"justCollided: {justCollided}", new Vector2(0, 180), Color.Black);
// _spriteBatch.DrawString(_font, $"affectOfGravity: {_box.affectOfGravity}", new Vector2(0, 200), Color.Black);
// _spriteBatch.DrawString(_font, $"forceDownSlope: {forceDownSlope:F4}", new Vector2(0, 220), Color.Black);
// _spriteBatch.DrawString(_font, $"maxFriction: {_box.maxForceFromFriction:F4}", new Vector2(0, 240), Color.Black);
// _spriteBatch.DrawString(_font, $"velAlongTangent: {velAlongTangent:F4}", new Vector2(0, 260), Color.Black);
        _spriteBatch.End();
        base.Draw(gameTime);

        GuiRenderer.BeginLayout(gameTime);
        if (_guiActive)
{
    if (ImGui.BeginMainMenuBar())
    {
        if (ImGui.BeginMenu("Boxes"))
        {
           // if (ImGui.MenuItem("Open..", "Ctrl+O")) { /* Do stuff */ }
           // if (ImGui.MenuItem("Save", "Ctrl+S")) { /* Do stuff */ }
           // if (ImGui.MenuItem("Close", "Ctrl+W")) { _guiActive = false; }
           foreach (BoxObject _box in listOfBoxes)
           {
            if (ImGui.BeginMenu($"Box {listOfBoxes.IndexOf(_box)+1}"))
                    {
                        ImGui.SliderFloat("Mass", ref _box.mass, 0.0f, 100f);
                        ImGui.SliderFloat("Coefficient of friction", ref _box.coefficientOfFriction, 0.0f, 1.0f);               
                        ImGui.EndMenu();
                    }
           }
            ImGui.EndMenu();
        }
        ImGui.EndMenuBar();
    }
    ImGui.End();
}
        GuiRenderer.EndLayout();
    }
}
