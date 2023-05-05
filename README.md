# Unity Cloud Reference Project

<img src="https://img.shields.io/badge/unity-2021.3.15-green.svg?style=flat-square" alt="unity 2021.3.15f1">

## A Unity project for building a collaborative 3D design review tool

> **Important**: Some services used in this project require closed beta access.

The Unity Cloud Reference Project is a fully functional collaborative design review tool for 3D assets, powered by Unity services. It's a Unity project intended to be built upon so you can easily create your own custom viewer application, while still being able to pull in the latest changes from this base project.

> **Important**: The Unity Cloud Reference Project is compatible with the latest Unity Long Term Support (LTS), currently [2021 LTS](https://unity.com/releases/2021-lts).

The Unity Cloud Reference Project has been developed and tested on the following platforms:

- Windows
- MacOS
- iOS
- Android
- WebGL
- Tethered VR headsets

To connect and find support, join the [Unity forum](https://forum.unity.com/forums/unity-cloud.868/) (requires beta access).

## Contents

- [Unity Cloud Reference Project](#unity-cloud-reference-project)
  - [A Unity project for building a collaborative 3D design review tool](#a-unity-project-for-building-a-collaborative-3d-design-review-tool)
  - [Contents](#contents)
  - [Overview](#overview)
  - [Getting the project](#getting-the-project)
    - [Direct download](#direct-download)
  - [Configure Unity services](#configure-unity-services)
    - [Sign in or create a Unity account](#sign-in-or-create-a-unity-account)
    - [Register your application](#register-your-application)
    - [Upload assets to review](#upload-assets-to-review)
  - [Opening the project for the first time](#opening-the-project-for-the-first-time)
  - [Exploring the project](#exploring-the-project)
    - [Logging in and accessing assets](#logging-in-and-accessing-assets)
    - [Viewing 3D assets](#viewing-3d-assets)
    - [Managing assets](#managing-assets)
  - [Troubleshooting](#troubleshooting)
  - [License](#license)
  - [Feedback](#feedback)

## Overview

The Unity Cloud Reference Project is a Unity project intended to be used as a starting point for building a collaborative 3D asset review tool. It can be built for many platforms, including WebGL, making it highly accessible and simple to share. The Unity Cloud Reference Project is designed to be highly customizable so you can add, remove, or change its features, tools, and UI; while still being able to pull in the latest updates with minimal conflicts.

The following online services are used in this project:

- authenticate with your Unity account
- list and select an asset from cloud storage
- stream a large 3D asset from cloud storage (vast support for 3D formats)
- collaborate online with voice chat and user avatars
- share a deep-link to an asset for others to view

**Other notable features**:

- Localization
- Customizable navigation modes
- Customizable settings
  - Performance info toggle
  - Localization (EN, FR)
- Customizable tools
- Customizable and themable UX
- VR Controls & UX

## Getting the project

### Direct download

You can download the latest version of the Unity Cloud Reference Project from our Releases page.
You can also, through the GitHub repository:

1. Select the **Code** button.
2. Select the **Download Zip** option.

## Configure Unity services

### Sign in or create a Unity account

1. Go to the [Unity Dashboard](https://dashboard.unity3d.com/digital-twins/) (requires beta access).
2. Sign in to register your app and manage your assets.

### Register your application

The Unity Cloud Reference Project requires an `App Id` in order to properly use the cloud services and to enable the custom URI scheme used in deep-linking and login operations.

> **Note**: The project will fail to build if you have not set your `App Id`.

#### Create an App Id

To create an `App Id`, follow these steps:

1. Log into the [Unity Dashboard](https://dashboard.unity3d.com/digital-twins/).
2. Select **Developer Hub** > **Registered Applications**.
3. Select the **+Register an application** button and follow instructions to complete the form.

#### Set the App Id in the Unity Editor

To use the `App Id`, follow these steps:

1. Open the Unity Cloud Reference Project in the Unity Editor.
2. Go to **Edit > Project Settings > Unity Cloud > App Registration**.
3. Enter your `App Id` in the **App Id** field.

4. Select **Refresh** to update the application data in the Unity Cloud Portal.

### Upload assets to review

To view assets in the Unity Cloud Reference Project you will first need to upload them. If other members of your organization have already done this, then you can get access to them once they have invited you to join their organization. You can switch organizations in the top-right corner of the dashboard.

> **Note**: Ensure you read through the supported file formats before uploading an asset.

To upload an asset:

1. Select the **New** button.
2. Name the asset.
3. From there, drag your 3D asset and its dependent files into the upload area.
4. Select **Create Digital Twin**.

Once the asset is uploaded and processed, you should be able to  stream through the Unity Cloud Reference Project.

## Opening the project for the first time

Once you have downloaded the project, follow these steps:

1. Check that you have installed the [2021 LTS Unity Editor](https://unity.com/releases/2021-lts).
2. To add the project to the Unity Hub, select **Add**.
3. Select the root folder of the downloaded project.

> **Note**: The first time you open the project, Unity will import all assets, including packages it depends on, which will take longer than usual.

4. [Set your App Id](#register-your-application)
5. Select **Play**.
6. Sign into your Unity account to view your uploaded assets.

## Exploring the project

### Logging in and accessing assets

1. Open the Unity Cloud Reference Project.
2. Log in to your Unity account to access your organization's 3D streamable assets.
3. After logging in, the project displays a list of 3D assets filtered by workspaces.
  If you have multiple workspaces, you can select another to view its assets.

### Viewing 3D assets

1. Select an asset from the list to open it in the 3D streaming viewer.
2. While viewing an asset, you can move the camera by:
   - selecting
   - touching
   - dragging it
   - zoom in or out
   - scrolling
   - pinching

</br> You can also change the navigation mode by selecting the icon in the bottom-left corner.

### Managing assets

1. To return to your asset list, select the folder button in the top-left corner.
2. If you need to access personal settings, select the cog icon in the top-right corner. From there, you can switch between English and French localization or toggle the display of frame rate information.
3. To sign out of your Unity account, select the avatar icon in the top-right corner.

## Troubleshooting
  
**Bugs**  
Report bugs in the Unity Cloud Reference Project using GitHub Issues. Report Unity Editor bugs using the [Unity bug submission process](https://unity3d.com/unity/qa/bug-reporting).
  
**Documentation**

Technical documentation for this project isn't yet available, but will be provided soon.
  
## License

See [LICENSE.md](LICENSE.md) for more legal information.

## Feedback

Thank you for taking a look at the project! To help us improve and provide greater value, please consider providing [feedback on our forum](https://forum.unity.com/forums/unity-cloud.868/) about your experience with the Unity Cloud Reference Project. Thank you!
