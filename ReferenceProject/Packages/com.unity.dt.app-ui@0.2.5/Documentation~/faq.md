---
uid: faq
---

# Frequently Asked Questions

## Why I can't see App UI components in the UI Builder components library?

By default, the UI Builder components library only shows the components that are available outside of
the `UnityEngine.*` namespace. If you want to see the App UI components, you need to enable
the **Developer Mode** of the Unity Editor.

To enable the **Developer Mode** of the Unity Editor, open the **About Unity** window and
while focusing on the **About Unity** window, type `internal` on your keyboard. This will
prompt you to restart the Unity Editor. After restarting the Unity Editor, you will be able to
see the App UI components in the UI Builder components library.

## In UI Builder, why App UI components look unstyled/broken?

The UI Builder use a default theme to display the visual tree inside the Preview panel.
This theme is useful for Editor windows development but it doesn't match the App UI theme.

To use the App UI theme in the UI Builder, choose the **App UI** theme in the **Theme** dropdown,
or a more specific theme like **App UI Dark - Medium** or **App UI Light - Medium**.

## I see an error message "Exception: Attempting to use an invalid operation handle" when I go in Play Mode

This error message is caused by an invalid load of Addressables content. 
App UI uses Addressables via the Localization Unity Package only for the localization of text elements.
To avoid this message, you have 2 options:
- Go to **Edit > Project Settings > Localization** and check the **Initialize Synchronously** option.
- Upgrade the Localization package version used by your project (1.4.3 for example seems to have this issue fixed).