using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The FullScreen mode used by a <see cref="Modal"/> component.
    /// </summary>
    public enum ModalFullScreenMode
    {
        /// <summary>
        /// The <see cref="Modal"/> is displayed as a normal size.
        /// </summary>
        None,

        /// <summary>
        /// The <see cref="Modal"/> is displayed in fullscreen but a small margin still present
        /// to display the <see cref="Modal"/> smir.
        /// </summary>
        FullScreen,

        /// <summary>
        /// The <see cref="Modal"/> is displayed in fullscreen without any margin.
        /// The <see cref="Modal"/> smir won't be reachable.
        /// </summary>
        FullScreenTakeOver
    }

    /// <summary>
    /// Interface that must be implemented by any UI component which wants to
    /// request a <see cref="Popup.Dismiss(DismissType)"/> if this component is displayed
    /// inside a <see cref="Popup"/> component.
    /// </summary>
    public interface IDismissInvocator
    {
        /// <summary>
        /// Event triggered when the UI component wants to request a <see cref="Popup.Dismiss(DismissType)"/>
        /// </summary>
        event Action<DismissType> dismissRequested;
    }

    /// <summary>
    /// The Modal Popup class.
    /// </summary>
    public sealed class Modal : Popup<Modal>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="modalView">The popup visual element itself.</param>
        /// <param name="content">The content that will appear inside this popup.</param>
        Modal(VisualElement parentView, ApplicationContext context, ModalVisualElement modalView, VisualElement content)
            : base(parentView, context, modalView, content)
        {
        }

        ModalVisualElement modal => (ModalVisualElement)view;

        /// <summary>
        /// Set the fullscreen mode for this <see cref="Modal"/>.
        /// <para>
        /// See <see cref="ModalFullScreenMode"/> values for more info.
        /// </para>
        /// </summary>
        public ModalFullScreenMode fullscreenMode
        {
            get => modal.fullScreenMode;
            set => modal.fullScreenMode = value;
        }

        /// <summary>
        /// Set a new value for <see cref="fullscreenMode"/> property.
        /// </summary>
        /// <param name="mode">The new value.</param>
        /// <returns>The <see cref="Modal"/> object.</returns>
        public Modal SetFullScreenMode(ModalFullScreenMode mode)
        {
            fullscreenMode = mode;
            return this;
        }

        /// <summary>
        /// Build a new Modal component.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element inside the UI panel.</param>
        /// <param name="content">The <see cref="VisualElement"/> UI element to display inside this <see cref="Modal"/>.</param>
        /// <returns>The <see cref="Modal"/> instance.</returns>
        public static Modal Build(VisualElement referenceView, VisualElement content)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.popupContainer;
            var popup = new Modal(parentView, context, new ModalVisualElement(content), content)
                .SetLastFocusedElement(referenceView);
            return popup;
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return modal.contentContainer;
        }

        /// <summary>
        /// The Modal UI Element.
        /// </summary>
        class ModalVisualElement : VisualElement
        {
            public static readonly string ussClassName = "appui-modal";

            public static readonly string fullScreenUssClassName = ussClassName + "--fullscreen";

            public static readonly string fullScreenTakeOverUssClassName = ussClassName + "--fullscreen-takeover";

            public static readonly string contentContainerUssClassName = ussClassName + "__content";

            readonly VisualElement m_ContentContainer;

            ModalFullScreenMode m_FullScreenMode = ModalFullScreenMode.None;

            public ModalFullScreenMode fullScreenMode
            {
                get => m_FullScreenMode;
                set
                {
                    m_FullScreenMode = value;
                    EnableInClassList(fullScreenUssClassName, m_FullScreenMode == ModalFullScreenMode.FullScreen);
                    EnableInClassList(fullScreenTakeOverUssClassName, m_FullScreenMode == ModalFullScreenMode.FullScreenTakeOver);
                }
            }

            public ModalVisualElement(VisualElement content)
            {
                AddToClassList(ussClassName);

                pickingMode = PickingMode.Position;
                focusable = true;

                m_ContentContainer = new ExVisualElement { name = contentContainerUssClassName, pickingMode = PickingMode.Position, focusable = true, passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows };
                m_ContentContainer.AddToClassList(contentContainerUssClassName);

                hierarchy.Add(m_ContentContainer);

                m_ContentContainer.Add(content);
                fullScreenMode = ModalFullScreenMode.None;
            }

            public override VisualElement contentContainer => m_ContentContainer;
        }
    }
}
