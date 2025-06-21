using System;
using System.Reflection;
using Colossal.UI.Binding;
using Game.Input;
using Game.Reflection;
using Game.UI.Menu;
using Game.UI.Widgets;

namespace BikesExtraHotKey.CustomOptionUIWidgets
{
    public static class Widgets
    {
        public class ExtendedKeybindingField : InputBindingField
        {
            private string m_Icon;

            public ExtendedKeybindingField(string icon, AutomaticSettings.SettingItemData itemData) : base()
            {
                m_Icon = icon;

                #region Vanilla

                path = (PathSegment)itemData.path;
                displayName = itemData.displayName;
                description = itemData.description;
                displayNameAction = itemData.dispayNameAction;
                descriptionAction = itemData.descriptionAction;
                Action<ProxyBinding> setterAction = itemData.setterAction as Action<ProxyBinding>;
                accessor = (ITypedValueAccessor<ProxyBinding>)new DelegateAccessor<ProxyBinding>((Func<ProxyBinding>)(() => {
                    ProxyBinding binding = (ProxyBinding)itemData.property.GetValue((object)itemData.setting);
                    var watcher = (ProxyBinding.Watcher)typeof(InputManager).GetMethod("GetOrCreateBindingWatcher", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(InputManager.instance, new object[] { binding });
                    ProxyBinding binding2 = watcher.binding;
                    FieldInfo fieldInfo = typeof(ProxyBinding).GetField("alies", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fieldInfo != null)
                    {
                        object originalAlias = fieldInfo.GetValue(binding);
                        fieldInfo.SetValue(binding2, originalAlias);
                    }
                    return binding2;
                }), (Action<ProxyBinding>)(value => {
                    ProxyBinding result;
                    if (!InputManager.instance.SetBinding(value, out result))
                        return;
                    Action<ProxyBinding> action = setterAction;
                    if (action != null)
                        action(result);
                    itemData.property.SetValue((object)itemData.setting, (object)result);
                    itemData.setting.ApplyAndSave();
                }));
                valueVersion = itemData.valueVersionAction ?? new Func<int>(GetValueVersion);
                disabled = itemData.disableAction;
                hidden = itemData.hideAction;

                #endregion
            }

            static int GetValueVersion()
            {
                return InputManager.instance.actionVersion;
            }

            protected override void WriteProperties(IJsonWriter writer)
            {
                base.WriteProperties(writer);
                writer.PropertyName("icon");
                writer.Write(m_Icon);
            }
        }

    }
}
