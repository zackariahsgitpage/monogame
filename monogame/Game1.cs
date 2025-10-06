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
    Rectangle _LineObject;
    float tempVelocity;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        
        base.Initialize();  
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BlackTexture = Content.Load<Texture2D>("blacksquare");
        _box = new BoxObject(_BlackTexture, new Vector2(0, 0));
        _LineObject = new Rectangle(0, 400, 400, 10);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
       _box.Update(Window);

        if (_box.BoundingBox.Intersects(_LineObject))
        {
            _box.verticalVelocity = 0;
            _box.normalReactionForce = _box.gravity;
            _box.forceFromFriction = (_box.normalReactionForce * _box.coefficientOfFriction) / 10;
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
        //_spriteBatch.Draw(_BlackTexture,new Vector2(0,400),_LineObject,Color.Black, 0, new Vector2(_LineObject.Width/2,_LineObject.Height/2),1.0f,SpriteEffects.None,0f);
        _spriteBatch.Draw(_BlackTexture, _LineObject, Color.Black);
       _box.Draw(_spriteBatch);
        _spriteBatch.End();
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
