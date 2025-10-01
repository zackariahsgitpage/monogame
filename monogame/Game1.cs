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
    Texture2D _BlackTexture;
    Rectangle _BoxObject;
    Rectangle _LineObject;
    decimal gravity;
    decimal verticalVelocity;
    decimal horizontalVelocity;
    float rotation;
    Vector2 mousePointBeforeRelease;
    Vector2 mousePointAfterRelease;
    Vector2 boxPosition;
    bool edgeTrigger;
    

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        gravity = 20;
        verticalVelocity = 0;
        horizontalVelocity = 0;
        edgeTrigger = false;
        rotation = 0;
        boxPosition= new Vector2(0,0);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BlackTexture = Content.Load<Texture2D>("blacksquare");
        _BoxObject = new Rectangle(0, 0, 100, 100);
        _LineObject = new Rectangle(0, 400, 800, 10);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            boxPosition.X = mouse.X ;
            boxPosition.Y = mouse.Y ;
            mousePointBeforeRelease.X = mouse.X;
            mousePointBeforeRelease.Y = mouse.Y;
            edgeTrigger = true;
        }
        if (mouse.LeftButton == ButtonState.Released && edgeTrigger)
        {
            mousePointAfterRelease.X = mouse.X;
            mousePointAfterRelease.Y = mouse.Y;
            verticalVelocity += (decimal)(mousePointAfterRelease.Y - mousePointBeforeRelease.Y)/5;
            horizontalVelocity += (decimal)(mousePointAfterRelease.X - mousePointBeforeRelease.X)/5;
            edgeTrigger = false;
        }
        if (boxPosition.Y + _BoxObject.Height/2 < this.Window.ClientBounds.Height && mouse.LeftButton == ButtonState.Released)
        {
            verticalVelocity += (gravity / 60);
            if (verticalVelocity < 0 && (boxPosition.Y-_BoxObject.Height/2)< 0)
            {
                verticalVelocity = 0;
            }
                boxPosition.Y += (int)verticalVelocity;
            }
        else
        {
            verticalVelocity = 0;
        }
        if (boxPosition.X + _BoxObject.Width/2 < this.Window.ClientBounds.Width && boxPosition.X - _BoxObject.Width/2 > 0 && mouse.LeftButton == ButtonState.Released)
        {
            boxPosition.X += (int)horizontalVelocity;
        }
        else
        {
            horizontalVelocity = 0;
        }
        

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_BlackTexture,new Vector2(0,400),_LineObject,Color.Black, rotation, new Vector2(_LineObject.Width/2,_LineObject.Height/2),1.0f,SpriteEffects.None,0f);
        _spriteBatch.Draw(_BlackTexture,boxPosition,_BoxObject,Color.Black, rotation, new Vector2(_BoxObject.Width/2,_BoxObject.Height/2),1.0f,SpriteEffects.None,0f);
        _spriteBatch.End();
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
