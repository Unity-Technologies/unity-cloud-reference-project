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

        public event Action ToolOpened;
        public event Action ToolClosed;
        public event Action ToolPointerEntered;
        public event Action ToolPointerExited;
        public event Action ToolFocusIn;
        public event Action ToolFocusOut;

        Action m_CloseAction;

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

        public event Action<Sprite> IconChanged;

        protected virtual VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var visualElement = template != null ? template.Instantiate() : new VisualElement();
            RegisterCallbacks(visualElement);
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
        }

        protected virtual void UnregisterCallbacks(VisualElement visualElement)
        {
            visualElement.UnregisterCallback<PointerEnterEvent>(OnPointerEntered);
            visualElement.UnregisterCallback<PointerLeaveEvent>(OnPointerExited);
            visualElement.UnregisterCallback<FocusInEvent>(OnFocusIn);
            visualElement.UnregisterCallback<FocusOutEvent>(OnFocusOut);
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

            icon.AddToClassList(k_ActionButtonIconUssClassName);
            return icon;
        }
    }
}
