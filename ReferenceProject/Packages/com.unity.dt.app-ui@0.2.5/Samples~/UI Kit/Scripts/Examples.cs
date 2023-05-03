using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Dt.App.Core;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Button = UnityEngine.Dt.App.UI.Button;
using Toggle = UnityEngine.Dt.App.UI.Toggle;
using Dropdown = UnityEngine.Dt.App.UI.Dropdown;

namespace UnityEngine.Dt.App.Samples
{
    public class Examples : MonoBehaviour
    {
        public UIDocument uiDocument;

        const int DELETE_ACTION = 99;

        const int DISMISS_ACTION = 1;

        // Start is called before the first frame update
        void Start()
        {
            if (uiDocument)
                SetupDataBinding(uiDocument.rootVisualElement);
        }

        void Update()
        {
            if (uiDocument)
            {
                var progressValue = Mathf.Repeat(Time.realtimeSinceStartup, 10f) / 10f;
                uiDocument.rootVisualElement.Q<CircularProgress>("determinateCircularProgress").value = progressValue;
                uiDocument.rootVisualElement.Q<CircularProgress>("determinateCircularProgressWithLabel").value = progressValue;
                uiDocument.rootVisualElement.Q<Text>("determinateCircularProgressLabel").text = $"{Mathf.RoundToInt(progressValue * 100f)}%";
                uiDocument.rootVisualElement.Q<LinearProgress>("determinateLinearProgress").value = progressValue;
            }
        }

