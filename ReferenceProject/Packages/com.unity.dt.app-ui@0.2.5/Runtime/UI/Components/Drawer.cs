using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Drawer UI element. A drawer is a UI element that slides in from the side of the screen. It can be used to display
    /// additional content or to display a menu.
    /// </summary>
    public class Drawer : VisualElement
    {
        /// <summary>
        /// The Drawer main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-drawer";

        /// <summary>
        /// The Drawer backdrop styling class.
        /// </summary>
        public static readonly string backdropUssClassName = ussClassName + "__backdrop";

        /// <summary>
        /// The Drawer element styling class.
        /// </summary>
        public static readonly string drawerUssClassName = ussClassName + "__drawer";

        /// <summary>
        /// The Drawer container styling class.
        /// </summary>
        public static readonly string drawerContainerUssClassName = ussClassName + "__drawer-container";

        /// <summary>
        /// The Drawer variant styling class.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The elevation styling class prefix.
        /// </summary>
        public static readonly string elevationUssClassName = "appui-elevation-";

        readonly VisualElement m_Backdrop;

        readonly VisualElement m_DrawerElement;

        readonly ExVisualElement m_DrawerContainer;

        DrawerAnchor m_Anchor;

        DrawerVariant m_Variant;

        readonly Draggable m_SwipeManipulator;

        bool m_InSwipeAreaToOpen;

        Vector2 m_SwipeToOpenVector;

        bool m_Swipeable;

        float m_SwipeAreaWidth;

        bool m_IsOpen;

        float m_Elevation;

        /// <summary>
        /// Event fired when the drawer is closed.
        /// </summary>
        public event Action<Drawer> closed;

        /// <summary>
        /// Event fired when the drawer is opened.
        /// </summary>
        public event Action<Drawer> opened;

        /// <summary>
        /// The opacity of the backdrop when the drawer is open.
        /// </summary>
        public float backdropFinalOpacity { get; set; }

        /// <summary>
        /// Ability to swipe the drawer to open it or close it.
        /// </summary>
        public bool swipeable
        {
            get
            {
                return m_Swipeable && variant == DrawerVariant.Temporary;
            }

            set
            {
                if (m_Swipeable == value)
                    return;

                m_Swipeable = value;
                m_SwipeManipulator?.Cancel();
            }
        }

        /// <summary>
        /// Check if the drawer is open.
        /// </summary>
        public bool isOpen
        {
            get => m_IsOpen || variant == DrawerVariant.Permanent;
            private set
            {
                EnableInClassList(Styles.openUssClassName, value);
                m_IsOpen = value;
            }
        }

        /// <summary>
        /// The duration of the transition when opening or closing the drawer in milliseconds.
        /// </summary>
        public int transitionDurationMs { get; set; }

        /// <summary>
        /// Enable or disable the transition animation for the backdrop when opening or closing the drawer.
        /// </summary>
        public bool backdropTransitionEnabled { get; set; }

        /// <summary>
        /// Show or hide the backdrop of this drawer.
        /// </summary>
        public bool hideBackdrop
        {
            get => m_Backdrop.ClassListContains(Styles.hiddenUssClassName);
            set => m_Backdrop.EnableInClassList(Styles.hiddenUssClassName, value);
        }

        /// <summary>
        /// The content container of the drawer.
        /// </summary>
        public override VisualElement contentContainer => m_DrawerContainer;

        /// <summary>
        /// The normalized distance of the drawer from the edge of the screen. 0 means the drawer is closed, 1 means the
        /// drawer is fully open.
        /// </summary>
        public float distance
        {
            get
            {
                var size = m_DrawerElement.localBound.width;
                return anchor switch
                {
                    DrawerAnchor.Left => (size + m_DrawerElement.resolvedStyle.left) / size,
                    _ => (size + m_DrawerElement.resolvedStyle.right) / size,
                };
            }
        }

        /// <summary>
        /// The size of the swipe area to open the drawer.
        /// </summary>
        public float swipeAreaWidth
        {
            get => m_SwipeAreaWidth;

            set
            {
                m_SwipeAreaWidth = value;
                if (m_Variant == DrawerVariant.Temporary)
                    style.width = m_SwipeAreaWidth;
            }
        }

        /// <summary>
        /// The distance threshold to interact with the drawer when swiping.
        /// </summary>
        public float hysteresis { get; set; }

        /// <summary>
        /// The elevation level of the drawer.
        /// </summary>
        public float elevation
        {
            get => m_Elevation;
            set
            {
                m_DrawerContainer.RemoveFromClassList(elevationUssClassName + m_Elevation.ToString().ToLower());
                m_Elevation = value;
                m_DrawerContainer.passMask = m_Elevation > 0
                    ? ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows
                    : 0;
                m_DrawerContainer.AddToClassList(elevationUssClassName + m_Elevation.ToString().ToLower());
                if (anchor == DrawerAnchor.Left)
                {
                    m_DrawerElement.style.paddingRight = m_Elevation;
                    m_DrawerElement.style.paddingLeft = 0;
                }
                else
                {
                    m_DrawerElement.style.paddingLeft = m_Elevation;
                    m_DrawerElement.style.paddingRight = 0;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DrawerAnchor anchor
        {
            get => m_Anchor;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Anchor.ToString().ToLower());
                m_Anchor = value;

                if (m_Anchor == DrawerAnchor.Left)
                {
                    style.left = 0;
                    style.top = 0;
                    style.bottom = 0;
                    style.right = new StyleLength(StyleKeyword.Auto);
                    m_DrawerElement.style.left = -640;
                    m_DrawerElement.style.right = new StyleLength(StyleKeyword.Auto);
                    m_DrawerElement.style.paddingRight = m_Elevation;
                    m_DrawerElement.style.paddingLeft = 0;
                }
                else
                {
                    style.left = new StyleLength(StyleKeyword.Auto);
                    style.top = 0;
                    style.bottom = 0;
                    style.right = 0;
                    m_DrawerElement.style.right = -640;
                    m_DrawerElement.style.left = new StyleLength(StyleKeyword.Auto);
                    m_DrawerElement.style.paddingLeft = m_Elevation;
                    m_DrawerElement.style.paddingRight = 0;
                }
                AddToClassList(variantUssClassName + m_Anchor.ToString().ToLower());
            }
        }

        /// <summary>
        /// The variant of the drawer. Permanent drawers are always open and cannot be closed. Temporary drawers can be
        /// opened and closed.
        /// </summary>
        public DrawerVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantUssClassName + m_Variant.ToString().ToLower());
                if (m_Variant == DrawerVariant.Permanent)
                {
                    style.width = new StyleLength(StyleKeyword.Auto);
                    if (anchor == DrawerAnchor.Left)
                    {
                        m_DrawerElement.style.left = 0;
                        m_DrawerElement.style.right = new StyleLength(StyleKeyword.Auto);
                    }
                    else
                    {
                        m_DrawerElement.style.right = 0;
                        m_DrawerElement.style.left = new StyleLength(StyleKeyword.Auto);
                    }
                }
                else
                {
                    if (isOpen)
                    {
                        Close();
                    }
                    else
                    {
                        style.width = swipeAreaWidth;
                        if (anchor == DrawerAnchor.Left)
                        {
                            m_DrawerElement.style.left = -640;
                            m_DrawerElement.style.right = new StyleLength(StyleKeyword.Auto);
                        }
                        else
                        {
                            m_DrawerElement.style.right = -640;
                            m_DrawerElement.style.left = new StyleLength(StyleKeyword.Auto);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Drawer()
        {
            AddToClassList(ussClassName);

            m_Backdrop = new VisualElement
            {
                name = backdropUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicColor,
            };
            m_DrawerElement = new VisualElement
            {
                name = drawerUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_DrawerContainer = new ExVisualElement
            {
                name = drawerContainerUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = 0
            };

            m_Backdrop.AddToClassList(backdropUssClassName);
            m_DrawerElement.AddToClassList(drawerUssClassName);
            m_DrawerContainer.AddToClassList(drawerContainerUssClassName);

            hierarchy.Add(m_Backdrop);
            hierarchy.Add(m_DrawerElement);
            m_DrawerElement.hierarchy.Add(m_DrawerContainer);

            m_SwipeManipulator = new Draggable(OnClick, OnDrag, OnUp, OnDown);
            this.AddManipulator(m_SwipeManipulator);

            anchor = DrawerAnchor.Left;
            variant = DrawerVariant.Temporary;
            swipeable = true;
            backdropTransitionEnabled = true;
            swipeAreaWidth = 16;
            elevation = 16;
            backdropFinalOpacity = 0.33f;
            isOpen = false;
            transitionDurationMs = 150;
            hysteresis = 16;
        }

        /// <summary>
        /// Open the drawer.
        /// </summary>
        public void Open()
        {
            if (isOpen)
                return;

            if (anchor == DrawerAnchor.Left)
                m_DrawerElement.style.left = -m_DrawerElement.localBound.width;
            else
                m_DrawerElement.style.right = -m_DrawerElement.localBound.width;

            isOpen = true;
            m_Backdrop.pickingMode = PickingMode.Position;
            style.width = parent.localBound.width;
            if (m_SwipeToOpenVector.sqrMagnitude > 0)
            {
                // Opened without animation, nothing to do
            }
            else
            {
                FinishOpenAnimation();
            }
            opened?.Invoke(this);
        }

        void FinishOpenAnimation()
        {
            // Animation
            m_DrawerElement.experimental.animation.Start(distance, 1, transitionDurationMs, (element, f) =>
            {
                if (anchor == DrawerAnchor.Left)
                    element.style.left = (1 - f) * -m_DrawerElement.localBound.width;
                else
                    element.style.right = (1 - f) * m_DrawerElement.localBound.width;
            }).Ease(Easing.OutQuad);

            if (backdropTransitionEnabled && !hideBackdrop)
            {
                m_Backdrop.experimental.animation.Start(m_Backdrop.resolvedStyle.opacity, backdropFinalOpacity, transitionDurationMs, (element, f) =>
                {
                    element.style.opacity = f;
                }).Ease(Easing.OutQuad);
            }
            else if (!hideBackdrop)
            {
                m_Backdrop.style.opacity = backdropFinalOpacity;
            }
        }

        /// <summary>
        /// Close the drawer.
        /// </summary>
        public void Close()
        {
            var size = m_DrawerElement.localBound.width;
            var d = anchor == DrawerAnchor.Left ? distance : (size - m_DrawerElement.resolvedStyle.right) / size;
            m_DrawerElement.experimental.animation.Start(d, 0, transitionDurationMs,
                (element, f) =>
                {
                    if (anchor == DrawerAnchor.Left)
                        element.style.left = (1 - f) * -m_DrawerElement.localBound.width;
                    else
                        element.style.right = (1 - f) * -m_DrawerElement.localBound.width;
                }).Ease(Easing.OutQuad).OnCompleted(RequestDismiss);

            if (backdropTransitionEnabled && !hideBackdrop)
            {
                m_Backdrop.experimental.animation.Start(m_Backdrop.resolvedStyle.opacity, 0, transitionDurationMs, (element, f) =>
                {
                    element.style.opacity = f;
                }).Ease(Easing.OutQuad);
            }
            else if (!hideBackdrop)
            {
                m_Backdrop.style.opacity = 0;
            }
        }

        /// <summary>
        /// Toggle the drawer. If it is open, close it. If it is closed, open it.
        /// </summary>
        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        bool InSwipeArea(Vector2 pos)
        {
            if (anchor == DrawerAnchor.Left)
                return pos.x < swipeAreaWidth;
            else
                return pos.x > contentRect.width - swipeAreaWidth;
        }

        void OnDown(Draggable manipulator)
        {
            m_InSwipeAreaToOpen = false;
            m_SwipeToOpenVector = Vector2.zero;

            if (!isOpen && swipeable && variant == DrawerVariant.Temporary)
            {
                m_InSwipeAreaToOpen = InSwipeArea(manipulator.localPosition);
            }
        }

        void OnUp(Draggable manipulator)
        {
            var closing = m_SwipeToOpenVector.sqrMagnitude == 0 && !m_InSwipeAreaToOpen && isOpen;
            m_InSwipeAreaToOpen = false;
            m_SwipeToOpenVector = Vector2.zero;

            if (!manipulator.hasMoved || variant == DrawerVariant.Permanent)
                return;

            if (closing)
            {
                Close();
            }
            else if (isOpen)
            {
                FinishOpenAnimation();
            }
        }

        void OnDrag(Draggable manipulator)
        {
            if (!swipeable)
                return;

            if (isOpen)
            {
                // move the drawer
                float d;
                var size = m_DrawerElement.localBound.width;
                if (anchor == DrawerAnchor.Left)
                {
                    var newLeftValue = Mathf.Min(0, m_DrawerElement.resolvedStyle.left + manipulator.deltaPos.x);
                    m_DrawerElement.style.left = newLeftValue;
                    d = (size + newLeftValue) / size;
                }
                else
                {
                    var newRightValue = Mathf.Min(0, -m_DrawerElement.resolvedStyle.right - manipulator.deltaPos.x);
                    m_DrawerElement.style.right = newRightValue;
                    d = (size + newRightValue) / size;
                }

                if (backdropTransitionEnabled && !hideBackdrop)
                    m_Backdrop.style.opacity = Mathf.Lerp(0, backdropFinalOpacity, d);
            }
            else
            {
                // check if the pointer is in the swipe area to open the drawer
                if (m_InSwipeAreaToOpen)
                {
                    m_SwipeToOpenVector += manipulator.deltaPos;
                    if ((anchor == DrawerAnchor.Left && m_SwipeToOpenVector.x > hysteresis)
                        || (anchor == DrawerAnchor.Right && m_SwipeToOpenVector.x < -hysteresis))
                        Open();
                }
            }
        }

        void OnClick()
        {
            if (m_SwipeManipulator.hasMoved)
                return;

            // Close the Drawer if it is open
            var mousePos = this.LocalToWorld(m_SwipeManipulator.localPosition);
            var hoverDrawer = m_DrawerElement.ContainsPoint(m_DrawerElement.WorldToLocal(mousePos));
            if (isOpen && !hoverDrawer)
                Close();
        }

        void RequestDismiss()
        {
            isOpen = false;
            m_Backdrop.pickingMode = PickingMode.Ignore;
            style.width = swipeAreaWidth;
            closed?.Invoke(this);

            //todo
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Drawer"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Drawer, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Drawer"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlEnumAttributeDescription<DrawerAnchor> m_Anchor =
                new UxmlEnumAttributeDescription<DrawerAnchor>()
                {
                    name = "anchor",
                    defaultValue = DrawerAnchor.Left,
                };

            readonly UxmlEnumAttributeDescription<DrawerVariant> m_Variant =
                new UxmlEnumAttributeDescription<DrawerVariant>()
                {
                    name = "variant",
                    defaultValue = DrawerVariant.Temporary,
                };

            readonly UxmlBoolAttributeDescription m_Swipeable =
                new UxmlBoolAttributeDescription()
                {
                    name = "swipeable",
                    defaultValue = true,
                };

            readonly UxmlBoolAttributeDescription m_HideBackdrop =
                new UxmlBoolAttributeDescription()
                {
                    name = "hide-backdrop",
                    defaultValue = false,
                };

            readonly UxmlBoolAttributeDescription m_BackdropTransitionEnabled =
                new UxmlBoolAttributeDescription()
                {
                    name = "backdrop-transition",
                    defaultValue = true,
                };

            readonly UxmlFloatAttributeDescription m_BackdropFinalOpacity =
                new UxmlFloatAttributeDescription()
                {
                    name = "backdrop-opacity",
                    defaultValue = 0.33f,
                };

            readonly UxmlFloatAttributeDescription m_SwipeAreaWidth =
                new UxmlFloatAttributeDescription()
                {
                    name = "swipe-area-width",
                    defaultValue = 16f,
                };

            readonly UxmlIntAttributeDescription m_TransitionDuration =
                new UxmlIntAttributeDescription()
                {
                    name = "transition-duration",
                    defaultValue = 150,
                };

            readonly UxmlFloatAttributeDescription m_Hysteresis =
                new UxmlFloatAttributeDescription()
                {
                    name = "hysteresis",
                    defaultValue = 16f,
                };

            readonly UxmlIntAttributeDescription m_Elevation =
                new UxmlIntAttributeDescription()
                {
                    name = "elevation",
                    defaultValue = 16,
                };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (Drawer)ve;
                el.anchor = m_Anchor.GetValueFromBag(bag, cc);
                el.variant = m_Variant.GetValueFromBag(bag, cc);
                el.swipeable = m_Swipeable.GetValueFromBag(bag, cc);
                el.hideBackdrop = m_HideBackdrop.GetValueFromBag(bag, cc);
                el.backdropTransitionEnabled = m_BackdropTransitionEnabled.GetValueFromBag(bag, cc);
                el.backdropFinalOpacity = m_BackdropFinalOpacity.GetValueFromBag(bag, cc);
                el.swipeAreaWidth = m_SwipeAreaWidth.GetValueFromBag(bag, cc);
                el.transitionDurationMs = m_TransitionDuration.GetValueFromBag(bag, cc);
                el.hysteresis = m_Hysteresis.GetValueFromBag(bag, cc);
                el.elevation = m_Elevation.GetValueFromBag(bag, cc);
            }
        }
    }

    /// <summary>
    /// The variant of the Drawer.
    /// </summary>
    public enum DrawerVariant
    {
        /// <summary>
        /// The Drawer is temporary and will be dismissed when the user clicks outside of it.
        /// </summary>
        Temporary,
        /// <summary>
        /// The Drawer is permanent and will not be dismissed when the user clicks outside of it.
        /// </summary>
        Permanent
    }

    /// <summary>
    /// The anchor of the Drawer. The Drawer will be anchored to the left or right side of the screen.
    /// </summary>
    public enum DrawerAnchor
    {
        /// <summary>
        /// The Drawer will be anchored to the left side of the screen.
        /// </summary>
        Left,
        /// <summary>
        /// The Drawer will be anchored to the right side of the screen.
        /// </summary>
        Right,
    }
}
