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
            Hotkey.Logger.Info("Start loading the localization.");
            try
            {
                Hotkey.Logger.Info("Loading multiple Localization file");
                foreach (string localeID in GameManager.instance.localizationManager.GetSupportedLocales())
                {
                    Hotkey.Logger.Info($"Loading {localeID}");
                    Dictionary<string, string> localization;

                    if (assembly.GetManifestResourceNames().Contains($"{namespaceName}.Localization.{localeID}.json"))
                        localization = Decoder.Decode(new StreamReader(assembly.GetManifestResourceStream($"{namespaceName}.Localization.{localeID}.json")).ReadToEnd()).Make<Dictionary<string, string>>();
                    else if (assembly.GetManifestResourceNames().Contains($"{namespaceName}.Localization.{defaultLocalID}.json"))
                    {
                        localization = Decoder.Decode(new StreamReader(assembly.GetManifestResourceStream($"{namespaceName}.Localization.{defaultLocalID}.json")).ReadToEnd()).Make<Dictionary<string, string>>();
                        Hotkey.Logger.Warn($"No {localeID} in the files, using {defaultLocalID} instead.");
                    }
                    else
                    {
                        Hotkey.Logger.Error($"No {localeID} in the files, and no {defaultLocalID}. This maybe due of an assembly name different from the namespace name.");
                        continue;
                    }

                    GameManager.instance.localizationManager.AddSource(localeID, new MemorySource(localization));
                }
            }
            catch (Exception ex) { Hotkey.Logger.Error(ex); }
        }
    }
}
