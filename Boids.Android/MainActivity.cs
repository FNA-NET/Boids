using Microsoft.Xna.Framework;
using Android.Content.PM;

namespace Boids.Android
{
	[Activity(
		Label = "@string/app_name",
		MainLauncher = true,
		HardwareAccelerated = true,
		ScreenOrientation = ScreenOrientation.Landscape
	)]
	public class MainActivity : AndroidGameActivity
	{
		protected override void SDLMain()
		{
            using (var game = new Game1())
                game.Run();
		}
    }
}
