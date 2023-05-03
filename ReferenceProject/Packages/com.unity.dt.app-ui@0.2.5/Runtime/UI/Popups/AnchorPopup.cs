using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Interface that should be implemented in UI elements that can be anchored to another UI element.
    /// </summary>
    public interface IPlaceableElement
    {
        /// <summary>
        /// The popover placement.
        /// </summary>
        PopoverPlacement placement { get; set; }

        /// <summary>
        /// The popover tip/arrow element.
        /// </summary>
        VisualElement tipElement { get; }
    }

    /// <summary>
    /// Base class for Popup that can be anchored to another UI Element.
    /// </summary>
    /// <typeparam name="T">The sealed anchor popup class type.</typeparam>
    public abstract class AnchorPopup<T> : Popup<T> where T : AnchorPopup<T>
    {
        const long k_AnchorUpdateInterval = 8L;

        VisualElement m_Anchor;

        Rect m_AnchorBounds;

        IVisualElementScheduledItem m_AnchorUpdate;

        int m_CrossOffset;

        PopoverPlacement m_CurrentPlacement;

        int m_Offset;

        PopoverPlacement m_Placement = PopoverPlacement.Bottom;

        bool m_ShouldFlip = true;

        Rect m_ContentBounds;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="view">The popup visual element itself.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        protected AnchorPopup(VisualElement parentView, ApplicationContext context, VisualElement view,
            VisualElement contentView = null)
            : base(parentView, context, view, contentView)
        { }

        /// <summary>
        /// The desired placement.
        /// <remarks>
        /// You can set the desired placement using <see cref="SetPlacement"/>.
        /// </remarks>
        /// </summary>
        public PopoverPlacement placement => m_Placement;

        /// <summary>
        /// The current placement.
        /// <remarks>
        /// The current placement can be different from the placement set with <see cref="SetPlacement"/>, based
        /// on the current position of the anchor on the screen and the ability to flip placement.
        /// </remarks>
        /// </summary>
        public PopoverPlacement currentPlacement => m_CurrentPlacement;

        /// <summary>
        /// The offset in pixels, in the direction of the primary placement vector.
        /// </summary>
        public int offset => m_Offset;

        /// <summary>
        /// The offset in pixels, in the direction of the secondary placement vector.
        /// </summary>
        public int crossOffset => m_CrossOffset;

        /// <summary>
        /// The padding in pixels, inside the popup panel's container.
        /// </summary>
        public int containerPadding { get; private set; }

        /// <summary>
        /// `True` if the popup will be displayed at the opposite position if there's not enough
        /// place using the preferred <see cref="placement"/>, `False` otherwise.
        /// </summary>
        public bool shouldFlip => m_ShouldFlip;

        /// <summary>
        /// `True` if the small arrow used next to the anchor should be visible, `False` otherwise.
        /// </summary>
        public bool arrowVisible { get; private set; } = true;

        /// <summary>
        /// `True` if the the popup can be dismissed by clicking outside of it, `False` otherwise.
        /// </summary>
        public bool outsideClickDismissEnabled { get; protected set; } = true;

        /// <summary>
        /// The popup's anchor.
        /// </summary>
        public VisualElement anchor => m_Anchor;

        /// <summary>
        /// Set the preferred <see cref="placement"/> value.
        /// <remarks>This will trigger a refresh of the current popup position automatically.</remarks>
        /// </summary>
        /// <param name="popoverPlacement">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetPlacement(PopoverPlacement popoverPlacement)
        {
            m_Placement = popoverPlacement;
            RefreshPosition();
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="offset"/> property.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetOffset(int value)
        {
            m_Offset = value;
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="crossOffset"/> property.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetCrossOffset(int value)
        {
            m_CrossOffset = value;
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="containerPadding"/> property.
        /// <remarks>This will trigger a refresh of the current popup position automatically.</remarks>
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetContainerPadding(int value)
        {
            containerPadding = value;
            view.contentContainer.style.paddingBottom = containerPadding;
            view.contentContainer.style.paddingLeft = containerPadding;
            view.contentContainer.style.paddingRight = containerPadding;
            view.contentContainer.style.paddingTop = containerPadding;
            RefreshPosition();
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="shouldFlip"/> property.
        /// <remarks>This will trigger a refresh of the current popup position automatically.</remarks>
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetShouldFlip(bool value)
        {
            m_ShouldFlip = value;
            RefreshPosition();
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="arrowVisible"/> property.
        /// <remarks>This will trigger a refresh of the current popup position automatically.</remarks>
        /// </summary>
        /// <param name="visible">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetArrowVisible(bool visible)
        {
            arrowVisible = visible;
            view.EnableInClassList(Styles.noArrowUssClassName, !arrowVisible);
            RefreshPosition();
            return (T)this;
        }

        /// <summary>
        /// Set a new value for the <see cref="anchor"/> property.
        /// <remarks>This will trigger a refresh of the current popup position automatically.</remarks>
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The popup.</returns>
        public T SetAnchor(VisualElement value)
        {
            m_AnchorUpdate?.Pause();
            m_AnchorUpdate = null;
            m_Anchor = value;
            m_AnchorUpdate = m_Anchor?.schedule.Execute(AnchorUpdate).Every(k_AnchorUpdateInterval);
            RefreshPosition();
            return (T)this;
        }
        
        /// <summary>
        /// Activate the possibility to dismiss the popup by clicking outside of it.
        /// </summary>
        /// <param name="dismissEnabled"> `True` to activate the feature, `False` otherwise.</param>
        /// <returns> The popup of type <typeparamref name="T"/>.</returns>
        public T SetOutsideClickDismiss(bool dismissEnabled)
        {
            outsideClickDismissEnabled = dismissEnabled;
            return (T)this;
        }

        /// <summary>
        /// Called when the popup's <see cref="Handler"/> has received a <see cref="Popup.k_PopupShow"/> message.
        /// <remarks>
        /// In this method the view should become visible at some point (directly or via an animation).
        /// </remarks>
        /// </summary>
        protected override void ShowView()
        {
            base.ShowView();
            context.panel.RegisterPopup(this);
        }

        /// <summary>
        /// Start the animation for this popup.
        /// </summary>
        protected override void AnimateViewIn()
        {
            view.schedule.Execute(() =>
            {
                if (view.parent != null)
                {
                    view.visible = true;
                    RefreshPosition();
                    InvokeShownEventHandlers();
                }
            }).ExecuteLater(k_NextFrameDurationMs);
        }

        /// <summary>
        /// Returns `True` if the popup should be dismissed, `False` otherwise.
        /// </summary>
        /// <param name="reason"> The reason for the dismissal.</param>
        /// <returns> `True` if the popup should be dismissed, `False` otherwise.</returns>
        protected override bool ShouldDismiss(DismissType reason)
        {
            if (reason == DismissType.OutOfBounds && !outsideClickDismissEnabled)
                return false;
            return true;
        }

        /// <summary>
        /// Called when the popup's <see cref="Handler"/> has received a <see cref="Popup.k_PopupDismiss"/> message.
        /// </summary>
        /// <param name="reason">The reason why the popup should be dismissed.</param>
        protected override void HideView(DismissType reason)
        {
            base.HideView(reason);
            context.panel.UnregisterPopup(this);
        }

        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            base.InvokeDismissedEventHandlers(reason);
            m_AnchorUpdate?.Pause();
            m_AnchorUpdate = null;
        }

        /// <summary>
        /// Start the hide animation for this popup.
        /// </summary>
        /// <param name="reason"></param>
        protected override void AnimateViewOut(DismissType reason)
        {
            view.visible = false;
            InvokeDismissedEventHandlers(reason);
        }

        void AnchorUpdate(TimerState timerState)
        {
            if (m_Anchor == null || contentView == null)
            {
                m_AnchorUpdate?.Pause();
                m_AnchorUpdate = null;
                return;
            }

            if (m_AnchorBounds != m_Anchor.worldBound)
            {
                m_AnchorBounds = m_Anchor.worldBound;
                RefreshPosition();
            }
            else if (contentView.worldBound != m_ContentBounds)
            {
                m_ContentBounds = contentView.worldBound;
                RefreshPosition();
            }
        }

        /// <summary>
        /// Recompute the position of the popup based on the anchor's position and size, but also others properties such
        /// as the <see cref="offset"/>, <see cref="crossOffset"/>, <see cref="shouldFlip"/> and <see cref="placement"/>.
        /// </summary>
        protected void RefreshPosition()
        {
            if (m_Anchor == null || !view.visible)
                return;

            var movableElement = GetMovableElement();
            var result = ComputePosition(movableElement, m_Anchor, context.panel, placement, offset, crossOffset, shouldFlip);
            movableElement.style.left = result.left;
            movableElement.style.top = result.top;
            movableElement.style.marginLeft = result.marginLeft;
            movableElement.style.marginTop = result.marginTop;
            if (view is IPlaceableElement placeableElement)
            {
                placeableElement.placement = result.finalPlacement;
                if (placeableElement.tipElement is { } tip)
                {
                    tip.style.bottom = result.tipBottom < 0 ? StyleKeyword.Auto : result.tipBottom;
                    tip.style.top = result.tipTop < 0 ? StyleKeyword.Auto : result.tipTop;
                    tip.style.left = result.tipLeft < 0 ? StyleKeyword.Auto : result.tipLeft;
                    tip.style.right = result.tipRight < 0 ? StyleKeyword.Auto : result.tipRight;
                }
            }
            m_CurrentPlacement = result.finalPlacement;
        }

        /// <summary>
        /// Method which must return the visual element that needs to be moved, based on the anchor position and size.
        /// </summary>
        /// <returns>The visual element which will be moved. The default value is <see cref="Popup.view"/>.</returns>
        protected virtual VisualElement GetMovableElement()
        {
            return view;
        }

        static void CrossSnapHorizontally(ref PositionResult result, Rect screenRect, Rect elementRect)
        {
            if (elementRect.width < screenRect.width)
            {
                if (result.left + result.marginLeft < screenRect.xMin)
                {
                    result.left = -result.marginLeft;
                }
                else if (result.left + result.marginLeft + elementRect.width > screenRect.width)
                {
                    var tmpLeft = screenRect.width - elementRect.width - result.marginLeft;
                    if (tmpLeft >= screenRect.xMin)
                        result.left = tmpLeft;
                }
            }
        }

        static void CrossSnapVertically(ref PositionResult result, Rect screenRect, Rect elementRect)
        {
            if (elementRect.height < screenRect.height)
            {
                if (result.top + result.marginTop < screenRect.yMin)
                {
                    result.top = -result.marginTop;
                }
                else if (result.top + result.marginTop + elementRect.height > screenRect.height)
                {
                    var tmpTop = screenRect.height - elementRect.height - result.marginTop;
                    if (tmpTop >= screenRect.yMin)
                        result.top = tmpTop;
                }
            }
        }

        /// <summary>
        /// This method will return the possible position of UI element based on a specific context.
        /// </summary>
        /// <param name="element">The element which needs to be positioned</param>
        /// <param name="anchor">The element used as an anchor for the element.</param>
        /// <param name="panel">The panel containing elements.</param>
        /// <param name="favoritePlacement">The preferred placement.</param>
        /// <param name="offset">An offset in the direction of the primary placement vector.</param>
        /// <param name="crossOffset">An offset in the direction of the secondary placement vector.</param>
        /// <param name="shouldFlip">`True` if the element's position can be flipped if there's not enough space.</param>
        /// <param name="crossSnap">`True` if the UI element should snap to the screen borders if the result overflows.</param>
        /// <returns>The computed position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The provided `favoritePlacement` value is invalid.</exception>
        public static PositionResult ComputePosition(VisualElement element, VisualElement anchor, Panel panel, PopoverPlacement favoritePlacement = PopoverPlacement.Bottom, int offset = 0, int crossOffset = 0, bool shouldFlip = true, bool crossSnap = true)
        {
            var anchorRect = anchor.worldBound;
            anchorRect.x -= panel.worldBound.x;
            anchorRect.y -= panel.worldBound.y;
            var screenRect = new Rect(Vector2.zero, panel.worldBound.size);
            var elementRect = element.worldBound;
            var halfHorizontalDeltaWidth = (elementRect.width - anchorRect.width) * 0.5f;
            var halfVerticalDeltaWidth = (elementRect.height - anchorRect.height) * 0.5f;

            var result = new PositionResult();
            result.finalPlacement = favoritePlacement;

            if (float.IsNaN(halfHorizontalDeltaWidth) || float.IsNaN(halfVerticalDeltaWidth))
                return result;

            switch (favoritePlacement)
            {
                case PopoverPlacement.Bottom:
                    result.top = anchorRect.yMax;
                    result.left = anchorRect.x - halfHorizontalDeltaWidth;
                    result.marginLeft = crossOffset;
                    result.marginTop = offset;
                    if (result.top + elementRect.height + result.marginTop > screenRect.height && shouldFlip)
                    {
                        result.marginTop = -offset;
                        result.top = anchorRect.yMin - elementRect.height;
                        result.finalPlacement = PopoverPlacement.Top;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.BottomLeft:
                    result.top = anchorRect.yMax;
                    result.left = anchorRect.x;
                    result.marginLeft = crossOffset;
                    result.marginTop = offset;
                    if (result.top + elementRect.height + result.marginTop > screenRect.height && shouldFlip)
                    {
                        result.marginTop = -offset;
                        result.top = anchorRect.yMin - elementRect.height;
                        result.finalPlacement = PopoverPlacement.TopLeft;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.BottomRight:
                    result.top = anchorRect.yMax;
                    result.left = anchorRect.xMax - elementRect.width;
                    result.marginLeft = -crossOffset;
                    result.marginTop = offset;
                    if (result.top + elementRect.height + result.marginTop > screenRect.height && shouldFlip)
                    {
                        result.marginTop = -offset;
                        result.top = anchorRect.yMin - elementRect.height;
                        result.finalPlacement = PopoverPlacement.TopRight;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.BottomStart:
                    result.top = anchorRect.yMax;
                    result.left = anchorRect.x;
                    result.marginLeft = crossOffset;
                    result.marginTop = offset;
                    if (result.top + elementRect.height + result.marginTop > screenRect.height && shouldFlip)
                    {
                        result.marginTop = -offset;
                        result.top = anchorRect.yMin - elementRect.height;
                        result.finalPlacement = PopoverPlacement.TopStart;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.BottomEnd:
                    result.top = anchorRect.yMax;
                    result.left = anchorRect.xMax - elementRect.width;
                    result.marginLeft = -crossOffset;
                    result.marginTop = offset;
                    if (result.top + elementRect.height + result.marginTop > screenRect.height && shouldFlip)
                    {
                        result.marginTop = -offset;
                        result.top = anchorRect.yMin - elementRect.height;
                        result.finalPlacement = PopoverPlacement.TopEnd;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.Top:
                    result.top = anchorRect.yMin - elementRect.height;
                    result.left = anchorRect.x - halfHorizontalDeltaWidth;
                    result.marginLeft = crossOffset;
                    result.marginTop = -offset;
                    if (result.top + result.marginTop < screenRect.yMin && shouldFlip)
                    {
                        result.marginTop = offset;
                        result.top = anchorRect.yMax;
                        result.finalPlacement = PopoverPlacement.Bottom;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.TopLeft:
                    result.top = anchorRect.yMin - elementRect.height;
                    result.left = anchorRect.x;
                    result.marginLeft = crossOffset;
                    result.marginTop = -offset;
                    if (result.top + result.marginTop < screenRect.yMin && shouldFlip)
                    {
                        result.marginTop = offset;
                        result.top = anchorRect.yMax;
                        result.finalPlacement = PopoverPlacement.BottomLeft;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.TopRight:
                    result.top = anchorRect.yMin - elementRect.height;
                    result.left = anchorRect.xMax - elementRect.width;
                    result.marginLeft = -crossOffset;
                    result.marginTop = -offset;
                    if (result.top + result.marginTop < screenRect.yMin && shouldFlip)
                    {
                        result.marginTop = offset;
                        result.top = anchorRect.yMax;
                        result.finalPlacement = PopoverPlacement.BottomRight;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.TopStart:
                    result.top = anchorRect.yMin - elementRect.height;
                    result.left = anchorRect.x;
                    result.marginLeft = crossOffset;
                    result.marginTop = -offset;
                    if (result.top + result.marginTop < screenRect.yMin && shouldFlip)
                    {
                        result.marginTop = offset;
                        result.top = anchorRect.yMax;
                        result.finalPlacement = PopoverPlacement.BottomStart;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.TopEnd:
                    result.top = anchorRect.yMin - elementRect.height;
                    result.left = anchorRect.xMax - elementRect.width;
                    result.marginLeft = -crossOffset;
                    result.marginTop = -offset;
                    if (result.top + result.marginTop < screenRect.yMin && shouldFlip)
                    {
                        result.marginTop = offset;
                        result.top = anchorRect.yMax;
                        result.finalPlacement = PopoverPlacement.BottomEnd;
                    }

                    if (crossSnap)
                        CrossSnapHorizontally(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.Left:
                    result.top = anchorRect.yMin - halfVerticalDeltaWidth;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.Right;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.LeftTop:
                    result.top = anchorRect.yMin;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.RightTop;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.LeftBottom:
                    result.top = anchorRect.yMax - elementRect.height;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = -crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.RightBottom;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.Start:
                    result.top = anchorRect.yMin - halfVerticalDeltaWidth;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.End;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.StartTop:
                    result.top = anchorRect.yMin;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.EndTop;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.StartBottom:
                    result.top = anchorRect.yMax - elementRect.height;
                    result.left = anchorRect.xMin - elementRect.width;
                    result.marginLeft = -offset;
                    result.marginTop = -crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = offset;
                        result.left = anchorRect.xMax;
                        result.finalPlacement = PopoverPlacement.EndBottom;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.Right:
                    result.top = anchorRect.yMin - halfVerticalDeltaWidth;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.Left;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.RightTop:
                    result.top = anchorRect.yMin;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.LeftTop;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.RightBottom:
                    result.top = anchorRect.yMax - elementRect.height;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = -crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.LeftBottom;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.End:
                    result.top = anchorRect.yMin - halfVerticalDeltaWidth;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.Start;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.EndTop:
                    result.top = anchorRect.yMin;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.StartTop;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                case PopoverPlacement.EndBottom:
                    result.top = anchorRect.yMax - elementRect.height;
                    result.left = anchorRect.xMax;
                    result.marginLeft = offset;
                    result.marginTop = -crossOffset;
                    if (result.left + result.marginLeft < screenRect.xMin && shouldFlip)
                    {
                        result.marginLeft = -offset;
                        result.left = anchorRect.xMin - elementRect.width;
                        result.finalPlacement = PopoverPlacement.StartBottom;
                    }

                    if (crossSnap)
                        CrossSnapVertically(ref result, screenRect, elementRect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(favoritePlacement), favoritePlacement, null);
            }

            const float tipHalfSize = 6;
            const float autoLength = -1;
            const float popoverPadding = 12;

            // compute tip/arrow placement
            switch (result.finalPlacement)
            {
                case PopoverPlacement.Bottom:
                case PopoverPlacement.BottomLeft:
                case PopoverPlacement.BottomRight:
                case PopoverPlacement.BottomStart:
                case PopoverPlacement.BottomEnd:
                    result.tipTop = tipHalfSize;
                    result.tipBottom = autoLength;
                    result.tipLeft = Mathf.Clamp(anchorRect.center.x - (result.left + result.marginLeft), popoverPadding * 2, elementRect.width - popoverPadding * 2);
                    result.tipRight = autoLength;
                    break;
                case PopoverPlacement.Top:
                case PopoverPlacement.TopLeft:
                case PopoverPlacement.TopRight:
                case PopoverPlacement.TopStart:
                case PopoverPlacement.TopEnd:
                    result.tipTop = autoLength;
                    result.tipBottom = tipHalfSize;
                    result.tipLeft = Mathf.Clamp(anchorRect.center.x - (result.left + result.marginLeft), popoverPadding * 2, elementRect.width - popoverPadding * 2);
                    result.tipRight = autoLength;
                    break;
                case PopoverPlacement.Left:
                case PopoverPlacement.LeftTop:
                case PopoverPlacement.LeftBottom:
                case PopoverPlacement.Start:
                case PopoverPlacement.StartTop:
                case PopoverPlacement.StartBottom:
                    result.tipTop = Mathf.Clamp(anchorRect.center.y - (result.top + result.marginTop), popoverPadding * 2, elementRect.height - popoverPadding * 2);
                    result.tipBottom = autoLength;
                    result.tipLeft = autoLength;
                    result.tipRight = tipHalfSize;
                    break;
                case PopoverPlacement.Right:
                case PopoverPlacement.RightTop:
                case PopoverPlacement.RightBottom:
                case PopoverPlacement.End:
                case PopoverPlacement.EndTop:
                case PopoverPlacement.EndBottom:
                    result.tipTop = Mathf.Clamp(anchorRect.center.y - (result.top + result.marginTop), popoverPadding * 2, elementRect.height - popoverPadding * 2);
                    result.tipBottom = autoLength;
                    result.tipLeft = tipHalfSize;
                    result.tipRight = autoLength;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}
