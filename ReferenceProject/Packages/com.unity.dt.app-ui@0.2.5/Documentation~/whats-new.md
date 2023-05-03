---
uid: whats-new
---

# What's New in **0.2.5**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the [Changelog](../changelog/CHANGELOG.html).

The main updates in this release include:

### Added

- Added `SnapTo` and `GoTo` methods in the SwipeView component.
- Added `startSwipeThreshold` property in the SwipeView component.
- Added Scrollable Manipulator which uses a threshold before beginning to scroll.
- Added Pressable Manipulator which can be used to capture a pointer during press but can continue propagation of Move and Up events.

### Fixed

- Removed the use of System.Linq in the App UI package.

### Changed

- SwipeView now uses the Scrollable Manipulator instead of Draggable.