using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class SequencerNote : MonoBehaviour
    {
        public float beat;

        private RectTransform rt;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }
        
        

    }
}
