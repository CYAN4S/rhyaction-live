using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public abstract class ChannelSO<T> : ScriptableObject, ISerializationCallbackReceiver
    {
        public T initialValue;
        public T value;
        public event UnityAction<T> OnEventRaised;

        public void OnBeforeSerialize()
        {
            value = initialValue;
        }

        public void OnAfterDeserialize()
        {
        }
        
        public void RaiseEvent(T data)
        {
            value = data;
            OnEventRaised?.Invoke(data);
        }
    }
}