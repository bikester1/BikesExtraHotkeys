using cohtml.Net;
using Game.Input;
using Game.Prefabs;
using System.Collections.Generic;
using Unity.Entities;
using KSExtraHotkey;
using KSExtraHotkey.Settings;
using KSExtraHotkey.Input;
using Unity.Collections;

namespace KSExtraHotkey.Models.Tools
{
    public class ToolWindowManager
    {
        private readonly View _uiView;
        private readonly UIInputManager _uiInputManager;
        private readonly ModSettings _modSettings;
        private readonly PrefabSystem _prefabSystem;
        private readonly List<(ProxyAction binding, string toolName)> _openToolWindowsBindings;

        public ToolWindowManager(
            View uiView,
            UIInputManager uiInputManager,
            ModSettings modSettings,
            PrefabSystem m_prefabSystem
            )
        {
            _uiView = uiView;
            _uiInputManager = uiInputManager;
            _modSettings = modSettings;
            _prefabSystem = m_prefabSystem;
            _openToolWindowsBindings = new List<(ProxyAction, string)>();

            Hotkey.Logger.Info($"{nameof(ToolWindowManager)} initialized");
        }

        private void RegisterKeybinding(string settingName, string toolName)
        {
            var binding = _uiInputManager.GetAndEnableBinding(settingName);
            _openToolWindowsBindings.Add((binding, toolName));
        }

        public void CheckHotkeys()
        {
            foreach (var (binding, toolName) in _openToolWindowsBindings)
            {
                if (binding.WasPerformedThisFrame())
                {
                    _uiView.TriggerEvent("toolbar.selectAssetMenu", GetAssetMenuObject(toolName));
                    PlayUISound("open-panel");
                }
            }
        }

        private object GetAssetMenuObject(string toolName)
        {
            EntityQuery assetMenuData = _prefabSystem.World.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<UIAssetMenuData>());
            NativeArray<Entity> menuEntities = assetMenuData.ToEntityArray(Allocator.Temp);
            Entity menuEntity = Entity.Null;

            foreach (Entity entity in menuEntities)
            {
                UIAssetMenuPrefab assetMenuPrefab = _prefabSystem.GetPrefab<UIAssetMenuPrefab>(entity);
                if (assetMenuPrefab.name == toolName)
                {
                    menuEntity = entity;
                    break;
                }
            }
            menuEntities.Dispose();

            if (menuEntity == Entity.Null)
            {
                Hotkey.Logger.Error($"Could not find menu entity for {toolName}");
            }

            return new
            {
                __Type = menuEntity.GetType().ToString(),
                index = menuEntity.Index,
                version = menuEntity.Version
            };
        }

        public void PlayUISound(string soundName)
        {
            _uiView.TriggerEvent("audio.playSound", soundName, 1);
        }
    }
}