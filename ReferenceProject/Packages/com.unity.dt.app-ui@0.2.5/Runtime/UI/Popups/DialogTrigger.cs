using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// By providing a type prop, you can specify the type of Dialog that is rendered by your DialogTrigger.
    /// <remarks>
    /// Note that pressing the Esc key will close the Dialog regardless of its type.
    /// </remarks>
    /// </summary>
    public enum PopupPresentationType
    {
        /// <summary>
        /// Modal Dialogs create an underlay that blocks access to the underlying user interface until the Dialog is closed.
        /// Sizing options can be found on the Dialog page.
        /// Focus is trapped inside the Modal.
        /// </summary>
        Modal,

        /// <summary>
        /// If a Dialog without an underlay is needed, consider using a Popover Dialog.
        /// See Dialog placement for how you can customize the positioning.
        /// Note that popovers are automatically rendered as modals on mobile by default.
        /// See the mobile type option for more information.
        /// </summary>
        Popover,

        /// <summary>
        /// Tray Dialogs are typically used to portray information on mobile devices or smaller screens.
        /// </summary>
        Tray,

        /// <summary>
        /// Fullscreen Dialogs are a fullscreen variant of the Modal Dialog, only revealing a small portion of the page
        /// behind the underlay. Use this variant for more complex workflows that do not fit in the available
        /// Modal Dialog sizes.
        /// This variant does not support dismissible.
        /// </summary>
        FullScreen,

        /// <summary>
        /// Fullscreen takeover Dialogs are similar to the fullscreen variant except that the Dialog covers the entire screen.
        /// </summary>
        FullScreenTakeOver,
    }

    /// <summary>
    /// Same as <see cref="PopupPresentationType"/> but for Mobile explicitly.
    /// </summary>
    public enum MobilePopupPresentationType
    {
        /// <summary>
        /// Modal Dialogs create an underlay that blocks access to the underlying user interface until the Dialog is closed.
        /// Sizing options can be found on the Dialog page.
        /// Focus is trapped inside the Modal.
        /// </summary>
        Modal,

        /// <summary>
        /// Tray Dialogs are typically used to portray information on mobile devices or smaller screens.
        /// </summary>
        Tray,

        /// <summary>
        /// Fullscreen Dialogs are a fullscreen variant of the Modal Dialog, only revealing a small portion of the page
        /// behind the underlay. Use this variant for more complex workflows that do not fit in the available
        /// Modal Dialog sizes.
        /// This variant does not support dismissible.
        /// </summary>
        FullScreen,

        /// <summary>
        /// Fullscreen takeover Dialogs are similar to the fullscreen variant except that the Dialog covers the entire screen.
        /// </summary>
        FullScreenTakeOver,
    }

    /// <summary>
    /// DialogTrigger serves as a wrapper around a Dialog and its associated trigger,
    /// linking the Dialog's open state with the trigger's press state. Additionally,
    /// it allows you to customize the type and positioning of the Dialog.
    /// </summary>
    public class DialogTrigger : VisualElement
    {
        string m_AnchorName = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DialogTrigger()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// The dialog to display.
        /// </summary>
        public Dialog dialog { get; private set; }

        /// <summary>
        /// The trigger that will be used to start the display of the <see cref="dialog"/> element.
        /// </summary>
        public VisualElement trigger { get; private set; }

        /// <summary>
        /// The type of presentation used for this <see cref="dialog"/> element.
        /// <remarks>Some types are not available on mobile, to specify different presentation on mobile context
        /// use the <see cref="mobileType"/> property.</remarks>
        /// </summary>
        public PopupPresentationType type { get; set; }

        /// <summary>
        /// The position of the Tray element.
        /// <remarks>This property is useful only if you set the <see cref="type"/> property to <see cref="PopupPresentationType.Tray"/>.</remarks>
        /// </summary>
        public TrayPosition trayPosition { get; set; }

        /// <summary>
        /// The size of the Tray element.
        /// </summary>
        public float traySize { get; set; }

        /// <summary>
        /// Make the Tray element expandable.
        /// </summary>
        public bool trayExpandable { get; set; }

        /// <summary>
        /// The duration of the transition in milliseconds.
        /// </summary>
        public int transitionDuration { get; set; }

        /// <summary>
        /// The margin in pixels of the Tray element.
        /// </summary>
        public float trayMargin { get; set; }

        /// <summary>
        /// Should the arrow be hidden.
        /// <remarks>
        /// This property is only useful with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        /// </summary>
        public bool hideArrow { get; set; }

        /// <summary>
        /// The type of presentation used for this <see cref="dialog"/> element on mobile platforms.
        /// </summary>
        public MobilePopupPresentationType mobileType { get; set; }

        /// <summary>
        /// The padding in pixels of the content inside the Popup.
        /// </summary>
        public int containerPadding { get; set; }

        /// <summary>
        /// The offset in pixels in the direction of the <see cref="placement"/> primary vector.
        /// </summary>
        public int offset { get; set; }

        /// <summary>
        /// The offset in pixels in the direction of the <see cref="placement"/> secondary vector.
        /// </summary>
        public int crossOffset { get; set; }

        /// <summary>
        /// Should the Popover <see cref="placement"/> be flipped if there's not enough space.
        /// </summary>
        public bool shouldFlip { get; set; }

        /// <summary>
        /// The open state of the dialog.
        /// </summary>
        public bool isOpen { get; private set; } = false;

        /// <summary>
        /// Disallow the use of Escape key or Return button to dismiss the <see cref="dialog"/>.
        /// </summary>
        public bool keyboardDismissDisabled { get; set; }

        /// <summary>
        /// Allow the use of clicking outside the <see cref="dialog"/> to dismiss it.
        /// </summary>
        /// <remarks>
        /// This property works only with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        public bool outsideClickDismissEnabled { get; set; }

        /// <summary>
        /// Enable or disable the blocking of the UI behind the <see cref="dialog"/>.
        /// </summary>
        /// <remarks>
        /// This property works only with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        public bool modalBackdrop { get; set; }

        /// <summary>
        /// The UI element used as an anchor.
        /// <remarks>This is only useful for presentations using popups of type <see cref="AnchorPopup{T}"/>.</remarks>
        /// </summary>
        public VisualElement anchor { get; set; }

        /// <summary>
        /// The placement of the Popover.
        /// <remarks>This is only useful for presentations using popups of type <see cref="AnchorPopup{T}"/>.</remarks>
        /// </summary>
        public PopoverPlacement placement { get; set; }

        /// <summary>
        /// The content container of the DialogTrigger.
        /// </summary>
        public override VisualElement contentContainer => this;

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Dialog dlg = null;
            VisualElement ve = null;

            foreach (var child in Children())
            {
                if (dlg == null && child is Dialog d)
                    dlg = d;
                
                if (ve == null && !(child is Dialog))
                    ve = child;
                
                if (dlg != null && ve != null)
                    break;
            }
            
            if (dlg != null && dlg != dialog)
            {
                // New Dialog attached as child
                dialog = dlg;
                Remove(dlg);
            }

            if (ve != null && ve != trigger)
            {
                if (trigger is IClickable c1)
                    c1.clickable.clicked -= OnActionTriggered;
                trigger = ve;
                if (trigger is IClickable c2)
                    c2.clickable.clicked += OnActionTriggered;
            }

            // we can also try to find the anchor (if any has been given with the UXML attribute)
            if (!string.IsNullOrEmpty(m_AnchorName) && panel != null)
            {
                var anchorElement = panel.visualTree.Q<VisualElement>(m_AnchorName);
                if (anchorElement != null)
                    anchor = anchorElement;
                else
                    Debug.LogWarning($"Unable to find {m_AnchorName}");
            }
        }

        void OnActionTriggered()
        {
            switch (type)
            {
                case PopupPresentationType.Modal:
                    Modal.Build(trigger, dialog).Show();
                    break;
                case PopupPresentationType.Popover:
                    Popover.Build(trigger, dialog)
                        .SetPlacement(placement)
                        .SetShouldFlip(shouldFlip)
                        .SetOffset(offset)
                        .SetCrossOffset(crossOffset)
                        .SetArrowVisible(!hideArrow)
                        .SetContainerPadding(containerPadding)
                        .SetOutsideClickDismiss(outsideClickDismissEnabled)
                        .SetModalBackdrop(modalBackdrop)
                        .SetKeyboardDismiss(!keyboardDismissDisabled).Show();
                    break;
                case PopupPresentationType.Tray:
                    Tray.Build(trigger, dialog)
                        .SetPosition(trayPosition)
                        .SetSize(traySize)
                        .SetExpandable(trayExpandable)
                        .SetMargin(trayMargin)
                        .SetTransitionDuration(transitionDuration)
                        .Show();
                    break;
                case PopupPresentationType.FullScreen:
                    Modal.Build(trigger, dialog).SetFullScreenMode(ModalFullScreenMode.FullScreen).Show();
                    break;
                case PopupPresentationType.FullScreenTakeOver:
                    Modal.Build(trigger, dialog).SetFullScreenMode(ModalFullScreenMode.FullScreenTakeOver).Show();
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(type), type);
            }

            isOpen = true;
        }

        /// <summary>
        /// Class used to instantiate <see cref="DialogTrigger"/> from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<DialogTrigger, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="DialogTrigger"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Anchor = new UxmlStringAttributeDescription
            {
                name = "anchor",
                defaultValue = null
            };

            readonly UxmlIntAttributeDescription m_ContainerPadding = new UxmlIntAttributeDescription
            {
                name = "container-padding",
                defaultValue = 0
            };

            readonly UxmlIntAttributeDescription m_CrossOffset = new UxmlIntAttributeDescription
            {
                name = "cross-offset",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_HideArrow = new UxmlBoolAttributeDescription
            {
                name = "hide-arrow",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_KeyboardDismissDisabled = new UxmlBoolAttributeDescription
            {
                name = "keyboard-dismiss-disabled",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_OutsideClickDismissEnabled = new UxmlBoolAttributeDescription()
            {
                name = "outside-click-dismiss-enabled",
                defaultValue = true
            };

            readonly UxmlBoolAttributeDescription m_ModalBackdrop = new UxmlBoolAttributeDescription()
            {
                name = "modal-backdrop",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<MobilePopupPresentationType> m_MobileType = new UxmlEnumAttributeDescription<MobilePopupPresentationType>
            {
                name = "mobile-type",
                defaultValue = MobilePopupPresentationType.Modal
            };

            readonly UxmlIntAttributeDescription m_Offset = new UxmlIntAttributeDescription
            {
                name = "offset",
                defaultValue = 0
            };

            readonly UxmlEnumAttributeDescription<PopoverPlacement> m_Placement = new UxmlEnumAttributeDescription<PopoverPlacement>
            {
                name = "placement",
                defaultValue = PopoverPlacement.Top
            };

            readonly UxmlBoolAttributeDescription m_ShouldFlip = new UxmlBoolAttributeDescription
            {
                name = "should-flip",
                defaultValue = true
            };

            readonly UxmlEnumAttributeDescription<PopupPresentationType> m_Type = new UxmlEnumAttributeDescription<PopupPresentationType>
            {
                name = "type",
                defaultValue = PopupPresentationType.Modal
            };

            readonly UxmlEnumAttributeDescription<TrayPosition> m_TrayPosition = new UxmlEnumAttributeDescription<TrayPosition>
            {
                name = "tray-position",
                defaultValue = TrayPosition.Bottom
            };

            readonly UxmlFloatAttributeDescription m_TraySize = new UxmlFloatAttributeDescription
            {
                name = "tray-size",
                defaultValue = 200
            };

            readonly UxmlBoolAttributeDescription m_TrayExpandable = new UxmlBoolAttributeDescription
            {
                name = "tray-expandable",
                defaultValue = false
            };

            readonly UxmlIntAttributeDescription m_TransitionDuration = new UxmlIntAttributeDescription
            {
                name = "transition-duration",
                defaultValue = 150
            };

            readonly UxmlFloatAttributeDescription m_TrayMargin = new UxmlFloatAttributeDescription
            {
                name = "tray-margin",
                defaultValue = 0
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
                var el = (DialogTrigger)ve;

                el.type = m_Type.GetValueFromBag(bag, cc);
                el.trayPosition = m_TrayPosition.GetValueFromBag(bag, cc);
                el.traySize = m_TraySize.GetValueFromBag(bag, cc);
                el.trayExpandable = m_TrayExpandable.GetValueFromBag(bag, cc);
                el.trayMargin = m_TrayMargin.GetValueFromBag(bag, cc);
                el.transitionDuration = m_TransitionDuration.GetValueFromBag(bag, cc);
                el.mobileType = m_MobileType.GetValueFromBag(bag, cc);
                el.hideArrow = m_HideArrow.GetValueFromBag(bag, cc);
                el.placement = m_Placement.GetValueFromBag(bag, cc);
                el.offset = m_Offset.GetValueFromBag(bag, cc);
                el.crossOffset = m_CrossOffset.GetValueFromBag(bag, cc);
                el.containerPadding = m_ContainerPadding.GetValueFromBag(bag, cc);
                el.m_AnchorName = m_Anchor.GetValueFromBag(bag, cc);
                el.shouldFlip = m_ShouldFlip.GetValueFromBag(bag, cc);
                el.keyboardDismissDisabled = m_KeyboardDismissDisabled.GetValueFromBag(bag, cc);
                el.outsideClickDismissEnabled = m_OutsideClickDismissEnabled.GetValueFromBag(bag, cc);
                el.modalBackdrop = m_ModalBackdrop.GetValueFromBag(bag, cc);
            }
        }
    }
}
