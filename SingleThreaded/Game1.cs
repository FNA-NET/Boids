using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace FNA.Boids
{
    public class Game1 : Game
    {
        public ComponentSprite bkg;
        
        ComponentText feedback;
        Stopwatch timer;

        

        
        public Game1()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            Data.GDM = graphics;
            Content.RootDirectory = "Content";
            Data.CM = Content;

            this.IsMouseVisible = true;
        }
        protected override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            Data.SB = spriteBatch;

            //load game assets
            Data.Assets_mainSheet = Data.CM.Load<Texture2D>(@"sheet");
            Data.Assets_font = Data.CM.Load<SpriteFont>(@"pixelFont");

            //setup the bkg sprite
            bkg = new ComponentSprite(Data.Assets_mainSheet,
                1024 / 2, 512 / 2,
                new AnimFrame(0, 1, false), new Point(1024, 512));

            //setup onscreen debugger
            feedback = new ComponentText(
                "test",
                new Vector2(
                    1024 / 2 - 16 * 3, 
                    512 / 2 + 16 * 3 - 4), 
                Color.Orange * 0.9f);
            feedback.scale = 2.0f;

            //init boid pool
            BoidPool.Init();
            timer = new Stopwatch();
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            timer.Restart();
            BoidPool.Update();
            timer.Stop();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Data.SB.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                depthStencilState: DepthStencilState.Default,
                rasterizerState: RasterizerState.CullCounterClockwise);

            base.Draw(gameTime);
            Functions.Draw(bkg);
            BoidPool.Draw();

            feedback.text = "" + BoidPool.size + " at " + 
                timer.ElapsedMilliseconds + "ms";

            Functions.Draw(feedback);

            Data.SB.End();
        }
        
    }
}