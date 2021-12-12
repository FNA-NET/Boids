using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FNA.Boids
{
    public class Game1 : Game
    {
        public ComponentSprite bkg;
        ComponentText feedback;
        Stopwatch timer;
        Color bkgCol = new Color(30, 30, 60);



        public Game1()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);


            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

#if NET
            if (System.OperatingSystem.IsAndroid() || System.OperatingSystem.IsIOS())
            {
                graphics.IsFullScreen = true;
                Data.winW = displayMode.Width;
                Data.winH = displayMode.Height;
            }
#endif

            graphics.PreferredBackBufferWidth = Data.winW;
            graphics.PreferredBackBufferHeight = Data.winH;

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
                Data.winW / 2, Data.winH / 2,
                new AnimFrame(0, 1, false), 
                new Point(1024, 512));

            //setup onscreen debugger
            feedback = new ComponentText(
                "test",
                new Vector2(
                    Data.winW / 2 - 16 * 3,
                    Data.winH / 2 + 16 * 3 - 4),
                Color.Orange * 0.9f);
            feedback.scale = 2.0f;

            Data.DisplayRT2D = new RenderTarget2D(
                this.GraphicsDevice,
                Data.winW, Data.winH);


            //init boid pool
            BoidPool.Init();
            timer = new Stopwatch();
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            timer.Restart();

            var thread1 = Task.Run(() => BoidPool.UpdateA());
            var thread2 = Task.Run(() => BoidPool.UpdateB());
            var thread3 = Task.Run(() => BoidPool.UpdateC());
            var thread4 = Task.Run(() => BoidPool.UpdateD());

            timer.Stop();
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw game to texture
            Data.GDM.GraphicsDevice.SetRenderTarget(Data.DisplayRT2D);
            Data.SB.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                depthStencilState: DepthStencilState.Default,
                rasterizerState: RasterizerState.CullCounterClockwise);
            base.Draw(gameTime);
            Functions.Draw(bkg);
            BoidPool.Draw();
            //feedback.text = "" + BoidPool.size + " at " +
            //    timer.ElapsedMilliseconds + "ms";
            //Functions.Draw(feedback);
            Data.SB.End();

            //draw game + bloom
            Data.GDM.GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(bkgCol);
            Data.SB.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                depthStencilState: DepthStencilState.Default,
                rasterizerState: RasterizerState.CullCounterClockwise);
            //draw orig game to screen
            Data.SB.Draw(Data.DisplayRT2D,
               new Rectangle(0, 0, Data.winW, Data.winH),
                Color.White);
            Data.SB.End();
        }

    }
}