using Colossal.Json;
using Colossal.Localization;
using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BikesExtraHotKey.Models.Localization
{
	public class Localization
	{
		public static void LoadLocalization(Assembly assembly, string namespaceName = null, string defaultLocalID = "en-US")
		{
			namespaceName ??= assembly.GetName().Name;
			Hotkey.debugLogger.InfoWithLine("Start loading the localization.");
			try
			{
				Hotkey.debugLogger.InfoWithLine("Loading multiple Localization file");
				foreach (string localeID in GameManager.instance.localizationManager.GetSupportedLocales())
				{
					Hotkey.debugLogger.InfoWithLine($"Loading {localeID}");
					Dictionary<string, string> localization;

					if (assembly.GetManifestResourceNames().Contains($"{namespaceName}.Localization.{localeID}.json"))
						localization = Decoder.Decode(new StreamReader(assembly.GetManifestResourceStream($"{namespaceName}.Localization.{localeID}.json")).ReadToEnd()).Make<Dictionary<string, string>>();
					else if (assembly.GetManifestResourceNames().Contains($"{namespaceName}.Localization.{defaultLocalID}.json"))
					{
						localization = Decoder.Decode(new StreamReader(assembly.GetManifestResourceStream($"{namespaceName}.Localization.{defaultLocalID}.json")).ReadToEnd()).Make<Dictionary<string, string>>();
						Hotkey.debugLogger.Warn($"No {localeID} in the files, using {defaultLocalID} instead.");
					}
					else
					{
						Hotkey.debugLogger.Error($"No {localeID} in the files, and no {defaultLocalID}. This maybe due of an assembly name different from the namespace name.");
						continue;
					}

					ApplyDebugMarker(localization);

					GameManager.instance.localizationManager.AddSource(localeID, new MemorySource(localization));
				}
			}
			catch (Exception ex) { Hotkey.debugLogger.Error(ex); }
		}

		/// <summary>
		/// Appends a marker to the mod's settings page (and input map) titles so a debug build is
		/// immediately recognizable in the Options UI. This is compiled out of Release builds.
		/// </summary>
		private static void ApplyDebugMarker(Dictionary<string, string> localization)
		{
#if DEBUG
			const string suffix = " (Debug)";

			// Matches Game.Modding.ModSetting.id for this mod: {assembly}.{namespace}.{mod type}.
			string modId = $"{typeof(Hotkey).Assembly.GetName().Name}.{typeof(Hotkey).Namespace}.{typeof(Hotkey).Name}";

			string[] keys =
			{
				$"Options.SECTION[{modId}]",
				$"Options.INPUT_MAP[{modId}]",
			};

			foreach (string key in keys)
			{
				if (localization.TryGetValue(key, out string value) && !value.EndsWith(suffix))
					localization[key] = value + suffix;
			}
#endif
		}
	}
}
