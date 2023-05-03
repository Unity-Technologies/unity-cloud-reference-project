---
uid: styling
---

# Styling

App UI Unity Package provides a set of pre-defined styles 
based on the CSS App UI library. 
These styles can be used to quickly style your UI components, 
without the need to create custom styles from scratch.

The package uses the [BEM](http://getbem.com/)
naming convention for its [USS](xref:UIE-USS) 
(Unity Style Sheet) classes. 
This makes it easy to create and apply consistent styles to different elements in your UI.
You can use these classes to define styles for individual UI elements 
or to create custom stylesheets for your entire app.

In addition, App UI provides a set of pre-defined themes to help you 
quickly style your UI. These themes are designed to follow [App UI 
Design System guidelines](https://services.docs.internal.unity3d.com/unity-app-ui/docs/fundamentals/color/#theme)
and ensure consistency throughout your app. 
You can choose from several color schemes and typography styles that best 
fit your app's visual style. For more advanced theming options, you can also 
customize the themes or create your own from scratch. 
The [Theming](xref:theming) section goes into more detail on how to use and customize the provided 
themes.

## Using App UI Styles

When using the default App UI theme (`App UI.tss`), 
the default styling will be applied to all UI elements in your app.

Each component styling is defined in a separate stylesheet.
And every stylesheet is referenced into the `App UI.uss` stylesheet.

## Using Custom Styles

You can create your own custom stylesheets to override the default styles.
You can follow the steps given in the [theming](xref:theming) section to create your own custom stylesheets.

Then use provided stylesheet per component as reference to create your own styling.

Here is an example with the button component:

```css
/* Button.uss */
.appui-button {
    padding: 0 var(--appui-spacing-100);
}
```

> [!NOTE]
> It is good practice to use USS variables coming from our **Themes** folder 
> since they are designed to follow the App UI Design System guidelines.

