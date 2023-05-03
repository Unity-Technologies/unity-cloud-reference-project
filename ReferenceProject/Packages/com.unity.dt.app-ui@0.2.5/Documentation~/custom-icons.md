---
uid: custom-icons
---

# Custom Icons

Every icon provided by App UI is available as a PNG file.
They are all referenced inside USS files, which are used to load the icons.

## Adding Custom Icons

To add a custom icon, you need to add a PNG file to your project and reference it in a USS file.

Here's an example of how to add a custom icon named `home`:

```css
.appui-icon--home {
    --unity-image: url("path/to/home.png");
}
```

> [!IMPORTANT]
> Your USS class name must start with `appui-icon--` followed by the name of your icon
> in order to work with the [Icon](xref:UnityEngine.Dt.App.UI.Icon) UI component.

