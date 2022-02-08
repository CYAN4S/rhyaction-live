using System;
using UnityEngine;

namespace CYAN4S
{
    [CreateAssetMenu(fileName = "JudgeStandardSO", menuName = "Judge Standard SO", order = 0)]
    public class JudgeStandardSO : ScriptableObject
    {
        public float rushToBreak;
        public float ignorable;
        public float missed;
    }
}