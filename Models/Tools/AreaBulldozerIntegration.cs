using BikesExtraHotKey.Settings;
using Colossal.IO.AssetDatabase;
using System;
using System.Reflection;
using UnityEngine;

namespace BikesExtraHotKey.Models.Tools
{
	/// <summary>
	/// Optional bridge to the "Area Bulldozer" mod.
	///
	/// Bikes Extra Hotkeys deliberately keeps no compile-time dependency on Area
	/// Bulldozer, so the whole bridge is reflection based. When the mod is not
	/// installed, or its public API changes, every member degrades to a no-op and
	/// the normal scroll-wheel behaviour is left completely untouched.
	/// </summary>
	public class AreaBulldozerIntegration
	{
		/// <summary>
		/// Full name of Area Bulldozer's tool system. Used to cheaply detect the
		/// active tool without touching reflection.
		/// </summary>
		public const string ToolTypeName = "AreaBulldozer.Tools.AreaBulldozerToolSystem";

		private const string ModTypeName = "AreaBulldozer.Mod";
		private const string ModAssemblyName = "AreaBulldozer";

		// Keep in sync with Area Bulldozer's own UI limits
		// (RADIUS_MIN / RADIUS_MAX in AreaBulldozerSections.tsx).
		private const int MIN_RADIUS = 5;
		private const int MAX_RADIUS = 200;

		// Match Area Bulldozer's own settings save debounce.
		private const float SAVE_DELAY_SECONDS = 0.5f;

		private readonly ModSettings _modSettings;

		private bool _resolved;
		private bool _isAvailable;

		private PropertyInfo _settingsProperty;
		private PropertyInfo _brushRadiusProperty;
		private PropertyInfo _instanceProperty;
		private MethodInfo _invalidateGeometryMethod;

		private bool _savePending;
		private float _saveAt;

		// Logging guards so the per-frame paths never spam the log.
		private bool _lastToolActive;
		private bool _loggedDisabledScroll;
		private bool _loggedUnavailable;

		public AreaBulldozerIntegration(ModSettings modSettings)
		{
			_modSettings = modSettings;
		}

		/// <summary>
		/// True when the given tool is Area Bulldozer's custom selection tool.
		/// This does not require the dependency to be resolved yet.
		/// </summary>
		public bool IsActiveTool(object activeTool)
		{
			bool isActive =
				activeTool != null &&
				activeTool.GetType().FullName == ToolTypeName;

			if (isActive != _lastToolActive)
			{
				_lastToolActive = isActive;

				Hotkey.debugLogger.InfoWithLine(
					isActive
						? "AreaBulldozerIntegration: Area Bulldozer tool is now active."
						: "AreaBulldozerIntegration: Area Bulldozer tool is no longer active.");
			}

			return isActive;
		}

		/// <summary>
		/// True when the Area Bulldozer assembly could be found and its public
		/// API still matches what this bridge expects.
		/// </summary>
		public bool IsAvailable
		{
			get
			{
				if (!_resolved)
				{
					Resolve();
				}

				return _isAvailable;
			}
		}

