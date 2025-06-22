using BikesExtraHotKey.Constants;
using BikesExtraHotKey.Debugger;
using BikesExtraHotKey.Models.Helper;
using BikesExtraHotKey.Models.Localization;
using BikesExtraHotKey.Settings;
using BikesExtraHotKey.UiSystem;
using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
using Game.City;
using Game.Modding;
using Game.SceneFlow;
using System.IO;
using System.Reflection;

namespace BikesExtraHotKey
{
	public class Hotkey : IMod
	{
		public static ModSettings ModSettings;
		private LocalizationManager LocalizationManager => GameManager.instance.localizationManager;
		private string modPath;

		public static ILog logger = LogManager.GetLogger($"{nameof(BikesExtraHotKey)}.{nameof(Hotkey)}").SetShowsErrorsInUI(false);
		public static Logger debugLogger = new Logger(logger, true);

		public void OnLoad(UpdateSystem updateSystem)
		{
			
			debugLogger.InfoWithLine(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				modPath = Path.GetDirectoryName(asset.path);
                debugLogger.InfoWithLine($"Current mod asset at {modPath}");
			}


			Localization.LoadLocalization(Assembly.GetExecutingAssembly());

			FileInfo fileInfo = new(asset.path);
			Icons.LoadIconsFolder(Icons.IconsResourceKey, fileInfo.Directory.FullName);

			ModSettings = new ModSettings(this);
			ModSettings.RegisterInOptionsUI();
			ModSettings.RegisterKeyBindings();
			AssetDatabase.global.LoadSettings(Global.Base, ModSettings, new ModSettings(this));
			ModSettings.ApplyAndSave();

			updateSystem.UpdateAt<UISystem>(SystemUpdatePhase.UIUpdate);
		}

		public void OnDispose()
		{
            debugLogger.InfoWithLine($"{nameof(Hotkey)}.{nameof(OnDispose)}");

			if (ModSettings != null)
			{
				ModSettings.UnregisterInOptionsUI();
				ModSettings = null;
			}
			else
			{
                debugLogger.InfoWithLine($"ModSettings is NULL");
			}
		}
	}
}