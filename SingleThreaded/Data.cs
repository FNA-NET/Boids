using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace FNA.Boids
{
    public static class Data
    {
        public static GraphicsDeviceManager GDM;
        public static ContentManager CM;
        public static SpriteBatch SB;

        public static Texture2D Assets_mainSheet;
        public static SpriteFont Assets_font;

        //sorting layers
        public static float Layer_Bkg = 0.999990f; //back
        public static float Layer_Cloth = 0.999989f;
        public static float Layer_Obj = 0.999988f;
        public static float Layer_Player = 0.999987f;
        public static float Layer_Particle = 0.999986f; //front
        //ui layers
        public static float Layer_UI_Bkg = 0.000006f;
        public static float Layer_UI_Fore = 0.000005f;
        //debug layer
        public static float Layer_Debug = 0.000001f; //very top
    }





    #region Sprite Component Class

    public class ComponentSprite
    {   //visual representation of object/actor/projectile
        public Texture2D texture;
        public float X, Y = 0;
        public AnimFrame currentFrame;
        //flip vertically, flip horizontally, none
        public SpriteEffects spriteEffect;
        public Boolean flipHorizontally;
        public Boolean visible;
        public Vector2 origin;
        public Rectangle drawRec;
        public Color drawColor;
        public float alpha;
        public float scale;
        public float layer = Data.Layer_Bkg;
        public float rotationValue = 0.0f;

        public ComponentSprite(
            Texture2D Texture,
            float x, float y,
            AnimFrame CurrentFrame,
            Point CellSize)
        {
            texture = Texture; X = x; Y = y;
            currentFrame = CurrentFrame;
            spriteEffect = SpriteEffects.None;
            flipHorizontally = false;
            visible = true;
            drawRec = new Rectangle(
                (int)X, (int)Y,
                CellSize.X, CellSize.Y);
            Functions.Sprite_CenterOrigin(this);
            drawColor = new Color(255, 255, 255);
            alpha = 1.0f;
            scale = 1.0f;
        }
    }

    #endregion


    #region Animation Component Class

    public class AnimFrame
    {   //used for animation
        public byte X, Y; //frame in sprite sheet X,Y
        public Boolean flipHori;
        public AnimFrame(byte x, byte y, Boolean Flip)
        { X = x; Y = y; flipHori = Flip; }
    }

    public class ComponentAnimation
    {
        public byte counter, speed, index = 0;
        public List<AnimFrame> Frames; //points to animList
    }

    #endregion


    #region Text Component Class

    public class ComponentText
    {
        public SpriteFont font;
        public String text;             //the string of text to draw
        public Vector2 position;        //the position of the text to draw
        public Color color;             //the color of the text to draw
        public float alpha = 1.0f;      //the opacity of the text
        public float rotation = 0.0f;
        public float scale = 1.0f;
        public float zDepth = 0.001f;   //the layer to draw the text to

        public ComponentText(String Text,
            Vector2 Position, Color Color)
        {
            position = Position;
            text = Text;
            color = Color;
            font = Data.Assets_font;
        }
    }

    #endregion




    public static class BoidPool
    {
        //drawing fields
        public static int size = 512 + 256;
        static int i, k;
        public static List<AnimFrame> birdAnim = new List<AnimFrame>();
        public static ComponentSprite[] sprites = new ComponentSprite[size];
        public static List<ComponentAnimation> anims = new List<ComponentAnimation>();

        //sim fields
        public static Vector2[] positions = new Vector2[size];
        public static Vector2[] velocities = new Vector2[size];
        public static Vector2[] accelerations = new Vector2[size];
        
        public static Vector2 maxSpeed = new Vector2(2, 2);
        public static Vector2 maxForce = new Vector2(0.03f, 0.03f);
        public static float startVelocity = 0.001f;

        //used in update
        public static Vector2 cohesionVector = new Vector2();
        public static Vector2 alignmentVector = new Vector2();
        public static Vector2 separationVector = new Vector2();

        public static float desiredSeparation = 25.0f;
        public static Vector2 alignmentSum = new Vector2(0, 0);
        public static Vector2 cohesionSum  = new Vector2(0, 0);

        public static int separationCount = 0;
        public static int alignmentCount = 0;
        public static int cohesionCount = 0;

        public static float neighbordist_align = 50;
        public static float neighbordist_cohese = 50;
        public static Vector2 desired = new Vector2();
        public static Vector2 distanceDiff = new Vector2();
        public static Vector2 cohesionDiff = new Vector2();


        //public static MouseState currentMouseState = new MouseState();
        public static Vector2 cursorPos = new Vector2(0, 0);
        public static float cursorDistance = 150;
        public static float pushX, pushY, distance;
        public static Vector2 pushVector = new Vector2();


        public static void Init()
        {
            //setup birds animation
            birdAnim.Add(new AnimFrame(1, 0, false));
            birdAnim.Add(new AnimFrame(2, 0, false));
            birdAnim.Add(new AnimFrame(3, 0, false));
            birdAnim.Add(new AnimFrame(4, 0, false));
            //setup boid poool
            for (i = 0; i < size; i++)
            {
                //get a random position on screen
                positions[i] = new Vector2(
                    Functions.Rand.Next(0, 1024),
                    Functions.Rand.Next(0, 512));
                //velocity cant be zero, but should be almost zero
                velocities[i] = new Vector2(startVelocity, startVelocity);
                accelerations[i] = new Vector2(0, 0);
                //setup sprite using random pos
                sprites[i] = new ComponentSprite(Data.Assets_mainSheet,
                    positions[i].X, positions[i].Y,
                    birdAnim[0], new Point(16, 32));
                //setup sprite anims
                ComponentAnimation anim = new ComponentAnimation();
                anim.Frames = birdAnim;
                anim.speed = 7;
                //randomize starting anim for variety
                anim.index = (byte)Functions.Rand.Next(0, 4);
                anims.Add(anim);
            }
        }

        public static void Update()
        {
            //currentMouseState = Mouse.GetState();
            //cursorPos.X = currentMouseState.X;
            //cursorPos.Y = currentMouseState.Y;

            cursorPos.X = 1024 / 2;
            cursorPos.Y = 512 / 2;

            for (i = 0; i < size; i++)
            {
                //reset values
                separationCount = 0;
                alignmentCount = 0;
                cohesionCount = 0;

                alignmentSum.X = 0; alignmentSum.Y = 0;
                cohesionSum.X = 0; cohesionSum.Y = 0;


                #region Do distance check once, collecting values needed

                for (k = 0; k < size; k++)
                {
                    distance = Vector2.Distance(positions[i], positions[k]);
                    //remove all distances less than 0
                    if(distance <= 0) { continue; }
                    
                    //separation
                    if (distance < desiredSeparation)
                    {
                        distanceDiff = Vector2.Subtract(positions[i], positions[k]);
                        distanceDiff.Normalize();
                        distanceDiff = Vector2.Divide(distanceDiff, distance);
                        separationVector = Vector2.Add(separationVector, distanceDiff);
                        separationCount++;
                    }
                    //alignment
                    if (distance < neighbordist_align)
                    {
                        alignmentSum = Vector2.Add(alignmentSum, velocities[k]);
                        alignmentCount++;
                    }
                    //cohesion
                    if (distance < neighbordist_cohese)
                    {
                        cohesionSum = Vector2.Add(cohesionSum, positions[k]);
                        cohesionCount++;
                    }
                }

                #endregion


                #region Get separation vector

                if (separationCount > 0)
                {
                    separationVector = Vector2.Divide(separationVector, separationCount);
                }
                if(Math.Abs(separationVector.X) > 0 & Math.Abs(separationVector.Y) > 0)
                {
                    separationVector.Normalize();
                    separationVector = Vector2.Multiply(separationVector, maxSpeed);
                    separationVector = Vector2.Subtract(separationVector, velocities[i]);
                    separationVector = Vector2.Clamp(separationVector, -maxForce, maxForce);
                }

                #endregion


                #region Get alignemnt vector

                if (alignmentCount > 0)
                {
                    alignmentSum = Vector2.Divide(alignmentSum, alignmentCount);
                    alignmentSum.Normalize();
                    alignmentSum = Vector2.Multiply(alignmentSum, maxSpeed);
                    alignmentVector = Vector2.Subtract(alignmentSum, velocities[i]);
                    alignmentVector = Vector2.Clamp(alignmentVector, -maxForce, maxForce);
                }
                else
                { alignmentVector.X = 0; alignmentVector.Y = 0; }

                #endregion


                #region Get cohesion vector
                
                if (cohesionCount > 0)
                {
                    ///*
                    //method 1 - orig method
                    cohesionSum = Vector2.Divide(cohesionSum, cohesionCount);
                    desired = Vector2.Subtract(cohesionSum, positions[i]);
                    desired.Normalize();
                    desired = Vector2.Multiply(desired, maxSpeed);
                    cohesionVector = Vector2.Subtract(desired, velocities[i]);
                    cohesionVector = Vector2.Clamp(cohesionVector, -maxForce, maxForce);
                    //*/

                    /*
                    //method 2 - water like flocking
                    cohesionSum = Vector2.Divide(cohesionSum, cohesionCount);
                    cohesionVector = Vector2.Clamp(cohesionVector, -maxForce, maxForce);
                    */

                    /*
                    //method 3 - larger clumps
                    cohesionSum = Vector2.Divide(cohesionSum, cohesionCount);
                    desired = Vector2.Subtract(cohesionSum, positions[i]);
                    cohesionVector = Vector2.Subtract(desired, velocities[i]);
                    cohesionVector = Vector2.Clamp(cohesionVector, -maxForce, maxForce);
                    */
                }
                else { cohesionVector.X = 0; cohesionVector.Y = 0; }

                #endregion



                //boids avoid mg logo in center
                distance = Vector2.Distance(
                    positions[i], cursorPos);
                pushVector.X = 0.0f; pushVector.Y = 0.0f;
                if ((distance > 0) && (distance < cursorDistance))
                {
                    pushVector = Vector2.Subtract(positions[i], cursorPos);
                    pushVector.Normalize();
                    accelerations[i] = Vector2.Add(accelerations[i], pushVector);
                }
                //multiply vectors
                separationVector = Vector2.Multiply(separationVector, 2.0f);
                alignmentVector = Vector2.Multiply(alignmentVector, 1.0f);
                cohesionVector = Vector2.Multiply(cohesionVector, 1.0f);
                //apply vectors
                accelerations[i] = Vector2.Add(accelerations[i], separationVector);
                accelerations[i] = Vector2.Add(accelerations[i], alignmentVector);
                accelerations[i] = Vector2.Add(accelerations[i], cohesionVector);

                //calc movement
                velocities[i] = Vector2.Add(velocities[i], accelerations[i]);
                velocities[i] = Vector2.Clamp(velocities[i], -maxSpeed, maxSpeed);
                positions[i] = Vector2.Add(positions[i], velocities[i]);
                
                //handle borders
                if (positions[i].X > 1024) { positions[i].X = 0; }
                if (positions[i].X < 0) { positions[i].X = 1024; }
                if (positions[i].Y > 512) { positions[i].Y = 0; }
                if (positions[i].Y < 0) { positions[i].Y = 512; }

                //update animations
                Functions.Animation_Update(anims[i], sprites[i]);

                //match sprite positions to positions data (view of model)
                sprites[i].X = positions[i].X;
                sprites[i].Y = positions[i].Y;

                //set sprite horizontal flip based X velocity
                if (velocities[i].X < 0) //moving left
                { sprites[i].flipHorizontally = true; }
                else { sprites[i].flipHorizontally = false; }
            }


            //lazy debugger
            //Debug.WriteLine("boid 7 mag: " + velocities[7].X + ", " + velocities[7].Y);
            //Debug.WriteLine("boid 7 acc: " + accelerations[7].X + ", " + accelerations[7].Y);

            
            //clear accelerations
            for (i = 0; i < size; i++)
            { accelerations[i].X = 0; accelerations[i].Y = 0; }
        }

        public static void Draw()
        {
            for (i = 0; i < size; i++)
            { Functions.Draw(sprites[i]); }
        }

    }


    

    
}