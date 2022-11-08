using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class PressDisplay : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int IsPressed = Animator.StringToHash("IsPressed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetBool(bool value)
        {
            _animator.SetBool(IsPressed, value);
        }
    }
}
