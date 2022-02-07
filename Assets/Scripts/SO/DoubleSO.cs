using System;
using UnityEngine;

namespace CYAN4S
{
    [CreateAssetMenu(fileName = "DoubleValue", menuName = "Double SO", order = 0)]
    public class DoubleSO : ScriptableObject, ISerializationCallbackReceiver
    {
        public double initialValue;

        [NonSerialized] public double Value;

        public void OnAfterDeserialize()
        {
            Value = initialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}