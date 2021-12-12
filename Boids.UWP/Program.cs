using System;
using SDL2;

namespace Boids.UWP
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            realArgs = args;
            SDL.SDL_SetHint("SDL_WINRT_HANDLE_BACK_BUTTON", "1");
            SDL.SDL_main_func mainFunction = FakeMain;
            SDL.SDL_WinRTRunApp(mainFunction, IntPtr.Zero);
        }

        static string[] realArgs;
        static int FakeMain(int argc, IntPtr argv)
        {
            RealMain(realArgs);
            return 0;
        }

        static void RealMain(string[] args)
        {
            using (var g = new Game1())
            {
                g.Run();
            }
        }
    }
}
