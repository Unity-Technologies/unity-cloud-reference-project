using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

[assembly: InternalsVisibleTo("Unity.DigitalTwins.AppUI.Tests")]
[assembly: InternalsVisibleTo("Unity.DigitalTwins.AppUI.Editor")]
#if UNITY_EDITOR
[assembly: UxmlNamespacePrefix("UnityEngine.Dt.App.UI", "appui")]
#endif
