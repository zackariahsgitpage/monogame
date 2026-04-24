
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
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
    const float speedDisplayScale = 0.25f;
    const float displayedForcesScale = 1 / 0.075f;
    const int radius = 2700;
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch _spriteBatch;
    protected static ImGuiRenderer guiRenderer;
    protected Texture2D blackTexture;
    protected LineObject _line;
    protected Texture2D arrowTexture;
    protected Rectangle[] arrow;
    private float lineRotation;
    // protected Vector2[] axes;
    //protected bool justCollided = true;
    protected SpriteFont _font;
    protected bool _guiActive;
    //protected float forceDownSlope;
    // protected float displayedForceFromFriction;
    // float frictionForce;
    protected List<BoxObject> listOfBoxes;
    protected bool keyPressed;
    protected float[,] coefficientsMatrix;
    protected List<BoxObject> boxesToRemove;
    protected bool isPaused;
    protected bool spacePressed;
    protected bool timerActive;
    protected float messageTimer;

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
        public static (bool IsColliding, Vector2 MTV) CollisionData(Vector2[] CornersA, Vector2[] CornersB, Vector2[] axesOfNormals)
        {
            float smallestOverlap = float.PositiveInfinity;
            Vector2 smallestAxis = Vector2.Zero;
            foreach (var axis in axesOfNormals)
            {
                float[] projectionA = ProjectOntoAxis(CornersA, axis);
                float[] projectionB = ProjectOntoAxis(CornersB, axis);

                if (projectionA[1] < projectionB[0] || projectionB[1] < projectionA[0]) // if max of A is less than min of B, definitely not colliding
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

    public class SaveData
    {
        public List<BoxSave> Boxes { get; set; }
        public LineSave Line { get; set; }
    }

    public class BoxSave
    {
        public float CentreOfBoxX { get; set; }
        public float CentreOfBoxY { get; set; }
        public float Rotation { get; set; }
        public float Mass { get; set; }
        public float CoefficientOfFriction { get; set; }
        public bool CustomCoefficientToggle { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public int MaterialIndex { get; set; }
    }

    public class LineSave
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Rotation { get; set; }
        public int MaterialIndex { get; set; }
    }
    public void SaveConfig(List<BoxObject> boxes, LineObject line, int fileNumber)
    {
        var saveData = new SaveData
        {
            Boxes = boxes.Select(box => new BoxSave
            {
                CentreOfBoxX = box.GetCentreOfBox().X,
                CentreOfBoxY = box.GetCentreOfBox().Y,
                Rotation = box.GetRotation(),
                Mass = box.GetMass(),
                CoefficientOfFriction = box.GetCoefficientOfFriction(),
                CustomCoefficientToggle = box.GetCustomCoefficientToggle(),
                VelocityX = box.GetBoxVelocity().X,
                VelocityY = box.GetBoxVelocity().Y,
                MaterialIndex = box.GetIndexOfMaterialInMatrix()
            }).ToList(),
            Line = new LineSave
            {
                PositionX = line.GetPosition().X,
                PositionY = line.GetPosition().Y,
                Rotation = line.GetRotation(),
                MaterialIndex = line.GetIndexOfMaterialInMatrix()
            }
        };
        string jsonPath = AppDomain.CurrentDomain.BaseDirectory + $"\\saveData\\save{fileNumber}.json";
        string jsonData = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, jsonData);
    }
    public void LoadConfig(int saveNumber)
    {
        SaveData saveData = SetConfig(saveNumber);
        if (saveData == null || saveData.Line == null)
        {
            Console.WriteLine("Failed to load config.");
        }
        else
        {
            listOfBoxes.Clear();
            if (saveData.Boxes != null)
            {
                foreach (var boxSave in saveData.Boxes)
                {
                    BoxObject _box = boxSave.MaterialIndex switch
                    {
                        0 => new BrassBox(blackTexture, new Vector2(boxSave.CentreOfBoxX, boxSave.CentreOfBoxY), new Rectangle(0, 0, 100, 100)),
                        1 => new CastIronBox(blackTexture, new Vector2(boxSave.CentreOfBoxX, boxSave.CentreOfBoxY), new Rectangle(0, 0, 100, 100)),
                        2 => new IceBox(blackTexture, new Vector2(boxSave.CentreOfBoxX, boxSave.CentreOfBoxY), new Rectangle(0, 0, 100, 100)),
                        3 => new SteelBox(blackTexture, new Vector2(boxSave.CentreOfBoxX, boxSave.CentreOfBoxY), new Rectangle(0, 0, 100, 100)),
                        _ => new DefaultBox(blackTexture, new Vector2(boxSave.CentreOfBoxX, boxSave.CentreOfBoxY), new Rectangle(0, 0, 100, 100))
                    };

                    _box.SetRotation(boxSave.Rotation);
                    _box.SetMass(boxSave.Mass);
                    _box.SetCoefficientOfFriction(boxSave.CoefficientOfFriction);
                    _box.SetCustomCoefficientToggle(boxSave.CustomCoefficientToggle);
                    _box.SetBoxVelocity(new Vector2(boxSave.VelocityX, boxSave.VelocityY));

                    listOfBoxes.Add(_box);
                }
            }
            _line = saveData.Line.MaterialIndex switch
            {
                0 => new BrassLine(blackTexture, new Vector2(saveData.Line.PositionX, saveData.Line.PositionY), saveData.Line.Rotation, new Rectangle(0, 0, 800, 10)),
                1 => new CastIronLine(blackTexture, new Vector2(saveData.Line.PositionX, saveData.Line.PositionY), saveData.Line.Rotation, new Rectangle(0, 0, 800, 10)),
                2 => new IceLine(blackTexture, new Vector2(saveData.Line.PositionX, saveData.Line.PositionY), saveData.Line.Rotation, new Rectangle(0, 0, 800, 10)),
                3 => new SteelLine(blackTexture, new Vector2(saveData.Line.PositionX, saveData.Line.PositionY), saveData.Line.Rotation, new Rectangle(0, 0, 800, 10)),
                _ => new DefaultLine(blackTexture, new Vector2(saveData.Line.PositionX, saveData.Line.PositionY), saveData.Line.Rotation, new Rectangle(0, 0, 800, 10))
            };
            _line.SetOrigin(new Vector2(_line.GetBounds().Width / 2f, _line.GetBounds().Height / 2f));
            _line.SetRotation(saveData.Line.Rotation);
        }
    }
    public SaveData SetConfig(int fileNumber)
    {
        string jsonPath = AppDomain.CurrentDomain.BaseDirectory + $"\\saveData\\save{fileNumber}.json";
        string jsonData = File.ReadAllText(jsonPath);
        var saveData = JsonSerializer.Deserialize<SaveData>(jsonData);
        return saveData;
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
        guiRenderer = new ImGuiRenderer(this);
        _guiActive = true;
        coefficientsMatrix = new float[5, 5];
        keyPressed = false;
        spacePressed = false;
        isPaused = false;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _font = Content.Load<SpriteFont>("File");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        blackTexture = Content.Load<Texture2D>("blacksquare");
        arrowTexture = Content.Load<Texture2D>("arrow");
        arrow = new Rectangle[3];
        for (int i = 0; i < 3; i++)
        {
            arrow[i] = new Rectangle(0, 0, 50, 100);
        }
        coefficientsMatrix[0, 0] = 0.5f;
        coefficientsMatrix[0, 1] = 0.3f;
        coefficientsMatrix[0, 2] = 0.02f;
        coefficientsMatrix[0, 3] = 0.44f;
        coefficientsMatrix[0, 4] = 0.5f;
        coefficientsMatrix[1, 0] = 0.3f;
        coefficientsMatrix[1, 1] = 0.15f;
        coefficientsMatrix[1, 2] = 0.03f;
        coefficientsMatrix[1, 3] = 0.23f;
        coefficientsMatrix[1, 4] = 0.5f;
        coefficientsMatrix[2, 0] = 0.02f;
        coefficientsMatrix[2, 1] = 0.04f;
        coefficientsMatrix[2, 2] = 0.01f;
        coefficientsMatrix[2, 3] = 0.05f;
        coefficientsMatrix[2, 4] = 0.5f;
        coefficientsMatrix[3, 0] = 0.44f;
        coefficientsMatrix[3, 1] = 0.23f;
        coefficientsMatrix[3, 2] = 0.06f;
        coefficientsMatrix[3, 3] = 0.42f;
        coefficientsMatrix[3, 4] = 0.5f;
        coefficientsMatrix[4, 0] = 0.5f;
        coefficientsMatrix[4, 1] = 0.5f;
        coefficientsMatrix[4, 2] = 0.5f;
        coefficientsMatrix[4, 3] = 0.5f;
        coefficientsMatrix[4, 4] = 0.5f;
        listOfBoxes = new List<BoxObject>();
        boxesToRemove = new List<BoxObject>();
        _line = new DefaultLine(blackTexture, new Vector2(200, 400), 0, new Rectangle(0, 0, 800, 10));
        _line.SetOrigin(new Vector2(_line.GetBounds().Width / 2f, _line.GetBounds().Height / 2f));
        guiRenderer.RebuildFontAtlas();
        timerActive = false;
    }
    public void SpawnBox(int inputInt)
    {
        switch (inputInt)
        {
            case 0:
                listOfBoxes.Add(new BrassBox(blackTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, 0),
                 new Rectangle(0, 0, 100, 100)));
                break;
            case 1:
                listOfBoxes.Add(new CastIronBox(blackTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, 0),
                 new Rectangle(0, 0, 100, 100)));
                break;
            case 2:
                listOfBoxes.Add(new IceBox(blackTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, 0),
                 new Rectangle(0, 0, 100, 100)));
                break;
            case 3:
                listOfBoxes.Add(new SteelBox(blackTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, 0),
                 new Rectangle(0, 0, 100, 100)));
                break;
            case 4:
                listOfBoxes.Add(new DefaultBox(blackTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, 0),
                 new Rectangle(0, 0, 100, 100)));
                break;
        }
    }
    public Vector2[] GetAxes()
    {
        Vector2[] axes = new Vector2[]
                {
            new Vector2((float)Math.Cos(lineRotation), (float)Math.Sin(lineRotation)),
            new Vector2(-(float)Math.Sin(lineRotation), (float)Math.Cos(lineRotation)),
            new Vector2(1,0),
            new Vector2(0,1)
                };
        return axes;
    }
    public Vector2 GetTangent(Vector2 normal)
    {
        return new Vector2(-normal.Y, normal.X);
    }
    protected override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboardState = Keyboard.GetState();

        // Toggle pause with space
        if (keyboardState.IsKeyDown(Keys.Space) && !spacePressed)
        {
            isPaused = !isPaused;
            spacePressed = true;
        }
        else if (keyboardState.IsKeyUp(Keys.Space))
        {
            spacePressed = false;
        }

        if (!keyPressed && keyboardState.IsKeyDown(Keys.G))
        {
            SpawnBox(0);
            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.H))
        {
            SpawnBox(1);
            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.J))
        {
            SpawnBox(2);
            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.K))
        {
            SpawnBox(3);
            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.L))
        {
            SpawnBox(4);
            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.D))
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\saveData\\save0.json";

            if (File.Exists(path))
            {
                LoadConfig(0);
            }
            else
            {
                timerActive = true;
                messageTimer = 3f;
            }

            keyPressed = true;
        }
        if (!keyPressed && keyboardState.IsKeyDown(Keys.S))
        {
            SaveConfig(listOfBoxes, _line, 0);
            keyPressed = true;
        }
        else if (keyboardState.IsKeyUp(Keys.G) && keyboardState.IsKeyUp(Keys.H) && keyboardState.IsKeyUp(Keys.J) && keyboardState.IsKeyUp(Keys.K) && keyboardState.IsKeyUp(Keys.L))
        { keyPressed = false; }
        lineRotation = _line.GetRotation();
        _line.Update(Window);
        foreach (BoxObject _box in listOfBoxes)
        {
            if (!keyPressed && keyboardState.IsKeyDown(Keys.E))
            {
                _box.SetRotation(lineRotation);
            }
            if (!keyPressed && keyboardState.IsKeyDown(Keys.Q))
            {
                _box.SetRotation(0f);
            }
            if (mouse.RightButton == ButtonState.Pressed && _box.BoundingBox.Contains(mouse.Position))
            {
                boxesToRemove.Add(_box);
            }
            if (_box.GetCustomCoefficientToggle() == false)
            {
                _box.SetCoefficientOfFriction(coefficientsMatrix[_box.GetIndexOfMaterialInMatrix(), _line.GetIndexOfMaterialInMatrix()]);
            }

            _box.Update(Window, isPaused);

            if (!isPaused)
            {
                var result = SATHelper.CollisionData(_box.GetCorners(), _line.GetCorners(), GetAxes());
                if (_box.GetBoxVelocity().Length() < 0.5)
                {
                    _box.SetJustCollided(false);
                }
                if (result.IsColliding)
                {
                    Vector2 mtv = result.MTV;
                    if (Vector2.Dot(mtv, _box.GetCentreOfBox() - _line.GetPosition()) < 0)
                    {
                        mtv = -mtv;
                    }
                    if (mtv.Length() > 0.001f)
                    {
                        _box.SetNormal(Vector2.Normalize(mtv));
                    }
                    _box.SetTangent(new Vector2(-_box.GetNormal().Y, _box.GetNormal().X));
                    _box.SetVelAlongNormal(Vector2.Dot(_box.GetBoxVelocity(), _box.GetNormal()));
                    bool flatOnSurface = Math.Abs(_box.GetVelAlongNormal()) < 0.05f && Math.Abs(_box.GetAngularVelocity()) < 0.1f;
                    if (mtv.LengthSquared() < 10000) // Prevent huge translations from NaN
                    {
                        _box.Translate(mtv);
                    }
                    if (_box.GetVelAlongNormal() < 0)
                    {
                        _box.SetBoxVelocity(_box.GetBoxVelocity() - _box.GetNormal() * _box.GetVelAlongNormal());
                    }
                    _box.SetNormalReactionForce((float)(_box.GetForceFromGravity() * Math.Abs(Math.Cos(lineRotation))));
                    _box.SetMaxForceFromFriction(_box.GetNormalReactionForce() * _box.GetCoefficientOfFriction());
                    _box.SetVelAlongTangent(Vector2.Dot(_box.GetBoxVelocity(), _box.GetTangent()));
                    _box.ApplyFriction(lineRotation, displayedForcesScale);
                    Vector2 contactPoint;
                    if (flatOnSurface)
                    {
                        contactPoint = _box.GetCentreOfBox();
                    }
                    else
                    {
                        Vector2[] corners = _box.GetCorners();
                        contactPoint = corners[0];

                        float maxProjection = Vector2.Dot(corners[0], _box.GetNormal());
                        foreach (var corner in corners)
                        {
                            float point = Vector2.Dot(corner, _box.GetNormal());
                            if (point > maxProjection)
                            {
                                maxProjection = point;
                                contactPoint = corner;
                            }
                        }
                    }
                    Vector2 collisionForce = _box.GetNormal() * _box.GetNormalReactionForce();
                    Vector2 leverArm = contactPoint - _box.GetCentreOfBox();
                    _box.SetTorque(leverArm.X * collisionForce.Y - leverArm.Y * collisionForce.X);
                    if (flatOnSurface)
                    {
                        _box.SetAngularVelocity(0f);
                        _box.SetTorque(0f);
                    }
                    float angleDifference = Math.Abs(_box.GetRotation() - lineRotation);
                    _box.SetAngularAcceleration(_box.GetTorque() / (_box.GetMomentOfInertia() * 2));
                    if (_box.GetJustCollided() == false)
                    {
                        if (angleDifference > MathHelper.ToRadians(3f))
                        {
                            _box.SetAngularVelocity(_box.GetAngularVelocity() + _box.GetAngularAcceleration());
                        }

                        _box.SetJustCollided(true);
                    }
                    // stop rotation if the box is nearly aligned with the slope
                    if (angleDifference < MathHelper.ToRadians(3f))
                    {
                        _box.SetRotation(lineRotation);
                        _box.SetAngularVelocity(0f);
                    }
                    else
                    {
                        _box.SetAngularVelocity(_box.GetAngularVelocity() + _box.GetAngularAcceleration());
                    }
                }
                else
                {
                    _box.SetAffectOfGravity(true);
                    _box.SetAngularVelocity(_box.GetAngularVelocity() + _box.GetAngularAcceleration());
                }
            }
        }
        foreach (BoxObject box in boxesToRemove)
        {
            listOfBoxes.Remove(box);
        }
        boxesToRemove.Clear();
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.MintCream);
        _spriteBatch.Begin();
        _line.Draw(_spriteBatch);
        foreach (BoxObject _box in listOfBoxes)
        {
            _box.Draw(_spriteBatch);
            _spriteBatch.Draw(
            arrowTexture,
                _box.GetCentreOfBox(),
                null,
               Color.Red,
               MathHelper.ToRadians(180f),
               new Vector2(arrowTexture.Width / 2, radius),
                  0.05f,
               SpriteEffects.None,
                 0f
                 );
            _spriteBatch.Draw(
            arrowTexture,
            _box.GetCentreOfBox(),
            null,
                Color.Blue,
                lineRotation + MathHelper.ToRadians(90f) * -Math.Sign(_box.GetVelAlongTangent()),
                new Vector2(arrowTexture.Width / 2, radius),
                0.05f,
                SpriteEffects.None,
                0f
            );
            _spriteBatch.Draw(
                arrowTexture,
                _box.GetCentreOfBox(),
                null,
                Color.Green,
                lineRotation,
                new Vector2(arrowTexture.Width / 2, radius),
                0.05f,
                SpriteEffects.None,
                0f
            );
            _spriteBatch.DrawString(_font, $"{listOfBoxes.IndexOf(_box) + 1}", new Vector2((int)_box.GetCentreOfBox().X - (_box.GetWidth()) / 2 - 10, (int)_box.GetCentreOfBox().Y - _box.GetHeight() / 2 - 20), Color.Black);
            _spriteBatch.DrawString(_font, $"{_box.GetDisplayedForceFromGravity():F2}N", new Vector2((int)_box.GetCentreOfBox().X + 10, (int)(_box.GetCentreOfBox().Y + _box.GetHeight())), Color.Red);
            _spriteBatch.DrawString(_font, $"{_box.GetDisplayedForceFromFriction():F2}N", new Vector2((int)_box.GetCentreOfBox().X - _box.GetWidth(), (int)_box.GetCentreOfBox().Y), Color.Blue);
            _spriteBatch.DrawString(_font, $"{_box.GetDisplayedNormalForce():F2}N", new Vector2((int)_box.GetCentreOfBox().X - 10, (int)_box.GetCentreOfBox().Y - _box.GetHeight()+30), Color.Green);
        }
        if (timerActive)
        {
            messageTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spriteBatch.DrawString(_font, "No save file found in slot 1!",
                new Vector2(10, 20), Color.Red);
            if (messageTimer <= 0f)
            {
                timerActive = false;
            }
        }

        _spriteBatch.End();
        base.Draw(gameTime);
        guiRenderer.BeginLayout(gameTime);
        if (_guiActive)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Boxes"))
                {
                    foreach (BoxObject _box in listOfBoxes)
                    {
                        if (ImGui.BeginMenu($"Box {listOfBoxes.IndexOf(_box) + 1}"))
                        {
                            float massValue = _box.GetMass();
                            ImGui.InputFloat("Mass", ref massValue, 1f, 10f, "%.2f");
                            _box.SetMass(massValue);
                            bool customToggle = _box.GetCustomCoefficientToggle();
                            ImGui.Checkbox("Unlock Coefficient of Friction", ref customToggle);
                            _box.SetCustomCoefficientToggle(customToggle);

                            ImGui.BeginDisabled(!_box.GetCustomCoefficientToggle());
                            float coeffValue = _box.GetCoefficientOfFriction();
                            ImGui.SliderFloat("Coefficient of friction", ref coeffValue, 0.0f, 1.0f);
                            _box.SetCoefficientOfFriction(coeffValue);
                            ImGui.EndDisabled();
                            ImGui.EndMenu();
                        }
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Surface"))
                {
                    float rotationDegrees = MathHelper.ToDegrees(lineRotation);
                    ImGui.InputFloat("Rotation (degrees)", ref rotationDegrees, 1f, 10f, "%.2f");
                    rotationDegrees = rotationDegrees % 360f;
                    lineRotation = MathHelper.ToRadians(rotationDegrees);
                    _line.SetRotation(lineRotation);
                    int selected = 0;
                    if (ImGui.ListBox("Surface Material", ref selected, new string[] { "Brass", "Cast Iron", "Ice", "Steel", "Default" }, 5))
                    {
                        switch (selected)
                        {
                            case 0:
                                _line = new BrassLine(blackTexture, _line.GetPosition(), lineRotation, new Rectangle(0, 0, 800, 10));
                                break;
                            case 1:
                                _line = new CastIronLine(blackTexture, _line.GetPosition(), lineRotation, new Rectangle(0, 0, 800, 10));
                                break;
                            case 2:
                                _line = new IceLine(blackTexture, _line.GetPosition(), lineRotation, new Rectangle(0, 0, 800, 10));
                                break;
                            case 3:
                                _line = new SteelLine(blackTexture, _line.GetPosition(), lineRotation, new Rectangle(0, 0, 800, 10));
                                break;
                            case 4:
                                _line = new DefaultLine(blackTexture, _line.GetPosition(), lineRotation, new Rectangle(0, 0, 800, 10));
                                break;
                        }
                        _line.SetOrigin(new Vector2(_line.GetBounds().Width / 2f, _line.GetBounds().Height / 2f));
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Saved Configs"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (ImGui.BeginMenu($"File {i + 1}"))
                        {
                            if (ImGui.Button("Save Config")) { SaveConfig(listOfBoxes, _line, i); }
                            ImGui.BeginDisabled(!File.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\saveData\\save{i}.json"));
                            if (ImGui.Button("Load Config")) { LoadConfig(i); }
                            ImGui.EndDisabled();
                            ImGui.EndMenu();
                        }

                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Info"))
                {
                    ImGui.Text("This program is used to simulate physics of a box dragging against and inclined plane");
                    ImGui.Text("Hotkeys: ");
                    ImGui.Text("Move box: Left Click");
                    ImGui.Text("Delete box: Right Click");
                    ImGui.Text("Spawn box: G,H,J,K,L (Brass, Cast Iron, Steel, Ice, None)");
                    ImGui.Text("Rotate line: O, P");
                    ImGui.Text("Move line: Arrow keys");
                    ImGui.Text("Pause simulation: Space");
                    ImGui.Text("Quick save slot 1: S");
                    ImGui.Text("Quick load slot 1: D");
                    ImGui.Text("Align box with slope: E");
                    ImGui.Text("Reset rotation of box: Q");
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            ImGui.End();
            ImGui.Begin("Box Stats");
            foreach (BoxObject _box in listOfBoxes)
            {
                ImGui.Text($"Box {listOfBoxes.IndexOf(_box) + 1} Speed: {_box.GetBoxVelocity().Length() * speedDisplayScale:F2}");
                ImGui.Text($"Box {listOfBoxes.IndexOf(_box) + 1} Force Down the slope: {_box.GetForceDownSlope() * displayedForcesScale:F2}");
                //ImGui.Text($"Box {listOfBoxes.IndexOf(_box) + 1} MaxForcefromfriction: {_box.GetMaxForceFromFriction():F2}");
            }
            ImGui.End();
        }
        guiRenderer.EndLayout();
    }
}

