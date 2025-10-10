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
        lineRotation = -MathHelper.ToRadians(60);
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BlackTexture = Content.Load<Texture2D>("blacksquare");
        _box = new BoxObject(_BlackTexture, new Vector2(0, 0));
        _line = new LineObject(_BlackTexture, new Vector2(200,400), new Rectangle(0, 0, 400, 10), lineRotation);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
       _box.Update(Window);

        if (_box.BoundingBox.Intersects(_line.BoundingBox))
        {
            _box.verticalVelocity = 0;
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
        _spriteBatch.Draw(
             _BlackTexture,
            positionOfLine,
            _line.SourceRect,
            Color.Black,
            lineRotation,
            new Vector2(0,0),
            1.0f,
            SpriteEffects.None,
            0f);
       _box.Draw(_spriteBatch);
        _spriteBatch.End();
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
