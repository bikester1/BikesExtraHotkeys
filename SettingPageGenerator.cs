using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Colossal.Reflection;
using KSExtraHotkey.CustomOptionUIWidgets;
using Game.Settings;
using Game.UI.Menu;
using Game.UI.Widgets;
using KSExtraHotkey;
using KSExtraHotkey.Models.Ui;

namespace KSExtraHotkey.Settings
{
    public partial class ModSettings
    {
        private AutomaticSettings.SettingPageData GeneratePage(string pageId, bool addPrefix)
        {
            #region Vanilla Init

            ClearButtonGroups();
            Setting setting = this;
            AutomaticSettings.SettingPageData pageData = new AutomaticSettings.SettingPageData(pageId, addPrefix);
            var parameters = new object[]
            {
                   setting, false, null
            };
            if (MethodReturnsTrue(typeof(AutomaticSettings), "IsShowGroupName", BindingFlags.Static | BindingFlags.NonPublic, null, parameters))
            {
                bool showAll = (bool)parameters[1];
                ReadOnlyCollection<string> groups = (ReadOnlyCollection<string>)parameters[2];
                if (showAll)
                {
                    pageData.showAllGroupNames = true;
                }
                else
                {
                    foreach (string group in groups)
                        pageData.AddGroupToShowName(group);
                }
            }
            pageData.warningGetter = GetWarningGetter(setting);
            pageData.tabWarningGetters = GetTabWarningGetters(setting);

            FillTabs(setting, pageData);
            FillGroups(setting, pageData);

            #endregion

            #region Custom

            FillSections(setting, pageData);

            #endregion

            #region Vanilla
            ClearButtonGroups();
            return pageData;
            #endregion

        }

        private static void FillTabs(Setting setting, AutomaticSettings.SettingPageData pageData)
        {
            if (setting.GetType().TryGetAttribute<SettingsUITabOrderAttribute>(out SettingsUITabOrderAttribute attribute1))
            {
                Func<string[]> action;
                if (AutomaticSettings.TryGetAction<string[]>(setting, attribute1.checkType, attribute1.checkMethod, out action))
                {
                    foreach (string tab in action())
                        pageData.AddTab(tab);
                }
                else
                {
                    foreach (string tab in attribute1.tabs)
                        pageData.AddTab(tab);
                }
            }
        }

        private static void FillGroups(Game.Settings.Setting setting, AutomaticSettings.SettingPageData pageData)
        {
            if (setting.GetType().TryGetAttribute<SettingsUIGroupOrderAttribute>(out SettingsUIGroupOrderAttribute attribute2))
            {
                Func<string[]> action;
                if (AutomaticSettings.TryGetAction<string[]>(setting, attribute2.checkType, attribute2.checkMethod, out action))
                {
                    foreach (string group in action())
                        pageData.AddGroup(group);
                }
                else
                {
                    foreach (string group in attribute2.groups)
                        pageData.AddGroup(group);
                }
            }
        }

        private void FillSections(Game.Settings.Setting setting, AutomaticSettings.SettingPageData pageData)
        {
            foreach (PropertyInfo propInfo in setting.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                #region Vanilla

                AutomaticSettings.ProxyProperty property = new AutomaticSettings.ProxyProperty(propInfo);
                AutomaticSettings.WidgetType widgetType = AutomaticSettings.GetWidgetType(property);

                if (MethodReturnsTrue(typeof(AutomaticSettings), "IsHidden", BindingFlags.Static | BindingFlags.NonPublic, null, property) ||
                    !MethodReturnsTrue(typeof(AutomaticSettings), "IsSupportedOnPlatform", BindingFlags.Static | BindingFlags.NonPublic, null, property))
                {
                    continue;
                }

                #endregion

                CustomWidgetType customWidgetType = GetCustomWidgetType(property);

                if (widgetType != AutomaticSettings.WidgetType.None || customWidgetType != CustomWidgetType.None)
                {
                    foreach (SectionInfo sectionInfo in BuildSections(property).Values)
                    {
                        AutomaticSettings.SettingItemData settingItemData;
                        if (customWidgetType != CustomWidgetType.None)
                        {
                            #region Custom Logic

                            settingItemData = GetSettingsItemData(customWidgetType, setting, property, pageData.prefix);

                            Hotkey.Logger.Info("KSHotKey Init " + property);

                            #endregion
                        }
                        else
                        {
                            #region Vanilla Logic

                            settingItemData = widgetType != AutomaticSettings.WidgetType.MultilineText
                                ? new AutomaticSettings.SettingItemData(widgetType, setting, property, pageData.prefix)
                                : new MultilineTextSettingItemData(setting, property, pageData.prefix);

                            #endregion
                        }

                        // Vanilla logic
                        settingItemData.simpleGroup = sectionInfo.simpleGroup;
                        settingItemData.advancedGroup = sectionInfo.advancedGroup;
                        pageData[sectionInfo.tab].AddItem(settingItemData);
                        pageData.AddGroup(settingItemData.simpleGroup);
                        pageData.AddGroup(settingItemData.advancedGroup);
                    }
                }
            }
        }

