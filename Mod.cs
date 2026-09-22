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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BikesExtraHotKey
{
	public class Hotkey : IMod
	{
		public static ModSettings ModSettings;
		private LocalizationManager LocalizationManager => GameManager.instance.localizationManager;
		private string modPath;
		private static List<(LogLevel, object)> LogQueue = new List<(LogLevel, object)>();

		private static ILog logger;
		public static HotKeyLogger debugLogger = new HotKeyLogger();
		public static bool Initialized = false;


		public void OnLoad(UpdateSystem updateSystem)
		{

            logger = LogManager.GetLogger($"{nameof(BikesExtraHotKey)}.{nameof(Hotkey)}").SetShowsErrorsInUI(false);
			debugLogger.InitializeLogger(logger);

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

			Initialized = true;
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

		/// <summary>
		/// Works around a Colossal.Logging/Backtrace interaction that produces a storm of
		/// NullReferenceExceptions: when a warning or error is logged, Backtrace attaches the log
		/// files via <c>File.ReadAllBytes</c> (FileShare.Read). If the originating logger then tries
		/// to write, its <c>Open()</c> fails with a sharing violation, is swallowed, and
		/// <c>Internal_WriteStream</c> dereferences a null stream. The resulting NRE is itself logged
		/// as an error, triggering another report and repeating. Disabling Backtrace reports for the
		/// registered loggers breaks the loop. Debug builds only.
		/// </summary>
		private static void DisableBacktraceReports()
		{
			try
			{
				FieldInfo loggersField = typeof(LogManager).GetField("m_Loggers", BindingFlags.Static | BindingFlags.NonPublic);
				if (loggersField?.GetValue(null) is IDictionary loggers)
				{
					int count = 0;
					foreach (object value in loggers.Values)
					{
						if (value is ILog log)
						{
							log.disableBacktrace = true;
							count++;
						}
					}
					debugLogger.InfoWithLine($"DisableBacktraceReports: disabled backtrace on {count} loggers");
				}
			}
			catch (System.Exception ex)
			{
				debugLogger.WarnWithLine($"DisableBacktraceReports failed: {ex.Message}");
			}
		}

    }


}