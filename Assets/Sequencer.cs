using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace CYAN4S
{
    public class Sequencer : MonoBehaviour, IScrollHandler
    {
        [Header("Debug")] 
        public Chart chart;
        public float scale = 2;
        public float currentBeat;

        [Header("Set on inspector")] 
        public TextMeshProUGUI currentBeatText;
        public TextMeshProUGUI ScaleText;

        public event Action<float> OnCurrentBeatChange;
        public event Action<float> OnScaleChange;

        public static Sequencer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Keyboard.current.leftCtrlKey.isPressed)
            {
                scale += eventData.scrollDelta.y / 120f;
                ScaleText.text = $"{scale:F2}";
                OnScaleChange?.Invoke(scale);
                
                return;
            }
            
            currentBeat += eventData.scrollDelta.y / 120f;
            currentBeatText.text = $"{currentBeat:F2}";
            OnCurrentBeatChange?.Invoke(currentBeat);
            
            Debug.Log(eventData.scrollDelta);
        }
    }
}