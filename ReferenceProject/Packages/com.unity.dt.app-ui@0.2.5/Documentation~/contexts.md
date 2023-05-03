---
uid: contexts
---

# Contexts

Contexts in App UI are a powerful way to manage application 
state in a more predictable and testable way. 
Instead of using global variables or singletons, 
you can encapsulate state and behavior into smaller, 
more focused units called contexts.

## ApplicationContext

The [ApplicationContext](xref:UnityEngine.Dt.App.Core.ApplicationContext)
is a special context in App UI that provides access to some 
global settings of the application, such as the current theme, language, and scale. 
You can use these settings to customize the appearance and behavior of your UI, 
depending on the user's preferences.

You can access the scoped 
[ApplicationContext](xref:UnityEngine.Dt.App.Core.ApplicationContext)
for a given component by calling the method 
[GetContext](xref:UnityEngine.Dt.App.UI.VisualElementExtensions).

### Theme

The theme context provides a way to customize the appearance of the UI. 
You can switch between different themes at runtime, and the UI will 
automatically update to reflect the new settings.

Internally the theme context is defined as a USS class.
For example, if the current theme is `dark`, the context will be defined as `dark`
and the corresponding USS class `appui--dark` will be applied to the context provider element 
that sets the context.

### Language

The language context provides a way to translate your UI into different languages. 
The context will give you the current locale identifier,
which you can use to load the correct translation for your UI.

> [!NOTE]
> The language context works in conjunction with the 
> [Unity Localization Package](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/index.html).
> 
> Localized text elements provided by App UI will automatically
> update their text when the language context changes.

### Scale

The scale context provides a way to adjust the size of the UI based on the device's 
pixel density. You can define different scaling factors for different devices, 
and the UI will automatically adjust to the appropriate size.

More info about Platform Scale can be found in the 
[Platform Scale](https://services.docs.internal.unity3d.com/unity-app-ui/docs/fundamentals/platform-scale/) documentation page.

### Panel

You can retrieve the instance of the root [App UI Panel](xref:UnityEngine.Dt.App.UI.Panel)
from the application context. This is useful if you want to access the panel's
properties or methods from a component that is not a direct child of the panel.

## ContextProvider

The [ContextProvider](xref:UnityEngine.Dt.App.UI.ContextProvider) 
is a component that allows you to create and manage your own contexts. 
You can use it to encapsulate state and behavior into smaller,
more focused units that can be reused across your application.

## App UI Panel - The root context provider

The [App UI Panel](xref:UnityEngine.Dt.App.UI.Panel) is a special
[ContextProvider](xref:UnityEngine.Dt.App.UI.ContextProvider) that is
must be added to the root of the UI. It provides access to the global 
[ApplicationContext](xref:UnityEngine.Dt.App.Core.ApplicationContext)
for this visual tree.