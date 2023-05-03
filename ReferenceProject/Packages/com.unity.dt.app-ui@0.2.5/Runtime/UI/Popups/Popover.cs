using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Possible placements for a Popover.
    /// </summary>
    public enum PopoverPlacement
    {
#pragma warning disable CS1591
        Bottom,

        BottomLeft,

        BottomRight,

        BottomStart,

        BottomEnd,

        Top,

        TopLeft,

        TopRight,

        TopStart,

        TopEnd,

        Left,

        LeftTop,

        LeftBottom,

        Start,

        StartTop,

        StartBottom,

        Right,

        RightTop,

        RightBottom,

        End,

        EndTop,

        EndBottom,
#pragma warning restore CS1591
    }

    /// <summary>
    /// The position result data structure returned in <see cref="AnchorPopup{T}.ComputePosition"/> utility method.
    /// </summary>
    public struct PositionResult
    {
        /// <summary>
        /// The Y Position from the top, in pixels.
        /// </summary>
        public float top { get; set; }

        /// <summary>
        /// The X Position from the left, in pixels.
        /// </summary>
        public float left { get; set; }

        /// <summary>
        /// The top margin, in pixels.
        /// </summary>
        public float marginTop { get; set; }

        /// <summary>
        /// The left margin, in pixels.
        /// </summary>
        public float marginLeft { get; set; }

        /// <summary>
        /// The computed placement, that may differ from the desired one.
        /// </summary>
        public PopoverPlacement finalPlacement { get; set; }

        /// <summary>
        /// The USS left value for the tip element.
        /// </summary>
        public float tipLeft { get; set; }

        /// <summary>
        /// The USS right value for the tip element.
        /// </summary>
        public float tipRight { get; set; }

        /// <summary>
        /// The USS top value for the tip element.
        /// </summary>
        public float tipTop { get; set; }

        /// <summary>
        /// The USS bottom value for the tip element.
        /// </summary>
        public float tipBottom { get; set; }
    }

    /// <summary>
    /// A popup usually anchored to another UI element.
    /// </summary>
    public sealed class Popover : AnchorPopup<Popover>
    {
        /// <summary>
        /// Enable or disable the blocking of outside click events.
        /// </summary>
        public bool modalBackdrop
        {
            get => popover.modalBackdrop;
            set => popover.modalBackdrop = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="popover">The popup visual element itself.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        Popover(VisualElement parentView, ApplicationContext context, PopoverVisualElement popover, VisualElement contentView)
            : base(parentView, context, popover, contentView)
        {
            parentView.panel.visualTree.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
        }

        PopoverVisualElement popover => (PopoverVisualElement)view;

        /// <summary>
        /// Build a new <see cref="Popover"/> instance.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element in the current panel.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        /// <returns>The <see cref="Popover"/> instance.</returns>
        public static Popover Build(VisualElement referenceView, VisualElement contentView)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.popupContainer;
            var popoverVisualElement = new PopoverVisualElement(contentView);
            var popoverElement = new Popover(parentView, context, popoverVisualElement, contentView)
                .SetAnchor(referenceView)
                .SetLastFocusedElement(referenceView);
            return popoverElement;
        }

        void OnTreeDown(PointerDownEvent evt)
        {
            if (!outsideClickDismissEnabled)
                return;

            var index = view.parent.IndexOf(view);
            if (index != view.parent.childCount - 1)
                return;

            var insidePopover = GetMovableElement().worldBound.Contains((Vector2)evt.position);
            if (!insidePopover)
            {
                var insideAnchor = anchor?.worldBound.Contains((Vector2)evt.position) ?? false;
                var insideLastFocusedElement = (m_LastFocusedElement as VisualElement)?.worldBound.Contains((Vector2)evt.position) ?? false;
                if (insideAnchor || insideLastFocusedElement)
                {
                    // prevent reopening the same popover again...
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                }
                Dismiss(DismissType.OutOfBounds);
            }
        }

        /// <inheritdoc cref="Popup.ShouldAnimate"/>
        protected override bool ShouldAnimate()
        {
            return true;
        }

        /// <summary>
        /// Enable or disable the blocking of outside click events.
        /// </summary>
        /// <param name="enableModalBackdrop"> Whether to enable the blocking of outside click events.</param>
        /// <returns> The <see cref="Popover"/> instance.</returns>
        public Popover SetModalBackdrop(bool enableModalBackdrop)
        {
            modalBackdrop = enableModalBackdrop;
            return this;
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc cref="AnchorPopup{T}.GetMovableElement"/>
        protected override VisualElement GetMovableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc cref="Popup{T}.InvokeDismissedEventHandlers"/>
        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            base.InvokeDismissedEventHandlers(reason);
            targetParent.panel.visualTree.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// The UI element used as a Popover.
        /// </summary>
        internal class PopoverVisualElement : VisualElement, IPlaceableElement
        {
            public static readonly string ussClassName = "appui-popover";

            public static readonly string modalBackdropUssClassName = ussClassName + "--modal-backdrop";

            public static readonly string popoverUssClassName = ussClassName + "__popover";

            public static readonly string containerUssClassName = ussClassName + "__container";

            public static readonly string shadowElementUssClassName = ussClassName + "__shadow-element";

            public static readonly string tipUssClassName = ussClassName + "__tip";

            public static readonly string upUssClassName = ussClassName + "--up";

            public static readonly string downUssClassName = ussClassName + "--down";

            public static readonly string leftUssClassName = ussClassName + "--left";

            public static readonly string rightUssClassName = ussClassName + "--right";

            readonly VisualElement m_ContentContainer;

            PopoverPlacement m_Placement = PopoverPlacement.Top;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="content">The content of the popup.</param>
            public PopoverVisualElement(VisualElement content)
            {
                AddToClassList(ussClassName);

                modalBackdrop = false;

                popoverElement = new VisualElement
                {
                    name = popoverUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = true,
                    usageHints = UsageHints.DynamicTransform,
                };
                popoverElement.AddToClassList(popoverUssClassName);
                hierarchy.Add(popoverElement);

                var shadowElement = new ExVisualElement
                {
                    name = shadowElementUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = false,
                    passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows
                };
                shadowElement.AddToClassList(shadowElementUssClassName);
                popoverElement.hierarchy.Add(shadowElement);

                tipElement = new VisualElement { name = tipUssClassName, pickingMode = PickingMode.Ignore, focusable = false };
                tipElement.AddToClassList(tipUssClassName);
                popoverElement.hierarchy.Add(tipElement);

                m_ContentContainer = new VisualElement
                {
                    name = containerUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = false,
                };
                m_ContentContainer.AddToClassList(containerUssClassName);
                popoverElement.hierarchy.Add(m_ContentContainer);

                m_ContentContainer.Add(content);

                RefreshPlacement();
            }

            /// <summary>
            /// The popover UI element.
            /// <remarks>This is the real popover element that needs to be anchored. Its parent is usually a smir.</remarks>
            /// </summary>
            public VisualElement popoverElement { get; }

            public VisualElement tipElement { get; }

            public override VisualElement contentContainer => m_ContentContainer;

            /// <summary>
            /// The popup placement, used to display the arrow at the right place.
            /// </summary>
            public PopoverPlacement placement
            {
                get => m_Placement;
                set
                {
                    m_Placement = value;
                    RefreshPlacement();
                }
            }

            public bool modalBackdrop
            {
                get => ClassListContains(modalBackdropUssClassName);
                set
                {
                    EnableInClassList(modalBackdropUssClassName, value);
                    pickingMode = value ? PickingMode.Position : PickingMode.Ignore;
                }
            }

            void RefreshPlacement()
            {
                bool up = false, down = false, left = false, right = false;

                switch (m_Placement)
                {
                    case PopoverPlacement.Bottom:
                    case PopoverPlacement.BottomLeft:
                    case PopoverPlacement.BottomRight:
                    case PopoverPlacement.BottomStart:
                    case PopoverPlacement.BottomEnd:
                        up = true;
                        break;
                    case PopoverPlacement.Top:
                    case PopoverPlacement.TopLeft:
                    case PopoverPlacement.TopRight:
                    case PopoverPlacement.TopStart:
                    case PopoverPlacement.TopEnd:
                        down = true;
                        break;
                    case PopoverPlacement.Left:
                    case PopoverPlacement.LeftTop:
                    case PopoverPlacement.LeftBottom:
                    case PopoverPlacement.Start:
                    case PopoverPlacement.StartTop:
                    case PopoverPlacement.StartBottom:
                        right = true;
                        break;
                    case PopoverPlacement.Right:
                    case PopoverPlacement.RightTop:
                    case PopoverPlacement.RightBottom:
                    case PopoverPlacement.End:
                    case PopoverPlacement.EndTop:
                    case PopoverPlacement.EndBottom:
                        left = true;
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(m_Placement), m_Placement);
                }

                EnableInClassList(upUssClassName, up);
                EnableInClassList(downUssClassName, down);
                EnableInClassList(leftUssClassName, left);
                EnableInClassList(rightUssClassName, right);
            }
        }
    }
}
