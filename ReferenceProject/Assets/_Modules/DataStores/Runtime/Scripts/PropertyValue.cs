using System;

namespace Unity.ReferenceProject.DataStores
{
    public delegate TValue GetValue<out TValue>(string propertyName);
    public delegate bool SetValue<in TValue>(string propertyName, TValue value, UpdateNotification notifyChange);
    public delegate bool SetValueAction<TValue>(string propertyName, ModifyReference<TValue> modifyAction, UpdateNotification notifyChange);

    public struct PropertyDelegates<TValue>
    {
        public readonly GetValue<TValue> GetValue;
        public readonly SetValue<TValue> SetValue;
        public readonly SetValueAction<TValue> SetValueAction;

        public PropertyDelegates(GetValue<TValue> getValue, SetValue<TValue> setValue, SetValueAction<TValue> setValueAction)
        {
            GetValue = getValue;
            SetValue = setValue;
            SetValueAction = setValueAction;
        }
    }

    public class PropertyValue<TValue>
    {
        readonly string m_PropertyName;
        readonly PropertyDelegates<TValue> m_PropertyDelegates;
        readonly INotifyPropertyChanged m_NotifyPropertyChanged;

        public Action<TValue> OnValueChanged;

        public PropertyValue(string propertyName, PropertyDelegates<TValue> delegates, INotifyPropertyChanged notifyPropertyChanged)
        {
            m_PropertyDelegates = delegates;
            m_PropertyName = propertyName;
            m_NotifyPropertyChanged = notifyPropertyChanged;
            m_NotifyPropertyChanged.OnPropertyChanged += OnPropertyChanged;
        }

        ~PropertyValue()
        {
            m_NotifyPropertyChanged.OnPropertyChanged -= OnPropertyChanged;
        }

        void OnPropertyChanged(string propertyName)
        {
            if (propertyName == m_PropertyName)
                OnValueChanged?.Invoke(GetValue());
        }

        public TValue GetValue()
        {
            return m_PropertyDelegates.GetValue.Invoke(m_PropertyName);
        }

        public bool SetValue(TValue value, UpdateNotification notifyChange = UpdateNotification.NotifyIfChange)
        {
            return m_PropertyDelegates.SetValue.Invoke(m_PropertyName, value, notifyChange);
        }

        public bool SetValue(ModifyReference<TValue> updateAction,
            UpdateNotification notifyChange = UpdateNotification.NotifyIfChange)
        {
            return m_PropertyDelegates.SetValueAction.Invoke(m_PropertyName, updateAction, notifyChange);
        }
    }
}
