using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ComboDisplay : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Animator _animator;
        
        private static readonly int ComboAnimaID = Animator.StringToHash("Combo");

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _animator = GetComponent<Animator>();
        }

        public void ComboChanged(int combo)
        {
            _text.text = $"{combo}";
            _animator.SetTrigger(ComboAnimaID);
        }
    }
}
