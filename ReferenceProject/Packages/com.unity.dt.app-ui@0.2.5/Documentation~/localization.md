---
uid: localization
---

# Localization

## Introduction

App UI provides localization features through the [**Localization**](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/index.html) 
Unity package from UPM.

## ContextProvider

The [ContextProvider](xref:UnityEngine.Dt.App.UI.ContextProvider) element is a component that provides the [ApplicationContext](xref:UnityEngine.Dt.App.Core.ApplicationContext) to its children.
With this component, you can define the global context for your application, or override the context for a specific part of your UI.
In terms of localization, the [ContextProvider](xref:UnityEngine.Dt.App.UI.ContextProvider) is used to define the current locale identifier for the current scope of the application.

> [!NOTE]
> For more information about the ContextProvider, see the [ContextProvider documentation](xref:contexts).

Here is an example of how to get a the localized string of a given entry using the locale defined inside the [ApplicationContext](xref:UnityEngine.Dt.App.Core.ApplicationContext):

```csharp

using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

public class MyComponent : VisualElement
{
    [...]

    public void OnButtonClick()
    {
        var locale = this.GetContext().locale;
        var translatedString = LocalizationSettings.Instance.GetLocalizedString("table_name", "entry_key", locale);
    }
}

```

Note that the Localization Unity package also provides features for pluralization and formatting of localized strings.
You can find more information about these features in [their package documentation](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/index.html).

## App UI Localized elements

Every App UI element that displays text supports localization.
If an element has a `text`, `title` or `label` property for example, you can define its value with a string starting with `@`,
which enables localization for the element. The string value is then used as the resource key for the localized string.
If the resource key is not found, the string value is used as the displayed text.

The naming convention for the resource key is the following:

```
@<table_name>:<entry_key>
```

Here is an example of how to use localization with a button:

* UXML
  ```xml
  <appui:Button title="@table_name:entry_key" />
  ```

* C#
  ```csharp
  myButton.title = "@table_name:entry_key";
  ```
