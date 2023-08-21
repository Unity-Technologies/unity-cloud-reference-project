using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    public abstract class ToolUIController : MonoBehaviour
    {
        [SerializeField]
        string m_DisplayName;

        [SerializeField]
        Sprite m_Icon;

        [SerializeField]
        VisualTreeAsset m_Template;

        [SerializeField]
        bool m_HideButtonAtStart;

        public event Action ToolOpened;
        public event Action ToolClosed;
        public event Action ToolPointerEntered;
        public event Action ToolPointerExited;
        public event Action ToolFocusIn;
        public event Action ToolFocusOut;
        public event Action ToolPointerDown;
        public event Action ToolPointerUp;
        public event Action ToolPointerCapture;
        public event Action ToolPointerCaptureOut;
        public event Action<Sprite> IconChanged;

        Action m_CloseAction;
        Action<DisplayStyle> m_SetButtonDisplayStyleAction;

        static readonly string k_ActionButtonIconUssClassName = "appui-actionbutton__icon";

        VisualElement m_RootVisualElement;

        public string DisplayName
        {
            get => m_DisplayName;
            set => m_DisplayName = value;
        }

        public Sprite Icon
        {
            get => m_Icon;
            set
            {
                m_Icon = value;
                IconChanged?.Invoke(value);
            }
        }

        public VisualTreeAsset Template
        {
            get => m_Template;
            set => m_Template = value;
        }

        public VisualElement RootVisualElement
        {
            get { return m_RootVisualElement ??= CreateVisualTree(m_Template); }
        }

        protected virtual void Awake()
        {
            ToolOpened += OnToolOpened;
            ToolClosed += OnToolClosed;
        }

        protected virtual void OnDestroy()
        {
            if (m_RootVisualElement != null)
            {
                UnregisterCallbacks(m_RootVisualElement);
            }
        }

        public void InvokeToolOpened()
        {
            ToolOpened?.Invoke();
        }

        public void InvokeToolClosed()
        {
            ToolClosed?.Invoke();
        }

        public void SetCloseAction(Action action)
        {
            m_CloseAction = action;
        }

        protected void CloseSelf()
        {
            m_CloseAction?.Invoke();
        }

        public void SetButtonDisplayStyleAction(Action<DisplayStyle> action)
        {
            m_SetButtonDisplayStyleAction = action;

            if (m_HideButtonAtStart)
            {
                m_SetButtonDisplayStyleAction?.Invoke(DisplayStyle.None);
            }
        }

        protected void SetButtonDisplayStyle(DisplayStyle style)
        {
            m_SetButtonDisplayStyleAction?.Invoke(style);
        }

        protected virtual VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var visualElement = template != null ? template.Instantiate() : new VisualElement();
            RegisterCallbacks(visualElement);
            visualElement.style.flexGrow = 1;
            return visualElement;
        }

        protected void SetRootVisualElement(VisualElement visualElement)
        {
            if (m_RootVisualElement != null)
            {
                UnregisterCallbacks(m_RootVisualElement);
            }

            RegisterCallbacks(visualElement);
            m_RootVisualElement = visualElement;
        }

        protected virtual void RegisterCallbacks(VisualElement visualElement)
        {
            visualElement.RegisterCallback<PointerEnterEvent>(OnPointerEntered);
            visualElement.RegisterCallback<PointerLeaveEvent>(OnPointerExited);
            visualElement.RegisterCallback<FocusInEvent>(OnFocusIn);
            visualElement.RegisterCallback<FocusOutEvent>(OnFocusOut);
            visualElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            visualElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
            visualElement.RegisterCallback<PointerCaptureEvent>(OnPointerCaptureEvent);
            visualElement.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOutEvent);
        }

        protected virtual void UnregisterCallbacks(VisualElement visualElement)
        {
            visualElement.UnregisterCallback<PointerEnterEvent>(OnPointerEntered);
            visualElement.UnregisterCallback<PointerLeaveEvent>(OnPointerExited);
            visualElement.UnregisterCallback<FocusInEvent>(OnFocusIn);
            visualElement.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            visualElement.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            visualElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            visualElement.UnregisterCallback<PointerCaptureEvent>(OnPointerCaptureEvent);
            visualElement.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOutEvent);
        }

        public virtual void OnToolOpened() { }

        public virtual void OnToolClosed() { }

        public virtual void OnPointerEntered(PointerEnterEvent evt)
        {
            ToolPointerEntered?.Invoke();
        }

        public virtual void OnPointerExited(PointerLeaveEvent evt)
        {
            ToolPointerExited?.Invoke();
        }

        public virtual void OnFocusIn(FocusInEvent evt)
        {
            ToolFocusIn?.Invoke();
        }

        public virtual void OnFocusOut(FocusOutEvent evt)
        {
            ToolFocusOut?.Invoke();
        }

        public virtual void OnPointerDown(PointerDownEvent evt)
        {
            ToolPointerDown?.Invoke();
        }

        public virtual void OnPointerUp(PointerUpEvent evt)
        {
            ToolPointerUp?.Invoke();
        }

        public virtual void OnPointerCaptureEvent(PointerCaptureEvent evt)
        {
            ToolPointerCapture?.Invoke();
        }

        public virtual void OnPointerCaptureOutEvent(PointerCaptureOutEvent evt)
        {
            ToolPointerCaptureOut?.Invoke();
        }

        public virtual VisualElement GetButtonContent()
        {
            return GetIcon();
        }

        protected Icon GetIcon()
        {
            var icon = new Icon
            {
                sprite = m_Icon,
                size = IconSize.L
            };

            IconChanged += sprite => icon.sprite = sprite;
            icon.AddToClassList(k_ActionButtonIconUssClassName);
            return icon;
        }
    }
}
