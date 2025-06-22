using BikesExtraHotKey.Constants;
using BikesExtraHotKey.Models.Helper;
using BikesExtraHotKey.Models.Localization;
using BikesExtraHotKey.Settings;
using BikesExtraHotKey.UiSystem;
using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
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

        public static ILog Logger = LogManager.GetLogger($"{nameof(BikesExtraHotKey)}.{nameof(Hotkey)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {

            Logger.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                modPath = Path.GetDirectoryName(asset.path);
                Logger.Info($"Current mod asset at {modPath}");
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
            Logger.Info($"{nameof(Hotkey)}.{nameof(OnDispose)}");

            if (ModSettings != null)
            {
                ModSettings.UnregisterInOptionsUI();
                ModSettings = null;
            }
            else
            {
                Logger.Info($"ModSettings is NULL");
            }
        }
    }
}