        private void ClearButtonGroups()
        {
            if ((typeof(AutomaticSettings).GetField("s_ButtonGroups", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) is ICollection<KeyValuePair<string, ButtonRow>> buttonGroups))
            {
                buttonGroups.Clear();
            }
        }

        private static bool MethodReturnsTrue(Type t, string methodName, BindingFlags bindingAttr, object instance, params object[] p)
        {
            bool? value = (bool?)t.GetMethod(methodName, bindingAttr)?.Invoke(instance, p);
            return value.HasValue && value.Value;
        }

        private static Func<bool> GetWarningGetter(Game.Settings.Setting setting)
        {
            SettingsUIPageWarningAttribute attribute = ReflectionUtils.GetAttribute<SettingsUIPageWarningAttribute>(setting.GetType().GetCustomAttributes(inherit: false));
            if (attribute != null && AutomaticSettings.TryGetAction(setting, attribute.checkType, attribute.checkMethod, out Func<bool> action))
            {
                return action;
            }
            return null;
        }

        private static Dictionary<string, Func<bool>> GetTabWarningGetters(Game.Settings.Setting setting)
        {
            Dictionary<string, Func<bool>> dictionary = new Dictionary<string, Func<bool>>();
            foreach (SettingsUITabWarningAttribute attribute in ReflectionUtils.GetAttributes<SettingsUITabWarningAttribute>(setting.GetType().GetCustomAttributes(inherit: false)))
            {
                if (!string.IsNullOrEmpty(attribute.tab) && AutomaticSettings.TryGetAction(setting, attribute.checkType, attribute.checkMethod, out Func<bool> action))
                {
                    if (!dictionary.ContainsKey(attribute.tab))
                    {
                        dictionary.Add(attribute.tab, action);
                    }
                }
            }
            return dictionary;
        }

        private Dictionary<string, SectionInfo> BuildSections(AutomaticSettings.IProxyProperty property)
        {
            Dictionary<string, SectionInfo> sections = new Dictionary<string, SectionInfo>();
            foreach (SettingsUISectionAttribute attribute in property.GetAttributes<SettingsUISectionAttribute>())
            {
                sections[attribute.tab] = new SectionInfo
                {
                    tab = attribute.tab,
                    simpleGroup = attribute.simpleGroup,
                    advancedGroup = attribute.advancedGroup
                };
            }
            if (sections.Count != 0)
            {
                return sections;
            }
            foreach (SettingsUISectionAttribute attribute2 in ReflectionUtils.GetAttributes<SettingsUISectionAttribute>(property.declaringType.GetCustomAttributes(inherit: false)))
            {
                sections[attribute2.tab] = new SectionInfo
                {
                    tab = attribute2.tab,
                    simpleGroup = attribute2.simpleGroup,
                    advancedGroup = attribute2.advancedGroup
                };
            }
            if (sections.Count != 0)
            {
                return sections;
            }
            sections["General"] = new SectionInfo
            {
                tab = "General",
                simpleGroup = string.Empty,
                advancedGroup = string.Empty
            };
            return sections;
        }

        private CustomWidgetType GetCustomWidgetType(AutomaticSettings.IProxyProperty property)
        {
            UIAttributes.CustomUIExtendedKeybindingAttribute iconAttribute = property.GetAttribute<UIAttributes.CustomUIExtendedKeybindingAttribute>();
            return iconAttribute != null ? CustomWidgetType.KeybindWithIcon : CustomWidgetType.None;
        }

        private static AutomaticSettings.SettingItemData GetSettingsItemData(
            CustomWidgetType widgetType,
            Game.Settings.Setting setting,
            AutomaticSettings.IProxyProperty property,
            string prefix)
        {
            UIAttributes.CustomUIExtendedKeybindingAttribute iconAttribute = property.GetAttribute<UIAttributes.CustomUIExtendedKeybindingAttribute>();
            return new SettingItemDataTypes.ExtendedKeybindingSettingItemData(iconAttribute?.icon ?? string.Empty, setting, property, prefix);
        }

        private struct SectionInfo
        {
            public string tab;
            public string simpleGroup;
            public string advancedGroup;
        }

        public enum CustomWidgetType
        {
            None,
            KeybindWithIcon
        }
    }
}
