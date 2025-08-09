using Colossal.UI;
using System.Collections.Generic;

namespace BikesExtraHotKey.Models.Helper
{
	public static class Icons
	{
		private static readonly Dictionary<string, List<string>> pathToIconLoaded = new Dictionary<string, List<string>>();
		internal static readonly string IconsResourceKey = "bikester1-hotkey";
		internal static readonly string COUIBaseLocation = $"coui://{IconsResourceKey}";

		public static void LoadIconsFolder(string uri, string path, bool shouldWatch = false)
		{

			if (pathToIconLoaded.ContainsKey(uri))
			{
				if (pathToIconLoaded[uri].Contains(path)) return;
				pathToIconLoaded[uri].Add(path);
			}
			else
			{
				pathToIconLoaded.Add(uri, new List<string> { path });
			}

			UIManager.defaultUISystem.AddHostLocation(uri, path, shouldWatch);
		}

	}
}