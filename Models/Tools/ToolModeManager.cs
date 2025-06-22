using BikesExtraHotKey.Input;
using BikesExtraHotKey.Settings;
using cohtml.Net;
using Game.Input;
using Game.Tools;
using System;
using System.Collections.Generic;

namespace BikesExtraHotKey.Models.Tools
{
	public class ToolModeManager
	{
		private readonly View _uiView;
		private readonly UIInputManager _uiInputManager;
		private readonly ModSettings _modSettings;
		private readonly ToolSystem _toolSystem;
		private readonly NetToolSystem _netToolSystem;
		private readonly ZoneToolSystem _zoneToolSystem;
		private readonly List<(ProxyAction binding, string toolMode)> _toolModeBindings;

		public ToolModeManager(
			View uiView,
			UIInputManager uiInputManager,
			ModSettings modSettings,
			ToolSystem m_toolSystem,
			NetToolSystem m_netToolSystem,
			ZoneToolSystem m_zoneToolSystem
			)
		{
			_uiView = uiView;
			_uiInputManager = uiInputManager;
			_toolSystem = m_toolSystem;
			_modSettings = modSettings;
			_netToolSystem = m_netToolSystem;
			_zoneToolSystem = m_zoneToolSystem;
			_toolModeBindings = new List<(ProxyAction, string)>();

			InitializeBindings();

			Hotkey.debugLogger.InfoWithLine($"{nameof(ToolModeManager)} initialized");
		}

		private void InitializeBindings()
		{
			RegisterKeybinding(nameof(_modSettings.RoadStraight), "Straight");
			RegisterKeybinding(nameof(_modSettings.RoadSimpleCurve), "SimpleCurve");
			RegisterKeybinding(nameof(_modSettings.RoadComplexCurve), "ComplexCurve");
			RegisterKeybinding(nameof(_modSettings.RoadContinuous), "Continuous");
			RegisterKeybinding(nameof(_modSettings.RoadGrid), "Grid");
			RegisterKeybinding(nameof(_modSettings.RoadReplace), "Replace");

			RegisterKeybinding(nameof(_modSettings.ZoneFill), "FloodFill");
			RegisterKeybinding(nameof(_modSettings.ZoneMarquee), "Marquee");
			RegisterKeybinding(nameof(_modSettings.ZonePaint), "Paint");

			RegisterKeybinding(nameof(_modSettings.ResetElevation), "ResetElevation");
		}

		private void RegisterKeybinding(string settingName, string toolMode)
		{
			var binding = _uiInputManager.GetAndEnableBinding(settingName);
			_toolModeBindings.Add((binding, toolMode));
		}

		public void CheckHotkeys()
		{
			foreach (var (binding, toolMode) in _toolModeBindings)
			{
				if (binding.WasPerformedThisFrame())
				{
					if (toolMode == "ResetElevation")
					{
						ResetElevation();
					}
					else
					{
						SetToolMode(toolMode);
					}
				}
			}
		}

		private void SetToolMode(string toolMode)
		{
			if (_toolSystem.activeTool is NetToolSystem netTool)
			{
				string _toolMode = GetToolModeString(toolMode);
				SetNetToolMode(netTool, _toolMode);
			}
			else if (_toolSystem.activeTool is ZoneToolSystem zoneTool)
			{
				string _toolMode = GetToolModeString(toolMode, 1);
				SetZoneToolMode(zoneTool, _toolMode);
			}
		}

		private void SetNetToolMode(NetToolSystem tool, string modeName)
		{
			if (Enum.TryParse<NetToolSystem.Mode>(modeName, out var mode))
			{
				_uiView.TriggerEvent("tool.selectToolMode", (int)mode);
				PlayUISound("select-item");
			}
			else
			{
				Hotkey.logger.Warn($"Invalid mode name for NetToolSystem: {modeName}");
			}
		}

		private void SetZoneToolMode(ZoneToolSystem tool, string modeName)
		{
			if (Enum.TryParse<ZoneToolSystem.Mode>(modeName, out var mode))
			{
				_uiView.TriggerEvent("tool.selectToolMode", (int)mode);
				PlayUISound("select-item");
			}
			else
			{
				Hotkey.logger.Warn($"Invalid mode name for ZoneToolSystem: {modeName}");
			}
		}

		private void ResetElevation()
		{
			if (_modSettings.EnableResetElevation && _toolSystem.activeTool is NetToolSystem)
			{
				_netToolSystem.elevation = 0f;
				PlayUISound("select-item");
			}
		}

		private static string GetToolModeString(string toolMode, int index = 0)
		{
			if (toolMode.Contains("|"))
			{
				string[] modes = toolMode.Split('|');
				return modes[index];
			}
			return toolMode;
		}

		public void PlayUISound(string soundName)
		{
			_uiView.TriggerEvent("audio.playSound", soundName, 1);
		}
	}
}