        public static void SetupDataBinding(VisualElement root)
        {
            root.Q<ActionButton>("alert-trigger").clickable.clickedWithEventInfo += evt =>
            {
                if (evt.target is ActionButton btn)
                {
                    var dialog = new AlertDialog
                    {
                        title = "Delete 3 documents",
                        description = "The selected documents will be deleted.",
                        variant = AlertSemantic.Destructive
                    };
                    dialog.SetPrimaryAction(DELETE_ACTION, "Delete", () => Debug.Log("Deleted"));
                    dialog.SetCancelAction(DISMISS_ACTION, "Cancel");
                    var modal = Modal.Build(btn, dialog);
                    modal.dismissed += (modalElement, dismissType) => Debug.Log("Dismissed Alert");
                    modal.Show();
                }

            };

            var themeSwitcher = root.Q<RadioGroup>("theme-switcher");
            var scaleSwitcher = root.Q<RadioGroup>("scale-switcher");
            var panel = root.Q<Panel>("root-panel") ?? root.GetFirstAncestorOfType<Panel>();

            void OnSystemThemeChanged(string systemTheme)
            {
                panel.theme = systemTheme;
            }

            void SetTheme()
            {
                Platform.systemThemeChanged -= OnSystemThemeChanged;
                switch (themeSwitcher.value)
                {
                    case 0:
                        panel.theme = Platform.systemTheme;
                        Platform.systemThemeChanged += OnSystemThemeChanged;
                        break;
                    case 1:
                        panel.theme = "dark";
                        break;
                    case 2:
                        panel.theme = "light";
                        break;
                }
                PlayerPrefs.SetInt("theme", themeSwitcher.value);
            }

            void SetScale()
            {
                switch (scaleSwitcher.value)
                {
                    case 0:
                        panel.scale = "medium";
                        break;
                    case 1:
                        panel.scale = "large";
                        break;
                }
                PlayerPrefs.SetInt("scale", scaleSwitcher.value);
            }

            if (themeSwitcher != null)
            {
                themeSwitcher.RegisterValueChangedCallback(_ => SetTheme());
                themeSwitcher.SetValueWithoutNotify(PlayerPrefs.GetInt("theme", 1));
                SetTheme();
            }

            if (scaleSwitcher != null)
            {
                scaleSwitcher.RegisterValueChangedCallback(_ => SetScale());
                scaleSwitcher.SetValueWithoutNotify(PlayerPrefs.GetInt("scale", 0));
                SetScale();
            }

            var localizationVariables = new Dictionary<string, object>
            {
                { "playerName", "Toto" },
                { "appleCount", 3 }
            };
            root.Query<LocalizedTextElement>("playerAppleCount").ForEach(localizedTextElement =>
            {
                localizedTextElement.variables = new object[] { localizationVariables };
            });
            root.Query<LocalizedTextElement>("nameIs").ForEach(localizedTextElement =>
            {
                localizedTextElement.variables = new object[] { localizationVariables };
            });

            root.Q<Button>("default-slide-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Default, NotificationDuration.Short, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("informative-slide-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Informative, NotificationDuration.Short, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("positive-slide-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Positive, NotificationDuration.Short, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("warning-slide-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Warning, NotificationDuration.Short, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("negative-slide-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Negative, NotificationDuration.Short, AnimationMode.Slide, evt.target as VisualElement));


            root.Q<Button>("default-fade-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Default, NotificationDuration.Short, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("informative-fade-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Informative, NotificationDuration.Short, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("positive-fade-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Positive, NotificationDuration.Short, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("warning-fade-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Warning, NotificationDuration.Short, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("negative-fade-short-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Negative, NotificationDuration.Short, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("default-fade-long-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Default, NotificationDuration.Long, AnimationMode.Fade, evt.target as VisualElement));

            root.Q<Button>("default-slide-indef-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Default, NotificationDuration.Indefinite, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("informative-slide-indef-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Informative, NotificationDuration.Indefinite, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("positive-slide-indef-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Positive, NotificationDuration.Indefinite, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("warning-slide-indef-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Warning, NotificationDuration.Indefinite, AnimationMode.Slide, evt.target as VisualElement));

            root.Q<Button>("negative-slide-indef-toast-button")
                .clickable.clickedWithEventInfo += (evt => OpenToast(NotificationStyle.Negative, NotificationDuration.Indefinite, AnimationMode.Slide, evt.target as VisualElement));

            var dropdownSrc = new List<string>();
            
            for (var i = 1; i <= 100; i++)
            {
                dropdownSrc.Add($"Choice {i}");
            }

            root.Q<Dropdown>("dropdown1").bindItem = (item, i) => item.label = dropdownSrc[i];
            root.Q<Dropdown>("dropdown1").sourceItems = dropdownSrc;
            root.Q<Dropdown>("dropdown2").bindItem = (item, i) => item.label = dropdownSrc[i];
            root.Q<Dropdown>("dropdown2").sourceItems = dropdownSrc;

            root.Q<UI.Avatar>("avatar-with-picture").backgroundImage = new StyleBackground(Resources.Load<Texture2D>("example-avatar-pic"));

            root.Q<ColorSlider>("rainbow-slider").colorRange = new List<ColorEntry>
            {
                new ColorEntry(Color.red, 0),
                new ColorEntry(Color.yellow, 0.2f),
                new ColorEntry(Color.green, 0.45f),
                new ColorEntry(Color.cyan, 0.55f),
                new ColorEntry(Color.blue, 0.66f),
                new ColorEntry(Color.magenta, 0.85f),
                new ColorEntry(Color.red, 1f),
            };

            root.Q<ActionButton>("menu-action-code").clickable.clickedWithEventInfo += (evt =>
                OpenMenu((VisualElement)evt.target));

            root.Q<Button>("haptics-light-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.LIGHT);
            root.Q<Button>("haptics-medium-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.MEDIUM);
            root.Q<Button>("haptics-heavy-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.HEAVY);
            root.Q<Button>("haptics-success-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.SUCCESS);
            root.Q<Button>("haptics-error-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.ERROR);
            root.Q<Button>("haptics-warning-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.WARNING);
            root.Q<Button>("haptics-selection-btn").clicked += () => Platform.RunHapticFeedback(HapticFeedbackType.SELECTION);

            var leftDrawer = root.Q<Drawer>("left-drawer");
            root.Q<Button>("open-left-drawer-btn").clicked += leftDrawer.Open;

            var rightDrawer = root.Q<Drawer>("right-drawer");
            root.Q<Button>("open-right-drawer-btn").clicked += rightDrawer.Open;

            var permanentDrawer = root.Q<Drawer>("permanent-drawer");
            var temporaryDrawer = root.Q<Drawer>("temporary-drawer");
            var drawerVariantSwitcher = root.Q<RadioGroup>("drawer-variant-switcher");
            var openTempDrawerBtn = root.Q<Button>("open-temp-drawer-btn");
            openTempDrawerBtn.clicked += temporaryDrawer.Open;
            temporaryDrawer.AddToClassList(Styles.hiddenUssClassName);
            drawerVariantSwitcher.SetValueWithoutNotify(1);
            drawerVariantSwitcher.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case 0:
                        permanentDrawer.AddToClassList(Styles.hiddenUssClassName);
                        temporaryDrawer.RemoveFromClassList(Styles.hiddenUssClassName);
                        openTempDrawerBtn.SetEnabled(true);
                        break;
                    case 1:
                        temporaryDrawer.AddToClassList(Styles.hiddenUssClassName);
                        permanentDrawer.RemoveFromClassList(Styles.hiddenUssClassName);
                        openTempDrawerBtn.SetEnabled(false);
                        break;
                }
            });

            var swipeViewH = root.Q<SwipeView>("swipeview-horizontal");
            var swipePrevButtonH = root.Q<ActionButton>("swipeview-h-prev");
            var swipeNextButtonH = root.Q<ActionButton>("swipeview-h-next");
            var swipeFiveButtonH = root.Q<ActionButton>("swipeview-h-five");
            var swipeFiveSnapButtonH = root.Q<ActionButton>("swipeview-h-five-snap");
            
            swipePrevButtonH.SetEnabled(swipeViewH.canGoToPrevious);

            swipeViewH.RegisterValueChangedCallback(evt =>
            {
                swipePrevButtonH.SetEnabled(swipeViewH.canGoToPrevious);
                swipeNextButtonH.SetEnabled(swipeViewH.canGoToNext);
            });
            swipePrevButtonH.RegisterCallback<ClickEvent>(evt =>
                swipeViewH.GoToPrevious());
            swipeNextButtonH.RegisterCallback<ClickEvent>(evt =>
                swipeViewH.GoToNext());
            swipeFiveButtonH.RegisterCallback<ClickEvent>(evt => swipeViewH.GoTo(4));
            swipeFiveSnapButtonH.RegisterCallback<ClickEvent>(evt => swipeViewH.SnapTo(4));
            
            var swipeViewHW = root.Q<SwipeView>("swipeview-horizontal-wrap");
            var swipePrevButtonHW = root.Q<ActionButton>("swipeview-hw-prev");
            var swipeNextButtonHW = root.Q<ActionButton>("swipeview-hw-next");

            swipePrevButtonHW.SetEnabled(swipeViewHW.canGoToPrevious);

            swipeViewHW.RegisterValueChangedCallback(evt =>
            {
                swipePrevButtonHW.SetEnabled(swipeViewHW.canGoToPrevious);
                swipeNextButtonHW.SetEnabled(swipeViewHW.canGoToNext);
            });
            swipePrevButtonHW.RegisterCallback<ClickEvent>(evt =>
                swipeViewHW.GoToPrevious());
            swipeNextButtonHW.RegisterCallback<ClickEvent>(evt =>
                swipeViewHW.GoToNext());


            var swipeViewV = root.Q<SwipeView>("swipeview-vertical");
            var swipePrevButtonV = root.Q<ActionButton>("swipeview-v-prev");
            var swipeNextButtonV = root.Q<ActionButton>("swipeview-v-next");

            swipePrevButtonV.SetEnabled(false);

            swipeViewV.RegisterValueChangedCallback(evt =>
            {
                swipePrevButtonV.SetEnabled(swipeViewV.canGoToPrevious);
                swipeNextButtonV.SetEnabled(swipeViewV.canGoToNext);
            });
            swipePrevButtonV.RegisterCallback<ClickEvent>(evt =>
                swipeViewV.GoToPrevious());
            swipeNextButtonV.RegisterCallback<ClickEvent>(evt =>
                swipeViewV.GoToNext());

            var swipeViewD = root.Q<SwipeView>("swipeview-distance");
            swipeViewD.beingSwiped += OnBeingSwiped;

            var img = Resources.Load<Texture2D>("example-avatar-pic");
            root.Q<Chip>("filled-chip-ornament").ornament = new Image
            {
                image = img,
                style =
                {
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent))
                }
            };

            root.Q<Chip>("outlined-chip-ornament").ornament = new Image
            {
                image = img,
                style =
                {
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    height = new StyleLength(new Length(100, LengthUnit.Percent))
                }
            };

            var tabViewTabs = root.Q<Tabs>("tabview-tabs");
            var tabViewSwiperView = root.Q<SwipeView>("tabview-swipeview");
            tabViewTabs.RegisterValueChangedCallback(evt =>
            {
                tabViewSwiperView.SetValueWithoutNotify(evt.newValue);
            });
            tabViewSwiperView.RegisterValueChangedCallback(evt =>
            {
                tabViewTabs.SetValueWithoutNotify(evt.newValue);
            });

            var stackView = root.Q<StackView>("stack-view");
            stackView.Push(new Text("1"));
            stackView.pushEnterAnimation = new AnimationDescription
            {
                durationMs = 500,
                easing = Easing.OutBack,
                callback = (element, f) =>
                {
                    var progress = 1f - f;
                    element.style.rotate = new StyleRotate(new Rotate(30f * progress));
                    element.style.left = progress * stackView.resolvedStyle.width;
                }
            };
            stackView.popExitAnimation = new AnimationDescription
            {
                durationMs = 500,
                easing = Easing.InBack,
                callback = (element, f) =>
                {
                    var progress = 1f - f;
                    element.style.rotate = new StyleRotate(new Rotate(-30f * progress));
                    var scale = f;
                    element.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1.0f)));
                    element.style.top = progress * stackView.resolvedStyle.height;
                }
            };
            root.Q<ActionButton>("sv-push-btn").clickable.clicked += () =>
            {
                var next = stackView.depth + 1;
                stackView.Push(new Text(next.ToString()));
            };
            root.Q<ActionButton>("sv-pop-btn").clickable.clicked += () =>
            {
                stackView.Pop();
            };
            root.Q<ActionButton>("sv-pop2-btn").clickable.clicked += () =>
            {
                if (stackView.depth >= 3)
                    stackView.Pop(stackView.ElementAt(stackView.depth - 3) as StackViewItem);
            };
            root.Q<ActionButton>("sv-pop-null-btn").clickable.clicked += () =>
            {
                stackView.Pop(null);
            };
            root.Q<ActionButton>("sv-clear-btn").clickable.clicked += () =>
            {
                stackView.ClearStack();
            };
        }

        static void OpenToast(NotificationStyle style, NotificationDuration duration, AnimationMode animationMode, VisualElement ve)
        {
            var toast = Toast.Build(ve, "A Toast Message", duration)
                .SetStyle(style)
                .SetAnimationMode(animationMode);

            if (style == NotificationStyle.Informative)
                toast.SetIcon("info");

            if (duration == NotificationDuration.Indefinite)
                toast.SetAction(DISMISS_ACTION, "Dismiss", () => Debug.Log("Dismissed"));

            toast.Show();
        }

        static void OpenMenu(VisualElement anchor)
        {
            MenuBuilder.Build(anchor)
                .AddAction(123, "An Item", "info", evt => Debug.Log("Item clicked"))
                .PushSubMenu(456, "My Sub Menu", "help")
                    .AddAction(789, "Sub Menu Item", "info", evt => Debug.Log("Sub Item clicked"))
                    .PushSubMenu(3455, "Another Sub Menu", "help")
                        .AddAction(7823129, "Another Sub Menu Item", "info", evt => Debug.Log("Other Item clicked"))
                    .Pop()
                .Pop()
                .Show();
        }

        static void OnBeingSwiped(SwipeViewItem element, float distance)
        {
            var child = element.ElementAt(0);

            if (child == null)
                return;

            var minOpacity = 0.33f;
            var maxOpacity = 1.0f;
            var newOpacity = Mathf.Lerp(minOpacity, maxOpacity, Mathf.Clamp01(1.0f - Mathf.Abs(distance)));
            child.style.opacity = newOpacity;

            var minScale = 0.8f;
            var maxScale = 1.0f;
            var newScale = Mathf.Lerp(minScale, maxScale, Mathf.Clamp01(1.0f - Mathf.Abs(distance)));
            child.style.scale = new Scale(new Vector3(newScale, newScale, 1.0f));
        }
    }

}
