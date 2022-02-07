using System;
using UnityEngine;

namespace CYAN4S
{
    [CreateAssetMenu(fileName = "FloatValue", menuName = "Float SO", order = 0)]
    public class FloatSO : ScriptableObject, ISerializationCallbackReceiver
    {
        public float initialValue;

        [NonSerialized] public float Value;

        public void OnAfterDeserialize()
        {
            Value = initialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}