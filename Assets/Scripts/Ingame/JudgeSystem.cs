using System;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class JudgeSystem
    {
        [Header("Judgement")] 
        [SerializeField] private float ignorable = 0.35f;
        [SerializeField] private float tooEarly = 0.3f;
        [SerializeField] private float fairEarly = 0.15f;
        [SerializeField] private float greatEarly = 0.1f;
        [SerializeField] private float preciseEarly = 0.05f;
        [SerializeField] private float preciseLate = -0.05f;
        [SerializeField] private float greatLate = -0.1f;
        [SerializeField] private float fairLate = -0.15f;
        [SerializeField] private float tooLate = -0.3f;


        public Judgement GetJudgement(double delta)
        {
            if (delta >      tooEarly) return Judgement.Break;
            if (delta >     fairEarly) return Judgement.Poor;
            if (delta >    greatEarly) return Judgement.Fair;
            if (delta >  preciseEarly) return Judgement.Great;
            if (delta >= preciseLate)  return Judgement.Precise;
            if (delta >=   greatLate)  return Judgement.Great;
            if (delta >=    fairLate)  return Judgement.Fair;
            if (delta >=     tooLate)  return Judgement.Poor;
            return Judgement.Break;
        }

        public bool IsIgnorable(double delta)
        {
            return ignorable <= delta;
        }

        public bool IsTooLate(double delta)
        {
            return delta < tooLate;
        }

        public bool IsTooEarly(double delta)
        {
            return delta > tooEarly;
        }

        public bool IsIgnorableOrTooLate(double delta)
        {
            return IsIgnorable(delta) || IsTooLate(delta);
        }
    }
    
    public enum Judgement { Precise, Great, Fair, Poor, Break }
}