# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.5] - 2021-03-30

### Added

- Added `SnapTo` and `GoTo` methods in the SwipeView component.
- Added `startSwipeThreshold` property in the SwipeView component.
- Added Scrollable Manipulator which uses a threshold before beginning to scroll.
- Added Pressable Manipulator which can be used to capture a pointer during press but can continue propagation of Move and Up events.

### Fixed

- Removed the use of System.Linq in the App UI package.

### Changed

- SwipeView now uses the Scrollable Manipulator instead of Draggable.

## [0.2.4] - 2021-03-23

### Fixed

- Fixed Localization initialization for WebGL platform.
- Fixed ScrollView tracker styling.
- Fixed Focusable property in constructor for newer versions of Unity.
- Fixed UnityEngine namespace import for newer versions of Unity.

## [0.2.3] - 2021-03-22

### Fixed

- Fixed styling on Scroller.
- Fixed max-height on Dropdown menu.
- Fixed dismissible dialogs in UI Kit sample.
- Fixed flex-shrink value on multiple components.
- Fixed TextField UXML construction.

## [0.2.2] - 2021-03-20

### Fixed

- Fixed NullReferenceException in invalid AnchorPopup updates.

## [0.2.1] - 2021-03-17

### Fixed

- Fixed the localization of the Dropdown component.
- Fixed the sizing of Progress UI components.
- Fixed OutOfBounds dismiss on cascading Popovers.
- Fixed the blocking of placeholder events on TextField and TextArea.
- Fixed compilation error on 2022.2.0a8.

## [0.2.0] - 2023-03-05

## Added

- Added support of Native features such as system themes and scale.
  The support has been done for Android, iOS, MacOS, and Windows.
- Added support of the [Unity Localization package](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/index.html).
  You can localize strings from the Application Context or use a
  [LocalizedTextElement](xref:UnityEngine.Dt.App.UI.LocalizedTextElement) component to localize strings from the UI.
- Added a global UI component property `preferred-tooltip-position` to set the default tooltip position.
  The property is available by code and UXML.
- Added a App UI manager with a singleton pattern to manage the App UI configuration and lifecycle.
- Added an option in the App UI Settings to auto-override the Android manifest file (Android builds only).
- Added an option in the App UI Settings to enable or disable the usage of a custom loop frequency (Editor only).
- Added a World Space UI Document component to display UI elements in the world space.
- Added Avatar UI component to display an avatar with a name and a status.
- Added a Badge UI component to display a badge with a number.
- Added a BoundsField UI component to define a three-dimensional bounding box.
- Added a BoundsIntField UI component to define a three-dimensional bounding box with integer values.
- Added a Chip UI component to display a chip with a label and an icon.
- Added a CircularProgress UI component to display a circular progress bar.
- Added a LinearProgress UI component to display a linear progress bar.
- Added a ColorField UI component to define a color.
- Added a ColorSlider UI component to choose a value from a range of colors.
- Added a ColorSwatch UI component to display a color.
- Added a ColorWheel UI component to choose a hue from a color wheel.
- Added a ColorPicker UI component to choose a color from a color wheel and a color slider.
- Added a DoubleField UI component to define a double value.
- Added a Drawer UI component to display additional content from the sides of the screen.
- Added the support of `box-shadow` and `outline` using custom USS properties (see [ExVisualElement](xref:UnityEngine.Dt.App.UI.ExVisualElement)).
- Added a IconButton UI component to display an icon button.
- Added a LocalizedTextElement UI component to display a localized text.
  Most of the App UI components have been updated to use this component.
- Added a LongField UI component to define a long value.
- Added a Mask UI component to fill an area with a solid color.
- Added a PageIndicator UI component to display dots in a pagination system.
- Added a RectField UI component to define a two-dimensional rectangular area.
- Added a RectIntField UI component to define a two-dimensional rectangular area with integer values.
- Added a TextArea UI component to display a scrollable text area.
- Added the support of expression evaluation in numeric fields (see [ExpressionEvaluator](xref:UnityEditor.ExpressionEvaluator)).
- Added an [ActionTriggered UITK event](xref:UnityEngine.Dt.App.UI.ActionTriggeredEvent) that can be triggered by Menu items.
- Added a StackView UI component to display and animate a stack of items.
- Added a SwipeView UI component to display a list of items that can be swiped in a direction.
- Added a PageView UI component which is the combination of a SwipeView and a PageIndicator.
- Added the [Submittable](xref:UnityEngine.Dt.App.UI.Submittable)
  UI-Toolkit manipulator to handle the submission of Action UI elements via keyboard/mouse/pointer.
- Added the [KeyboardFocusController](xref:UnityEngine.Dt.App.UI.KeyboardFocusController)
  UI-Toolkit manipulator to differentiate the focus of a UI element from the keyboard or the pointer.
- Added a MenuBuilder class to create a Menu from code.
- Added the ability for Popover element to use a modal backdrop (Pointer events are blocked).
- Added a simple implementation of a [Storybook](xref:storybook)-like tool to display and test UI components.


## Changed

- The **Application** UI element is now called [Panel](xref:UnityEngine.Dt.App.UI.Panel).
- Improved the Slider UI component to display the current value.
- Improved the Tray UI element to be resizable.
- Ability to use the Modal component with any content derived from [VisualElement](xref:UnityEngine.UIElements.VisualElement).
- The App UI main Looper is now part of the App UI manager and is
  not present in the Application UI element anymore.


## Fixed

- Fixed the Notification system when the Notificiation UI element has been destroyed without being dismissed.
- Fixed the Menu system to handle sub-menus.
- Fixed the position calculator of Popups.
- Fixed the Popup system to be able to dismiss a popup when clicking outside of it in an area that is not handled by UI-Toolkit.
- Fixed the formatting of numerical fields.
- Fixed the handle of some edge-cases in TooltipManipulator.


## [0.1.0] - 2022-08-19

### Added
- Package Structure
- First draft of User Manual documentation
- Accordion UI Component
- ActionBar UI Component
- ActionButton UI Component
- ActionGroup UI Component
- Button UI Component
- Checkbox UI Component
- Divider UI Component
- Dropdown UI Component
- NumericalField UI Components
- Header UI Component
- Icon UI Component
- Radio UI Component
- Slider UI Component
- Stepper UI Component
- Tabs UI Component
- Text UI Component
- TextField UI Component
- Toggle UI Component
- TouchSlider UI Component
- VectorField UI Components
- Dialogs & Alerts UI System
- Menu UI System
- Popup UI System
- Notification Manager & Toasts
- Message Queue System (Looper & Handler)
- ContextProvider System
- Tooltip System
