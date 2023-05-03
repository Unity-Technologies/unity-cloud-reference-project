---
uid: iconography
---

# Iconography

The Iconography category includes more than 200 icons that follow the 
[App UI design system guidelines](https://services.docs.internal.unity3d.com/unity-app-ui/docs/fundamentals/Iconography/).
The icons are provided as PNG files and are set up to be loaded via USS classes.
The Icon UI component uses these USS classes to load the appropriate icon.

## Usage

The [Icon](xref:UnityEngine.Dt.App.UI.Icon) UI component is used to display an icon.

```xml
<Icon name="icon-name" />
```

Note that by default, all icons are white.
To change the color of an icon, you can use the `--unity-image-tint-color` custom USS property.
For example, to tint the add icon blue, you would use the following code:

```css
.icon-blue {
    --unity-image-tint-color: blue;
}
```

## About Vector Graphics

We are working on integrating Vector Graphics support for icons in a future release.
This will allow for more customizable features, such as custom thickness and multi-color support.

Stay tuned for updates on this feature!

