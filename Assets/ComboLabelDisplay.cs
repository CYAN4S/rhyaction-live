using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class ComboLabelDisplay : MonoBehaviour
    {
        private Animator _animator;
        
        private static readonly int ComboAnimaID = Animator.StringToHash("Combo");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void ComboChanged()
        {
            _animator.SetTrigger(ComboAnimaID);
        }
    }
}
