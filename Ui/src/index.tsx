import { ReactElement } from "react";
import { Widget } from "cs2/bindings";
import { ModRegistrar } from "cs2/modding";
import { ExtendedKeybinding } from "widgets/extendedKeybinding";

const register: ModRegistrar = (moduleRegistry) => {

    // Vanilla OptionsUI component renderers ({"C# type": React widget renderer hook})
    const widgetComponents: Record<string, (data: Widget<any>) => ReactElement> = moduleRegistry.get("game-ui/menu/widgets/option-widget-renderer.tsx", "optionsWidgetComponents");
    // Add custom renderer for matching C# widget type
    widgetComponents['KSExtraHotkey.CustomOptionUIWidgets.Widgets+ExtendedKeybindingField']   = ExtendedKeybinding;
}

export default register;