using System;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame;


public class BoxObject
{
    const float GRAVITY_SCALE = 0.075f;
    const float BOX_SCALE = 1f;
    const float displayedNormalForceScale = 10f;
    protected Texture2D texture;
    protected Rectangle bounds;
    protected Vector2 centreOfBox;
    protected Vector2 mousePointBeforeRelease;
    protected Vector2 mousePointAfterRelease;
    protected float angularVelocity;
    protected float momentOfInertia;
    protected float gravity;
    protected bool affectOfGravity;
    protected float forceFromGravity;
    
    protected Vector2 boxVelocity;
    protected float maxForceFromFriction;
    protected float coefficientOfFriction;

    protected float rotation;
    protected bool mouseHeld;
    private float resolvedSpeed;
    protected float normalReactionForce;
    protected float mass;

    protected float angularAcceleration;
    protected float torque;
    protected float accelerationFromGravity;
    protected float displayedForceFromGravity;
    protected bool boxHeld;
    protected Color colour;
    protected bool customCoefficientToggle;
    protected Vector2 tangent;
    protected Vector2 normal;
    protected float velAlongNormal;
    protected float velAlongTangent;
    protected float displayedForceFromFriction;
    protected bool justCollided;
    protected float forceDownSlope;
    protected float forceFromFriction;
    public bool GetJustCollided() {return justCollided;}
    public void SetJustCollided(bool input) {justCollided = input;}
    public float GetDisplayedForceFromFriction() {return displayedForceFromFriction;}
    public float GetDisplayedNormalForce() {return normalReactionForce/GRAVITY_SCALE;}
    public Vector2 GetCentreOfBox() {return centreOfBox;}
    public Vector2 GetBoxVelocity() {return boxVelocity;}
    public void SetBoxVelocity(Vector2 inputVelocity) {boxVelocity = inputVelocity;}
    public float GetRotation() {return rotation;}
    public void SetRotation(float inputRotation) {rotation = inputRotation;}
    public float GetAngularVelocity() {return angularVelocity;}
    public void SetAngularVelocity(float inputAngularVelocity) {angularVelocity = inputAngularVelocity;}
    public float GetMass() {return mass;}
    public void SetMass(float inputMass) {mass = inputMass;}
     public int GetWidth() {return bounds.Width;}
    public int GetHeight() {return bounds.Height;}
    public float GetNormalReactionForce() {return normalReactionForce;}
    public void SetNormalReactionForce(float inputForce) {normalReactionForce = inputForce;}
    public float GetCoefficientOfFriction() {return coefficientOfFriction;}
    public void SetCoefficientOfFriction(float inputCoefficient) {coefficientOfFriction = inputCoefficient;}
    public void SetNormal(Vector2 inputNormal) {normal = inputNormal;}
    public Vector2 GetNormal() {return normal;}
    public void SetTangent(Vector2 inputTangent) {tangent = inputTangent;}
    public Vector2 GetTangent() {return tangent;}
    public void SetVelAlongNormal(float inputVelAlongNormal) {velAlongNormal = inputVelAlongNormal;}
    public float GetVelAlongNormal() {return velAlongNormal;}
    public void SetVelAlongTangent(float inputVelAlongTangent) {velAlongTangent = inputVelAlongTangent;}
    public float GetVelAlongTangent() {return velAlongTangent;}
    public float GetMaxForceFromFriction() {return maxForceFromFriction;}
    public void SetMaxForceFromFriction(float input) {maxForceFromFriction = input;}
    public float GetForceFromGravity() {return forceFromGravity;}
    public void SetForceFromGravity(float input) {forceFromGravity = input;}
    public bool GetAffectOfGravity() {return affectOfGravity;}
    public void SetAffectOfGravity(bool input) {affectOfGravity = input;}
    public float GetAngularAcceleration() {return angularAcceleration;}
    public void SetAngularAcceleration(float input) {angularAcceleration = input;}
    public float GetTorque() {return torque;}
    public void SetTorque(float input) {torque = input;}
    public float GetMomentOfInertia() {return momentOfInertia;}
    public bool GetCustomCoefficientToggle() {return customCoefficientToggle;}
    public void SetCustomCoefficientToggle(bool input) {customCoefficientToggle = input;}
    public float GetDisplayedForceFromGravity() {return displayedForceFromGravity;}
    public float GetForceDownSlope() {return forceDownSlope;}
    public BoxObject(Texture2D texture, Vector2 initialPosition, Rectangle sourceRectangle)
    {
        this.texture = texture;
        bounds = sourceRectangle;
        centreOfBox = initialPosition;
        gravity = 9.8f;
        mouseHeld = false;
        rotation = 0f;
        resolvedSpeed = 0;
        normalReactionForce = 0f;
        mass = 1f;
        torque = 0f;
        angularVelocity = 0f;
        boxVelocity = new Vector2(0,0);
        affectOfGravity = true;
        boxHeld = false;
        accelerationFromGravity = GRAVITY_SCALE * gravity;
        colour = Color.Black;
    }

