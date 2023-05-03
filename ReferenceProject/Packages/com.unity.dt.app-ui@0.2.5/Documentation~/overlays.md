---
uid: overlays
---

# Overlays

Overlays are UI components that are displayed on top of the existing UI, 
often used to display extra information, modal dialogs, or notifications. 
App UI provides an overlay system to facilitate the creation of these components.

## The layering system

App UI's overlay system uses a layering system to determine the z-order of the overlays. 
Each overlay belongs to a specific layer, 
and the layers are ordered from lowest to highest. 
Overlays in higher layers are displayed on top of overlays in lower layers.

The following layers are defined by default:
- `main-content`: The main content of the application. 
  This is the default layer for the UI.
- `popup`: The layer for popups, such as modal dialogs or menus.
- `notification`: The layer for notifications, such as toasts or banners.
- `tooltip`: The layer for tooltips.

## Overlay components

The [Popup](xref:UnityEngine.Dt.App.UI.Popup) 
class is the base class for all overlay components in App UI. 
It provides the basic functionality for displaying and hiding an overlay.

To create and show a Popup element in your UI,
you need to use the `Build` method associated to the desired Popup type.

```csharp
var popover = Popover.Build(target, content)
        .SetPlacement(placement)
        .SetShouldFlip(shouldFlip)
        .SetOffset(offset)
        .SetCrossOffset(crossOffset)
        .SetArrowVisible(showArrow)
        .SetContainerPadding(containerPadding)
        .SetOutsideClickDismiss(outsideClickDismissEnabled)
        .SetModalBackdrop(modalBackdrop)
        .SetKeyboardDismiss(keyboardDismissEnabled);
popover.Show();

var modal = Modal.Build(content)
        .SetFullScreenMode(ModalFullScreenMode.None)
        // ...
```

### Popover

The [Popover](xref:UnityEngine.Dt.App.UI.Popover) 
class is a popup that is displayed next to a target element.
It can display a small arrow pointing to this target.

By default, the popover's backdrop is transparent,
and doesn't block the user from interacting with the rest of the UI.
However, you can configure the popover to display a backdrop that works as a modal one.


> [!IMPORTANT]
> On small screen devices, it is recommended to use 
> a [Tray](xref:UnityEngine.Dt.App.UI.Tray) instead of a Popover in most cases.

### Modal

The [Modal](xref:UnityEngine.Dt.App.UI.Modal)
class is a popup that is displayed as a modal dialog.
It displays a backdrop that blocks the user from interacting with the rest of the UI.
The dialog is positioned in the center of the screen. Its size is configurable,
and can take over the whole screen if needed.

By definition, you should use a Modal to display a dialog 
that requires user interaction/decision.

See [Dialogs](xref:layouts#dialogs) for more information.

### Tray

The [Tray](xref:UnityEngine.Dt.App.UI.Tray)
class is a popup that is displayed at the bottom of the screen.
It displays a backdrop that blocks the user from interacting with the rest of the UI.
The tray is positioned at the bottom of the screen, and its height is configurable.

The Tray is mainly used on small screen devices,
to display a menu or a list of actions.


### Notifications

App UI provides a Notification system based on a single message queue.
That means you can use different [UIDocument](xref:UnityEngine.UIElements.UIDocument) 
to trigger and display notifications.

For now, only Toasts are supported.

#### Toast

The [Toast](xref:UnityEngine.Dt.App.UI.Toast)
class is a popup that is displayed at the edge of the screen.
A Toast that doesn't require user interaction/decision is displayed for a few seconds,
and then automatically hides itself.
A Toast that requires user interaction/decision is displayed until the user interacts with it.

> [!NOTE]
> App UI decided to follow the Android design guidelines for Toasts.
> So only one Toast can be displayed at a time,
> and the new Toast will replace the previous one (even if it required user interaction).


### Tooltip

The [Tooltip](xref:UnityEngine.Dt.App.UI.Tooltip)
class is a popup that is displayed next to a target element.
The tooltip doesn't require user interaction/decision,
and is displayed as long as the target element is hovered.

In our layering system, the Tooltip layer is the highest one.

