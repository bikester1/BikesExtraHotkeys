using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI.Menu;
using System.Linq;
using System;
using System.Reflection;
using KSExtraHotkey.Models.Ui;
using KSExtraHotkey.Constants;

namespace KSExtraHotkey.Settings
{
    [FileLocation(Global.Settings)]
    [SettingsUIGroupOrder(gOpenToolsKeybindings, gRoadToolModeKeybindings, gZonesToolModeKeybindings, gSnappingKeybindings, gGeneral, gToolRelated, gAbout)]
    [SettingsUIShowGroupName(gOpenToolsKeybindings, gRoadToolModeKeybindings, gZonesToolModeKeybindings, gSnappingKeybindings, gGeneral, gToolRelated, gAbout)]

    public partial class ModSettings : ModSetting
    {
        #region Sections

        public const string sToolKeybindings            = "ToolKeybindings";
        public const string sGeneral                    = "General";

        #endregion

        #region Group Names

        public const string gOpenToolsKeybindings       = "OpenTools";
        public const string gRoadToolModeKeybindings    = "ToolModesRoads";
        public const string gZonesToolModeKeybindings   = "ToolModesZones";
        public const string gSnappingKeybindings        = "SnappingOptions";

        public const string gGeneral                    = "General";
        public const string gToolRelated                = "ToolRelated";
        public const string gAbout                      = "About";

        #endregion

        public ModSettings(IMod mod) : base(mod)
        {
        }

        

        #region Hidden Settings



        #endregion

        #region Road Tool Mode

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadStraight, BindingKeyboard.None, RoadToolActions.RoadToolStraight)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadStraight { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadSimpleCurve, BindingKeyboard.None, RoadToolActions.RoadToolSimpleCurve)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadSimpleCurve { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadComplexCurve, BindingKeyboard.None, RoadToolActions.RoadToolComplexCurve)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadComplexCurve { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadContinuous, BindingKeyboard.None, RoadToolActions.RoadToolContinuous)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadContinuous { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadGrid, BindingKeyboard.None, RoadToolActions.RoadToolGrid)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadGrid { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.RoadReplace, BindingKeyboard.None, RoadToolActions.RoadToolReplace)]
        [SettingsUISection(sToolKeybindings, gRoadToolModeKeybindings)]
        public ProxyBinding RoadReplace { get; set; }

        #endregion

        #region Zone Tool Mode

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.ZoneFill, BindingKeyboard.None, ZoneToolActions.ZoneToolFill)]
        [SettingsUISection(sToolKeybindings, gZonesToolModeKeybindings)]
        public ProxyBinding ZoneFill { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.ZoneMarquee, BindingKeyboard.None, ZoneToolActions.ZoneToolMarquee)]
        [SettingsUISection(sToolKeybindings, gZonesToolModeKeybindings)]
        public ProxyBinding ZoneMarquee { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.ZonePaint, BindingKeyboard.None, ZoneToolActions.ZoneToolPaint)]
        [SettingsUISection(sToolKeybindings, gZonesToolModeKeybindings)]
        public ProxyBinding ZonePaint { get; set; }

        #endregion

        #region Snapping Tools

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingExistingGeometry, BindingKeyboard.None, SnappingActions.ExistingGeometry)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToExistingGeometryKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingCellLength, BindingKeyboard.None, SnappingActions.CellLength)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToCellLengthKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingStraightDirection, BindingKeyboard.None, SnappingActions.StraightDirection)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapTo90DegreeAnglesKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingObjectSide, BindingKeyboard.None, SnappingActions.BuildingSides)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToBuildingSidesKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingGuideLines, BindingKeyboard.None, SnappingActions.Guidelines)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToGuidelinesKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingNearbyGeometry, BindingKeyboard.None, SnappingActions.NearbyGeometry)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToNearbyGeometryKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingZoneGrid, BindingKeyboard.None, SnappingActions.ZoneGrid)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToZoneGridKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.SnappingNetSide, BindingKeyboard.None, SnappingActions.RoadSides)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding SnapToRoadSidesKeyBinding { get; set; }

        [UIAttributes.CustomUIExtendedKeybinding(IconPath.ShowContourLines, BindingKeyboard.None, SnappingActions.ShowContourLines)]
        [SettingsUISection(sToolKeybindings, gSnappingKeybindings)]
        public ProxyBinding ShowContourLinesKeyBinding { get; set; }

        #endregion

        #region Generals

        [SettingsUISection(sGeneral, gGeneral)]
        public bool EnableMod { get; set; } = true;

        [SettingsUISection(sGeneral, gGeneral)]
        [SettingsUIConfirmation()]
        public bool ResetBindings
        {
            set
            {
                Hotkey.Logger.Info("ResetKey");
                ResetKeyBindings();
            }
        }

        #endregion

        #region Brush & Evevation

        [SettingsUISection(sGeneral, gToolRelated)]
        public bool EnableElevationScroll { get; set; }

        [SettingsUISection(sGeneral, gToolRelated)]
        public bool EnableElevationStepScroll { get; set; }

        [SettingsUISection(sGeneral, gToolRelated)]
        public bool EnableResetElevation { get; set; }

        [SettingsUIMouseBinding(BindingMouse.Right, nameof(ResetElevation), ctrl: true)]
        [SettingsUIHidden]
        public ProxyBinding ResetElevation { get; set; }

        [SettingsUISection(sGeneral, gToolRelated)]
        public bool EnableBrushSizeScroll { get; set; }

        [SettingsUISection(sGeneral, gToolRelated)]
        public bool EnableBrushStrengthScroll { get; set; }

        #endregion

        #region About

        [SettingsUISection(sGeneral, gAbout)]
        public string ModVersion => $"V{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        #endregion

        #region Methods

        public override AutomaticSettings.SettingPageData GetPageData(string pageId, bool addPrefix)
        {
            return GeneratePage(pageId, addPrefix);
        }

        public override void SetDefaults()
        {
            try
            {
                EnableMod = true;
                EnableElevationScroll = false;
                EnableElevationStepScroll = false;
                EnableResetElevation = false;
                EnableBrushSizeScroll = false;
                EnableBrushStrengthScroll = false;

                var properties = GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(ProxyBinding));

                foreach (var property in properties.ToList())
                {
                    property.SetValue(this, new ProxyBinding());
                }
            }
            catch (Exception ex)
            {
                Hotkey.Logger.Error(ex.Message);
            }
        }

        #endregion
    }
}