    public void Update(GameWindow window, bool isPaused = false)
    {
        boxHeld = false;
        displayedForceFromGravity = mass*gravity;
        momentOfInertia = mass*(bounds.Width*bounds.Width + bounds.Height*bounds.Height)/12;
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();
        resolvedSpeed = boxVelocity.Length();
        centreOfBox = new Vector2(centreOfBox.X, centreOfBox.Y);
        if (mouse.LeftButton == ButtonState.Pressed && (BoundingBox.Contains(mouse.Position) || mouseHeld) )
        {
            centreOfBox = mouse.Position.ToVector2();
            mousePointBeforeRelease.X = mouse.X;
            mousePointBeforeRelease.Y = mouse.Y;
            mouseHeld = true;
            angularAcceleration = 0f;
            boxHeld = true;
        }
        if (mouse.LeftButton == ButtonState.Released && mouseHeld)
        {
            mousePointAfterRelease.X = mouse.X;
            mousePointAfterRelease.Y = mouse.Y;
            boxVelocity+= new Vector2((mousePointAfterRelease.X - mousePointBeforeRelease.X)/5,(mousePointAfterRelease.Y - mousePointBeforeRelease.Y)/5);
            mouseHeld = false;
            angularVelocity=0;
            angularAcceleration=0;
            torque=0;
            affectOfGravity = true;
        }

        if (!isPaused)
        {
        if (centreOfBox.Y + bounds.Height / 2 < window.ClientBounds.Height && !mouseHeld)
        {
            {
                forceFromGravity = mass * (gravity)*GRAVITY_SCALE;
                if (affectOfGravity)
                {
                boxVelocity.Y += accelerationFromGravity;
            }
            }
            if (boxVelocity.Y < 0 && (centreOfBox.Y - bounds.Height / 2) < 0)
            {
                boxVelocity.Y = 0;
            }
           if (affectOfGravity)
           {
            centreOfBox.Y += boxVelocity.Y;
           }
        }
        else
        {
            boxVelocity.Y = 0;
        }

        if (centreOfBox.X + bounds.Width / 2 < window.ClientBounds.Width &&
            centreOfBox.X - bounds.Width / 2 > 0 &&
            !boxHeld)
        {
            centreOfBox.X += boxVelocity.X;
        }
        else
        {
            boxVelocity.X = 0;
        }
    angularVelocity *= 0.9f; 
    rotation-= angularVelocity;
        }
    
    }
    public void ApplyFriction(float lineRotation, float displayedFrictionScale, Vector2 linePosition)
    {
        forceDownSlope = (float)(forceFromGravity * Math.Abs(Math.Sin(lineRotation)));
                    if (forceDownSlope <= maxForceFromFriction && Math.Abs(velAlongTangent) < 0.5f
                      && boxVelocity.Length() < 1f && _centreOfBox.Y < linePosition.Y)
                    {
                        boxVelocity = new Vector2(0f, 0f);
                        affectOfGravity = false; // Disable gravity when friction holds the box  
                    }
                    else
                    {
                        affectOfGravity = true; // Re-enable gravity when friction can't hold the box
                        if (!float.IsNaN(velAlongTangent))
                        {
                            forceFromFriction = maxForceFromFriction;
                            float frictionAcceleration = forceFromFriction / mass;
                            boxVelocity = boxVelocity - tangent * Math.Sign(velAlongTangent) * frictionAcceleration;
                        }
                    }
                     if (maxForceFromFriction > forceDownSlope)
    {
        displayedForceFromFriction = forceDownSlope * displayedFrictionScale;
    }
    else
    {
        displayedForceFromFriction = maxForceFromFriction * displayedFrictionScale;
    }
    }
    public void Translate(Vector2 vectorToTranslateBy)
    {
        centreOfBox += vectorToTranslateBy;
    }
    public Vector2[] GetCorners()
    {
        float halfWidth = bounds.Width *BOX_SCALE / 2f;
        float halfHeight = bounds.Height *BOX_SCALE / 2f;
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
            worldCorners[i] = centreOfBox + new Vector2(
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
                (int)(centreOfBox.X - bounds.Width / 2), //left side
                (int)(centreOfBox.Y - bounds.Height / 2),//top side
                bounds.Width, //right side
                bounds.Height //bottom side
            );
        }
        
    }
    public virtual int GetIndexOfMaterialInMatrix()
        {
            return -1;
        }
         public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture,
            centreOfBox,
            bounds,
            colour,
            rotation,
            new Vector2(bounds.Width / 2, bounds.Height / 2),
            1.0f,
            SpriteEffects.None,
            0f
        );
    }
}
    public class BrassBox : BoxObject
    {
        public BrassBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.Gold;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 0;
        }
    }
    public class CastIronBox : BoxObject
    {
        public CastIronBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.Gray;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 1;
        }
    }
    public class IceBox : BoxObject
    {
        public IceBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.LightBlue;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 2;
        }
    }
    public class SteelBox : BoxObject
    {
        public SteelBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.LightGray;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 3;
        }
    }
    public class DefaultBox : BoxObject
    {
        public DefaultBox(Texture2D texture, Vector2 centreOfBox, Rectangle sourceRectangle) 
        : base(texture, centreOfBox, sourceRectangle)
        {
            colour = Color.Black;
        }
        public override int GetIndexOfMaterialInMatrix()
        {
            return 4;
        }
    }





