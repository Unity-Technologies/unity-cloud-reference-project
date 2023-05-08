using System;
using UnityEngine;
using UnityEngine.Events;
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

        [HideInInspector]
        public UnityEvent ToolOpened;

        [HideInInspector]
        public UnityEvent ToolClosed;

        [HideInInspector]
        public UnityEvent ToolPointerEntered;

        [HideInInspector]
        public UnityEvent ToolPointerExited;

        [HideInInspector]
        public UnityEvent ToolFocusIn;

        [HideInInspector]
        public UnityEvent ToolFocusOut;

        public Action Close;

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
            ToolOpened.AddListener(OnToolOpened);
            ToolClosed.AddListener(OnToolClosed);
        }

        void OnDestroy()
        {
            if (m_RootVisualElement != null)
            {
                UnregisterCallbacks(m_RootVisualElement);
            }
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
    }
}
