using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.ReferenceProject.DataStores
{
    public delegate void ModifyReference<T>(ref T item);

    public interface INotifyPropertyChanged
    {
        public Action<string> OnPropertyChanged { get; set; }
    }

    public abstract class DataStore<TContainer> : MonoBehaviour, INotifyPropertyChanged
    {
        [SerializeField, Tooltip("State of the Store")]
        TContainer dataContainer;

        readonly Dictionary<string, object> m_CachedProperties = new();

        public Action<string> OnPropertyChanged { get; set; }

        void Start()
        {
            ForceNotify();
        }

        public PropertyValue<TValue> GetProperty<TValue>(string propertyName)
        {
            if (!ValidateProperty<TValue>(propertyName))
                return null;

            if (m_CachedProperties.ContainsKey(propertyName))
                return (PropertyValue<TValue>) m_CachedProperties[propertyName];

            var propertyDelegates = new PropertyDelegates<TValue>
            {
                GetValue = GetValue<TValue>,
                SetValue = SetValue,
                SetValueAction = SetValue
            };

            var property = new PropertyValue<TValue>(propertyName, propertyDelegates, this);
            m_CachedProperties.Add(propertyName, property);
            return property;
        }

        bool ValidateProperty<TValue>(string propertyName)
        {
            if (!PropertyContainer.TryGetProperty(ref dataContainer, new PropertyPath(propertyName), out var prop,
                    out var returnCode))
            {
                Debug.LogWarning($"Failed to get property '{propertyName}': {returnCode}");
                return false;
            }

            var propType = prop.DeclaredValueType();
            if (propType == typeof(TValue)) return true;

            Debug.LogWarning($"Property type does not match! {typeof(TValue)} != {propType}");
            return false;
        }

        public void ForceNotify()
        {
            UpdateContainer(dataContainer, UpdateNotification.ForceNotify);
        }

        void UpdateContainer(TContainer data,
            UpdateNotification notifyChange = UpdateNotification.NotifyIfChange)
        {
            var propertyBag = PropertyBag.GetPropertyBag<TContainer>();
            foreach (var property in propertyBag.GetProperties(ref dataContainer))
            {
                var propertyName = property.Name;
                var value = property.GetValue(ref data);
                SetValue(propertyName, value, notifyChange);
            }
        }

        protected bool SetValue<TValue>(string propertyName, TValue value,
            UpdateNotification notifyChange = UpdateNotification.NotifyIfChange)
        {
            var oldValue = GetValue<TValue>(propertyName);
            if (!PropertyContainer.TrySetValue(ref dataContainer, propertyName, value))
                return false;

            var changed = Compare(value, oldValue);
            
            if (notifyChange == UpdateNotification.DoNotNotify)
                return changed;

            if (notifyChange == UpdateNotification.ForceNotify ||
                (changed || IsNullable<TValue>()) && notifyChange == UpdateNotification.NotifyIfChange)
            {
                OnPropertyChanged?.Invoke(propertyName);
            }

            return changed;
        }

        protected bool SetValue<TValue>(string propertyName, ModifyReference<TValue> modifyAction,
            UpdateNotification notifyChange = UpdateNotification.NotifyIfChange)
        {
            var value = GetValue<TValue>(propertyName);
            var oldValue = value;

            modifyAction?.Invoke(ref value);

            if (!PropertyContainer.TrySetValue(ref dataContainer, propertyName, value))
                return false;

            var changed = Compare(value, oldValue);

            if (notifyChange == UpdateNotification.DoNotNotify)
                return changed;

            if (notifyChange == UpdateNotification.ForceNotify ||
                (changed || IsNullable<TValue>()) && notifyChange == UpdateNotification.NotifyIfChange)
            {
                OnPropertyChanged?.Invoke(propertyName);
            }

            return changed;
        }

        bool Compare<TValue>(TValue value, TValue oldValue)
        {
            var changed = false;
            
            //Use .Equals() if value is not null
            if (value != null)
                changed = !value.Equals(oldValue);
            //Is true if the new value is null but the previous is not.
            else if (oldValue != null) 
                changed = true;

            return changed;
        }

        protected TValue GetValue<TValue>(string propertyName)
        {
            return PropertyContainer.GetValue<TContainer, TValue>(ref dataContainer, propertyName);
        }

        static bool IsNullable<TValue>()
        {
            return default(TValue) == null;
        }
    }
}
