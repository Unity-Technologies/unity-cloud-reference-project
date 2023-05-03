---
uid: typography
---

# Typography

App UI Unity Package includes typography styles that follow the [App UI Design System](https://services.docs.internal.unity3d.com/unity-app-ui/docs/fundamentals/typography/).
These styles are defined using Unity stylesheets (USS) and are based on a modular scale.
They include various font sizes, weights, and line heights to ensure that text is readable and consistent throughout the application.

## Font Family

The font family used in the App UI Design System is [Inter](https://rsms.me/inter/).
Font assets are included in the App UI Unity Package and are automatically loaded when the package is imported.

## Usage

App UI provide a set of UI components to display text. 
All these components are based on the [LocalizedTextElement](xref:UnityEngine.Dt.App.UI.LocalizedTextElement) component,
which gives you the ability to localize them. See the [Localization](xref:localization) page for more information.

### Text

The [Text](xref:UnityEngine.Dt.App.UI.Text) component is used to display general purpose text.

```xml
<appui:Text text="Text" />
```

### Heading

The [Heading](xref:UnityEngine.Dt.App.UI.Header) component is used to display headings. 
It is a variant of the [Text](#text) component that uses a larger font size.

```xml
<appui:Header text="Heading" />
```

## Customization

If the provided typography styles do not meet your needs, you can customize the USS file to add your own font families,
font sizes, weights, and line heights. Simply create a new USS file and define your custom styles.
You can then apply them to text elements in your application.

For more information on customization in general, see the [Styling](xref:styling) page.

