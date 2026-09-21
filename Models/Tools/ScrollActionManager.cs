using BikesExtraHotKey.Input;
using BikesExtraHotKey.Settings;
using Game.Tools;

namespace BikesExtraHotKey.Models.Tools
{
	public class ScrollActionManager
	{
		private readonly UIInputManager _uiInputManager;
		private readonly ModSettings _modSettings;
		private readonly ToolSystem _toolSystem;
		private readonly NetToolSystem _netToolSystem;
		private readonly TerrainToolSystem _terrainToolSystem;
		private readonly BrushManager _brushManager;
		private readonly ElevationManager _elevationManager;
		private readonly ObjectToolSystem _objectToolSystem;
		private readonly AreaBulldozerIntegration _areaBulldozerIntegration;

		public ScrollActionManager(
			UIInputManager uiInputManager,
			ModSettings modSettings,
			ToolSystem m_toolSystem,
			NetToolSystem m_netToolSystem,
			TerrainToolSystem m_terrainToolSystem,
			ObjectToolSystem m_objectToolSystem
			)
		{
			_uiInputManager = uiInputManager;
			_modSettings = modSettings;
			_toolSystem = m_toolSystem;
			_netToolSystem = m_netToolSystem;
			_terrainToolSystem = m_terrainToolSystem;
			_objectToolSystem = m_objectToolSystem;
			_brushManager = new BrushManager(modSettings, uiInputManager, m_terrainToolSystem, m_objectToolSystem, m_toolSystem);
			_elevationManager = new ElevationManager(modSettings, uiInputManager, m_netToolSystem);
			_areaBulldozerIntegration = new AreaBulldozerIntegration(modSettings);

			Hotkey.debugLogger.InfoWithLine($"{nameof(ScrollActionManager)} initialized");
		}

		public void CheckScrollWheelActions()
		{
			// Must run every frame, not just while the Area Bulldozer tool is active,
			// so a pending save still flushes if the user switches tools right after
			// scrolling.
			_areaBulldozerIntegration.Tick();

			if (_areaBulldozerIntegration.IsActiveTool(_toolSystem.activeTool))
				HandleAreaBulldozerScrollActions();
			else if (_toolSystem.activeTool is NetToolSystem)
				HandleNetToolScrollActions();
			else if (_toolSystem.activeTool is TerrainToolSystem || _toolSystem.activeTool is ObjectToolSystem)
				HandleBrushToolScrollActions();
		}

		private void HandleAreaBulldozerScrollActions()
		{
			if (_uiInputManager.IsHoldingCtrl())
			{
				_uiInputManager.DisableCameraZoom(true);

				if (!_areaBulldozerIntegration.OnScroll(
					_uiInputManager.IsZoomingIn(),
					_uiInputManager.IsZoomingOut()))
				{
					// The integration is disabled (or the mod is missing), so do not
					// swallow the camera zoom while the modifier is held.
					_uiInputManager.DisableCameraZoom(false);
				}
			}
			else
			{
				_uiInputManager.DisableCameraZoom(false);
			}
		}

		private void HandleNetToolScrollActions()
		{
			if (_uiInputManager.IsHoldingCtrl())
			{
				_uiInputManager.DisableCameraZoom(true);
				_elevationManager.OnElevationScroll();
			}
			else if (_uiInputManager.IsHoldingAlt())
			{
				_uiInputManager.DisableCameraZoom(true);
				_elevationManager.OnElevationStepScroll();
			}
			else
			{
				_uiInputManager.DisableCameraZoom(false);
			}
		}

		private void HandleBrushToolScrollActions()
		{
			if (_uiInputManager.IsHoldingCtrl())
			{
				_uiInputManager.DisableCameraZoom(true);
				_brushManager.OnBrushSizeScroll();
			}
			else if (_uiInputManager.IsHoldingAlt())
			{
				_uiInputManager.DisableCameraZoom(true);
				_brushManager.OnBrushStrengthScroll();
			}
			else
			{
				_uiInputManager.DisableCameraZoom(false);
			}
		}
	}
}