		/// <summary>
		/// Adjusts the Area Bulldozer brush radius based on the scroll direction.
		/// Returns true when the scroll wheel was actually consumed.
		/// </summary>
		public bool OnScroll(bool zoomingIn, bool zoomingOut)
		{
			if (!_modSettings.EnableAreaBulldozerBrushScroll)
			{
				if (!_loggedDisabledScroll)
				{
					_loggedDisabledScroll = true;

					Hotkey.debugLogger.InfoWithLine(
						"AreaBulldozerIntegration: Ctrl + scroll ignored because " +
						"EnableAreaBulldozerBrushScroll is disabled.");
				}

				return false;
			}

			_loggedDisabledScroll = false;

			if (!IsAvailable)
			{
				if (!_loggedUnavailable)
				{
					_loggedUnavailable = true;

					Hotkey.debugLogger.WarnWithLine(
						"AreaBulldozerIntegration: Ctrl + scroll ignored because the " +
						"Area Bulldozer bridge is unavailable.");
				}

				return false;
			}

			_loggedUnavailable = false;

			if (zoomingIn)
			{
				ChangeRadius(GetRadiusIncrement());
			}
			else if (zoomingOut)
			{
				ChangeRadius(-GetRadiusIncrement());
			}
			else
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Flushes a pending settings save once the scroll wheel has been idle for
		/// <see cref="SAVE_DELAY_SECONDS"/>. Call this every frame, even while the
		/// Area Bulldozer tool is not active, so the save still lands when the user
		/// switches tools right after scrolling.
		/// </summary>
		public void Tick()
		{
			if (!_savePending || Time.unscaledTime < _saveAt)
			{
				return;
			}

			_savePending = false;

			try
			{
				// Public API, and the exact call Area Bulldozer uses in its own UI
				// system. Serialises the current in-memory settings (including the
				// BrushRadius we just changed) to disk.
				_ = AssetDatabase.global.SaveSettings();

				Hotkey.debugLogger.InfoWithLine(
					$"AreaBulldozerIntegration: persisted brush radius " +
					$"{GetRadius()} to disk.");
			}
			catch (Exception ex)
			{
				Hotkey.debugLogger.WarnWithLine(
					$"AreaBulldozerIntegration.Tick failed to save settings: {ex.Message}");
			}
		}

		private int GetRadiusIncrement()
		{
			int current = GetRadius();

			if (current < 20)
			{
				return 1;
			}

			if (current < 50)
			{
				return 5;
			}

			return 10;
		}

		private int GetRadius()
		{
			try
			{
				object settings = _settingsProperty?.GetValue(null);
				if (settings == null)
				{
					return MIN_RADIUS;
				}

				return (int)_brushRadiusProperty.GetValue(settings);
			}
			catch (Exception ex)
			{
				Hotkey.debugLogger.WarnWithLine(
					$"AreaBulldozerIntegration.GetRadius failed: {ex.Message}");
				return MIN_RADIUS;
			}
		}

		private void ChangeRadius(int delta)
		{
			try
			{
				object settings = _settingsProperty.GetValue(null);
				if (settings == null)
				{
					Hotkey.debugLogger.WarnWithLine(
						"AreaBulldozerIntegration: cannot change radius because " +
						"Area Bulldozer settings are null.");

					return;
				}

				int currentRadius = GetRadius();
				int newRadius = Math.Min(
					Math.Max(currentRadius + delta, MIN_RADIUS),
					MAX_RADIUS);

				if (newRadius == currentRadius)
				{
					// Already at the limit: consume the scroll but don't log or
					// schedule a save for a no-op.
					return;
				}

				_brushRadiusProperty.SetValue(settings, newRadius);

				Hotkey.debugLogger.InfoWithLine(
					$"AreaBulldozerIntegration: brush radius " +
					$"{currentRadius} -> {newRadius} (delta {delta:+#;-#;0}).");

				// Make the tool drop its cached preview geometry so the new
				// radius is visible immediately.
				object instance = _instanceProperty?.GetValue(null);
				if (instance != null)
				{
					_invalidateGeometryMethod?.Invoke(instance, null);
				}

				// Persist the change once scrolling stops, mirroring Area Bulldozer's
				// own debounced save behaviour.
				_savePending = true;
				_saveAt = Time.unscaledTime + SAVE_DELAY_SECONDS;
			}
			catch (Exception ex)
			{
				Hotkey.debugLogger.WarnWithLine(
					$"AreaBulldozerIntegration.ChangeRadius failed: {ex.Message}");
			}
		}

		private void Resolve()
		{
			try
			{
				Type modType = null;
				Type toolType = null;
				Assembly modAssembly = null;

				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetName().Name != ModAssemblyName)
					{
						continue;
					}

					modAssembly = assembly;
					modType = assembly.GetType(ModTypeName);
					toolType = assembly.GetType(ToolTypeName);
					break;
				}

				if (modAssembly == null)
				{
					// Not installed (yet). Retried later in case load order changes.
					return;
				}

				Hotkey.debugLogger.InfoWithLine(
					$"AreaBulldozerIntegration: found assembly " +
					$"{modAssembly.GetName().Name} (version " +
					$"{modAssembly.GetName().Version}).");

				if (modType == null || toolType == null)
				{
					_resolved = true;

					Hotkey.debugLogger.WarnWithLine(
						$"AreaBulldozerIntegration: assembly is loaded but types " +
						$"'{ModTypeName}' / '{ToolTypeName}' were not found. " +
						"Brush scroll integration disabled.");

					return;
				}

				_settingsProperty = modType.GetProperty(
					"Settings",
					BindingFlags.Public | BindingFlags.Static);

				_brushRadiusProperty = _settingsProperty?
					.PropertyType
					.GetProperty("BrushRadius");

				_instanceProperty = toolType.GetProperty(
					"Instance",
					BindingFlags.Public | BindingFlags.Static);

				_invalidateGeometryMethod = toolType.GetMethod(
					"InvalidateSelectionGeometry",
					Type.EmptyTypes);

				if (_settingsProperty == null ||
				    _brushRadiusProperty == null ||
				    _instanceProperty == null)
				{
					_resolved = true;

					Hotkey.debugLogger.WarnWithLine(
						"AreaBulldozerIntegration: API mismatch. " +
						$"Settings={_settingsProperty != null}, " +
						$"BrushRadius={_brushRadiusProperty != null}, " +
						$"Instance={_instanceProperty != null}. " +
						"Brush scroll integration disabled.");

					return;
				}

				_isAvailable = true;
				_resolved = true;

				Hotkey.debugLogger.InfoWithLine(
					"AreaBulldozerIntegration: Area Bulldozer detected. " +
					$"Settings.{_brushRadiusProperty.Name} is " +
					$"{_settingsProperty.PropertyType.Name}, " +
					$"InvalidateSelectionGeometry=" +
					$"{_invalidateGeometryMethod != null}. " +
					"Ctrl + scroll wheel will resize its selection brush.");
			}
			catch (Exception ex)
			{
				Hotkey.debugLogger.WarnWithLine(
					$"AreaBulldozerIntegration.Resolve failed: {ex}");
			}
		}
	}
}
