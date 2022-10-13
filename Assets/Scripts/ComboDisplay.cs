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

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void ComboChanged(int combo)
        {
            _text.text = $"{combo}";
        }
    }
}
