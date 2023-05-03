---
uid: ui-kit
---

# UI Kit

The App UI Unity package provides a UI Kit sample project that 
demonstrates how to use the various UI components and features of the package. 
This sample project can be used as a starting point for creating your own UI using 
the App UI Unity package.

## Getting Started

### Installation

To use the UI Kit sample, you will need to have this package installed in your project. 

To install the package, follow the instructions in the [Installation and Setup](xref:setup)
section of the documentation.

Inside the Unity Package Manager window, select the **Replica App UI** package, then 
go to **Samples** and select **UI Kit**. Click **Install** to install the sample.

### Usage

To open the sample, in your Project panel go to 
**Assets > Samples > Replica App UI > UI Kit > Scenes**.

The folder contains 3 different scenes:
* **UI Kit** - This scene contains all the UI components and features of the package.
* **UI Kit WS** - This is the same scene as the **UI Kit** scene, but its UI Document 
  is displayed in [World Space](xref:UnityEngine.Dt.App.Core.WorldSpaceUIDocument).
* **UI Kit Transparent** - A special scene where the app user-interface background is transparent
  and doesn't block raycast in the 3D scene. It's a good example for making 
  overlays and HUD.

The **UI Kit** sample uses a custom theme derived from the App UI default theme.
It can be a good starting point for creating your own custom theme.
The sample also contains a **Story** which can be accessed in the **Storybook** window.

For more information about stories, see the [Storybook](xref:storybook) documentation.
