using System;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace CYAN4S
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Dropdown outputDevices;
        [SerializeField] private TMP_Dropdown bufferSizes;

        private void Awake()
        {
            outputDevices.onValueChanged.AddListener(i => AudioManager.Instance.ChangeDevice(i));
            bufferSizes.onValueChanged.AddListener(i => AudioManager.Instance.ChangeSize((uint)math.pow(2, i + 5)));
        }

        private void Start()
        {
            outputDevices.AddOptions(AudioManager.Instance.drivers.Select(a => a.name).ToList());
        }

        public void SwitchActive()
        {
            panel.SetActive(!panel.activeSelf);
        }
        
    }
}
