using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class Pauser : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        
        private Timer _timer;

        private void Awake()
        {
            _timer = GetComponent<Timer>();
        }

        private void OnPause()
        {
            
        }
    }
}
