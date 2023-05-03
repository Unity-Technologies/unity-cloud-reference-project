using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The position of the Tray.
    /// </summary>
    public enum TrayPosition
    {
        /// <summary>
        /// The Tray is displayed on the left side of the screen.
        /// </summary>
        Left,
        /// <summary>
        /// The Tray is displayed on the right side of the screen.
        /// </summary>
        Right,
        /// <summary>
        /// The Tray is displayed at the bottom of the screen.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// The Tray Popup class.
    /// </summary>
    public sealed class Tray : Popup<Tray>
    {
        const int k_TraySlideInDurationMs = 125;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="view">The Tray visual element itself.</param>
        Tray(VisualElement parentView, ApplicationContext context, TrayVisualElement view)
            : base(parentView, context, view)
        {
            keyboardDismissEnabled = true;
            view.RegisterCallback<ClickEvent>(OnTrayClicked);
        }

        TrayVisualElement tray => (TrayVisualElement)view;

        void OnTrayClicked(ClickEvent evt)
        {
            var insideTray = tray.trayElement.ContainsPoint(tray.trayElement.WorldToLocal(evt.position));
            if (!insideTray)
                Dismiss(DismissType.OutOfBounds);
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return tray.trayElement;
        }

        /// <summary>
        /// Dismiss the <see cref="Popup"/>.
        /// </summary>
        /// <param name="reason">Why the element has been dismissed.</param>
        protected override bool ShouldDismiss(DismissType reason)
        {
            return true;
        }

        /// <inheritdoc cref="Popup.ShouldAnimate"/>
        protected override bool ShouldAnimate()
        {
            return true;
        }

        /// <inheritdoc cref="Popup.AnimateViewIn"/>
        protected override void AnimateViewIn()
        {
            view.schedule.Execute(() =>
            {
                if (view.parent != null)
                {
                    tray.visible = true;
                    var fr = tray.position switch
                    {
                        TrayPosition.Left => -tray.trayElement.resolvedStyle.width,
                        TrayPosition.Right => -tray.trayElement.resolvedStyle.width,
                        TrayPosition.Bottom => -tray.trayElement.resolvedStyle.height,
                        _ => throw new ArgumentOutOfRangeException(nameof(tray.position), tray.position, "Unknown Tray position")
                    };
                    Action<VisualElement, float> interpolation = tray.position switch
                    {
                        TrayPosition.Left => (element, f) => element.style.marginLeft = f,
                        TrayPosition.Right => (element, f) => element.style.marginRight = f,
                        TrayPosition.Bottom => (element, f) => element.style.marginBottom = f,
                        _ => throw new ArgumentOutOfRangeException(nameof(tray.position), tray.position, "Unknown Tray position")
                    };
                    tray.experimental.animation
                        .Start(fr, 0, k_TraySlideInDurationMs, interpolation)
                        .Ease(Easing.OutQuad)
                        .OnCompleted(InvokeShownEventHandlers).Start();
                    tray.draggedOff += OnTrayDraggedOff;
                }
            }).ExecuteLater(k_NextFrameDurationMs);
        }

        void OnTrayDraggedOff()
        {
            Dismiss(DismissType.Manual);
        }

        /// <inheritdoc cref="Popup.AnimateViewOut"/>
        protected override void AnimateViewOut(DismissType reason)
        {
            switch (tray.position)
            {
                case TrayPosition.Left:
                    tray.trayElement.style.width = tray.trayElement.resolvedStyle.width;
                    tray.trayElement.style.right = new StyleLength(StyleKeyword.Auto);
                    break;
                case TrayPosition.Right:
                    tray.trayElement.style.width = tray.trayElement.resolvedStyle.width;
                    tray.trayElement.style.left = new StyleLength(StyleKeyword.Auto);
                    break;
                case TrayPosition.Bottom:
                    tray.trayElement.style.height = tray.trayElement.resolvedStyle.height;
                    tray.trayElement.style.top = new StyleLength(StyleKeyword.Auto);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var to = tray.position switch
            {
                TrayPosition.Left => -tray.trayElement.resolvedStyle.width,
                TrayPosition.Right => -tray.trayElement.resolvedStyle.width,
                TrayPosition.Bottom => -tray.trayElement.resolvedStyle.height,
                _ => throw new ArgumentOutOfRangeException(nameof(tray.position), tray.position, "Unknown Tray position")
            };
            Action<VisualElement, float> interpolation = tray.position switch
            {
                TrayPosition.Left => (element, f) => element.style.marginLeft = f,
                TrayPosition.Right => (element, f) => element.style.marginRight = f,
                TrayPosition.Bottom => (element, f) => element.style.marginBottom = f,
                _ => throw new ArgumentOutOfRangeException(nameof(tray.position), tray.position, "Unknown Tray position")
            };
            tray.experimental.animation
                .Start(0, to, k_TraySlideInDurationMs, interpolation)
                .OnCompleted(() =>
            {
                view.visible = false;
                InvokeDismissedEventHandlers(reason);
            }).Ease(Easing.OutQuad).Start();
        }

        /// <inheritdoc cref="Popup{T}.InvokeDismissedEventHandlers"/>
        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            base.InvokeDismissedEventHandlers(reason);
            tray.trayElement.UnregisterCallback<ClickEvent>(OnTrayClicked);
        }

        /// <summary>
        /// Build a new <see cref="Tray"/> component.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element inside the UI panel.</param>
        /// <param name="content">The content to display inside this <see cref="Tray"/>.</param>
        /// <returns>The <see cref="Tray"/> instance.</returns>
        public static Tray Build(VisualElement referenceView, VisualElement content)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.popupContainer;
            return new Tray(parentView, context, new TrayVisualElement(content))
                .SetLastFocusedElement(referenceView);
        }

        /// <summary>
        /// Build a new <see cref="Tray"/> component.
        /// </summary>
        /// <param name="position"> The position of the tray.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetPosition(TrayPosition position)
        {
            tray.position = position;
            return this;
        }

        /// <summary>
        /// Set the handle visibility.
        /// </summary>
        /// <param name="value"> The handle visibility.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetHandleVisible(bool value)
        {
            tray.showHandle = value;
            return this;
        }

        /// <summary>
        /// Set to true to make the tray expandable.
        /// </summary>
        /// <param name="expandable"> True to make the tray expandable.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetExpandable(bool expandable)
        {
            tray.expandable = expandable;
            return this;
        }

        /// <summary>
        /// Set the margin of the tray.
        /// </summary>
        /// <param name="margin"> The margin of the tray.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetMargin(float margin)
        {
            tray.margin = margin;
            return this;
        }

        /// <summary>
        /// Set the transition duration.
        /// </summary>
        /// <param name="durationMs"> The transition duration in milliseconds.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetTransitionDuration(int durationMs)
        {
            tray.transitionDurationMs = durationMs;
            return this;
        }

        /// <summary>
        /// Set the size of the tray.
        /// </summary>
        /// <param name="size"> The size of the tray.</param>
        /// <returns> The <see cref="Tray"/> instance.</returns>
        public Tray SetSize(float size)
        {
            tray.size = size;
            return this;
        }

        /// <summary>
        /// The Tray UI Element.
        /// </summary>
        class TrayVisualElement : VisualElement
        {
            public static readonly string ussClassName = "appui-tray";

            public static readonly string leftTrayUssClassName = ussClassName + "--left";

            public static readonly string rightTrayUssClassName = ussClassName + "--right";

            public static readonly string bottomTrayUssClassName = ussClassName + "--bottom";

            public static readonly string handleZoneUssClassName = ussClassName + "__handle-zone";

            public static readonly string handleUssClassName = ussClassName + "__handle";

            public static readonly string trayUssClassName = ussClassName + "__tray";

            public static readonly string containerUssClassName = ussClassName + "__container";

            public static readonly string contentUssClassName = ussClassName + "__content";

            /// <summary>
            /// Event triggered when the user has dragged almost completely the tray out of the screen.
            /// </summary>
            public event Action draggedOff;

            readonly VisualElement m_Container;

            TrayPosition m_Position;

            readonly Draggable m_Draggable;

            readonly VisualElement m_HandleZone;

            float m_StartHeight = float.NaN;

            bool m_OnHandleZone;

            float m_StartWidth = float.NaN;

            float m_Top;

            float m_StartTop;

            float m_Left;

            float m_StartLeft;

            float m_Right;

            float m_StartRight;

            readonly VisualElement m_Content;

            public bool showHandle
            {
                get => !m_HandleZone.ClassListContains(Styles.hiddenUssClassName);
                set => m_HandleZone.EnableInClassList(Styles.hiddenUssClassName, !value);
            }

            public bool expandable { get; set; }

            public float margin { get; set; }

            public float size
            {
                get => position switch
                {
                    TrayPosition.Left => m_Container.resolvedStyle.width,
                    TrayPosition.Right => m_Container.resolvedStyle.width,
                    TrayPosition.Bottom => m_Container.resolvedStyle.height,
                    _ => 0
                };

                set
                {
                    switch (position)
                    {
                        case TrayPosition.Left:
                        case TrayPosition.Right:
                            m_Container.style.width = value;
                            m_Container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                            break;
                        case TrayPosition.Bottom:
                            m_Container.style.height = value;
                            m_Container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public int transitionDurationMs { get; set; } = 150;

            public TrayVisualElement(VisualElement content)
            {
                AddToClassList(ussClassName);

                pickingMode = PickingMode.Position;
                focusable = false;

                trayElement = new VisualElement
                {
                    name = trayUssClassName,
                    usageHints = UsageHints.DynamicTransform,
                    focusable = true,
                    pickingMode = PickingMode.Position
                };
                trayElement.AddToClassList(trayUssClassName);
                m_HandleZone = new VisualElement { name = handleZoneUssClassName, focusable = true, pickingMode = PickingMode.Position };
                m_HandleZone.AddToClassList(handleZoneUssClassName);
                var handle = new VisualElement { name = handleUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
                handle.AddToClassList(handleUssClassName);
                m_Draggable = new Draggable(OnHandleClick, OnHandleDrag, OnHandleUp, OnHandleDown);
                trayElement.AddManipulator(m_Draggable);
                m_Container = new VisualElement { name = containerUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
                m_Container.AddToClassList(containerUssClassName);
                m_Content = new VisualElement { name = contentUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
                m_Content.AddToClassList(contentUssClassName);

                hierarchy.Add(trayElement);
                m_HandleZone.Add(handle);
                trayElement.hierarchy.Add(m_HandleZone);
                trayElement.hierarchy.Add(m_Container);
                m_Container.hierarchy.Add(m_Content);

                m_Content.Add(content);

                position = TrayPosition.Bottom;
                showHandle = true;
            }

            void OnHandleClick()
            {
                // nothing
            }

            void OnHandleDrag(Draggable draggable)
            {
                if (!m_OnHandleZone)
                    return;

                switch (m_Position)
                {
                    case TrayPosition.Left:
                        m_Right = expandable ? m_Right - draggable.deltaPos.x : Mathf.Max(m_Right - draggable.deltaPos.x, m_StartRight);
                        trayElement.style.right = m_Right;
                        break;
                    case TrayPosition.Right:
                        m_Left = expandable ? m_Left + draggable.deltaPos.x : Mathf.Max(m_Left + draggable.deltaPos.x, m_StartLeft);
                        trayElement.style.left = m_Left;
                        break;
                    case TrayPosition.Bottom:
                        m_Top = expandable ? m_Top + draggable.deltaPos.y : Mathf.Max(m_Top + draggable.deltaPos.y, m_StartTop);
                        trayElement.style.top = m_Top;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(m_Position), m_Position, "Unknown Tray position");
                }
            }

            void OnHandleUp(Draggable _)
            {
                if (!m_OnHandleZone)
                    return;

                var trayStyle = trayElement.resolvedStyle;

                switch (m_Position)
                {
                    case TrayPosition.Left:
                        {
                            var right = resolvedStyle.width - trayStyle.width;
                            var distToRight = expandable ? Mathf.Abs(right) : float.MaxValue;
                            var distToLeft = Mathf.Abs(trayStyle.width);
                            var distToStartWidth = Mathf.Abs(trayStyle.width - m_StartWidth);
                            if (distToRight < distToLeft && distToRight < distToStartWidth)
                            {
                                experimental.animation.Start(right, margin, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.right = f;
                                }).Ease(Easing.OutQuad);
                            }
                            else if (distToLeft < distToRight && distToLeft < distToStartWidth)
                            {
                                experimental.animation.Start(right, resolvedStyle.width, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.right = f;
                                }).Ease(Easing.OutQuad).OnCompleted(InvokeDraggedOff);
                            }
                            else // distToStartWidth
                            {
                                experimental.animation.Start(right, resolvedStyle.width - m_StartWidth, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.right = f;
                                }).Ease(Easing.OutQuad);
                            }
                        }
                        break;
                    case TrayPosition.Right:
                        {
                            var distToRight = Mathf.Abs(trayStyle.width);
                            var distToLeft = expandable ? Mathf.Abs(trayStyle.left) : float.MaxValue;
                            var distToStartWidth = Mathf.Abs(trayStyle.width - m_StartWidth);
                            if (distToLeft < distToRight && distToLeft < distToStartWidth)
                            {
                                experimental.animation.Start(trayStyle.left, margin, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.left = f;
                                }).Ease(Easing.OutQuad);
                            }
                            else if (distToRight < distToLeft && distToRight < distToStartWidth)
                            {
                                experimental.animation.Start(trayStyle.left, resolvedStyle.width, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.left = f;
                                }).Ease(Easing.OutQuad).OnCompleted(InvokeDraggedOff);
                            }
                            else // distToStartWidth
                            {
                                experimental.animation.Start(trayStyle.left, resolvedStyle.width - m_StartWidth, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.left = f;
                                }).Ease(Easing.OutQuad);
                            }
                        }
                        break;
                    case TrayPosition.Bottom:
                        {
                            var distToTop = expandable ? Mathf.Abs(trayStyle.top) : float.MaxValue;
                            var distToBottom = Mathf.Abs(trayStyle.height);
                            var distToStartHeight = Mathf.Abs(trayStyle.height - m_StartHeight);
                            if (distToTop < distToBottom && distToTop < distToStartHeight)
                            {
                                experimental.animation.Start(trayStyle.top, margin, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.top = f;
                                }).Ease(Easing.OutQuad);
                            }
                            else if (distToBottom < distToTop && distToBottom < distToStartHeight)
                            {
                                experimental.animation.Start(trayStyle.top, resolvedStyle.height, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.top = f;
                                }).Ease(Easing.OutQuad).OnCompleted(InvokeDraggedOff);
                            }
                            else // distToStartHeight
                            {
                                experimental.animation.Start(trayStyle.top, resolvedStyle.height - m_StartHeight, transitionDurationMs, (_, f) =>
                                {
                                    trayElement.style.top = f;
                                }).Ease(Easing.OutQuad);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(m_Position), m_Position, "Unknown Tray position");
                }

                m_OnHandleZone = false;
            }

            void InvokeDraggedOff()
            {
                draggedOff?.Invoke();
            }

            void OnHandleDown(Draggable _)
            {
                if (!showHandle)
                {
                    m_OnHandleZone = false;
                    return;
                }

                var trayStyle = trayElement.resolvedStyle;

                m_OnHandleZone = true;
                if (float.IsNaN(m_StartHeight))
                    m_StartHeight = trayStyle.height;
                if (float.IsNaN(m_StartWidth))
                    m_StartWidth = trayStyle.width;
                m_Top = trayStyle.top;
                m_StartTop = m_Top;
                m_Left = trayStyle.left;
                m_StartLeft = m_Left;
                m_Right = resolvedStyle.width - trayStyle.width;
                m_StartRight = m_Right;
            }

            public VisualElement trayElement { get; }

            public TrayPosition position
            {
                get => m_Position;
                set
                {
                    m_Position = value;
                    EnableInClassList(leftTrayUssClassName, m_Position == TrayPosition.Left);
                    EnableInClassList(rightTrayUssClassName, m_Position == TrayPosition.Right);
                    EnableInClassList(bottomTrayUssClassName, m_Position == TrayPosition.Bottom);

                    switch (m_Position)
                    {
                        case TrayPosition.Left:
                        case TrayPosition.Right:
                            m_Container.style.width = size;
                            m_Container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                            break;
                        case TrayPosition.Bottom:
                            m_Container.style.height = size;
                            m_Container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public override VisualElement contentContainer => m_Content;
        }
    }
}
