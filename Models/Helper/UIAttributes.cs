using System;
using Game.Input;
using Game.Settings;
using KSExtraHotkey.Models.Tools;

namespace KSExtraHotkey.Models.Ui
{
        public static class UIAttributes
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class CustomUIExtendedKeybindingAttribute : SettingsUIKeyboardBindingAttribute
        {
            public readonly string icon;

            public CustomUIExtendedKeybindingAttribute(
                string icon,
                BindingKeyboard defaultKey,
                string actionName = null,
                bool alt = false,
                bool ctrl = false,
                bool shift = false)
                : base(defaultKey, actionName, alt, ctrl, shift)
            {
                this.icon = icon;
            }
        }
    }
}