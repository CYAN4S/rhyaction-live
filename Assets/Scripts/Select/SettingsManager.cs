using System.Linq;
using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Dropdown outputDevices;
        [SerializeField] private TMP_Dropdown bufferSizes;

        private void Awake()
        {
            panel.SetActive(false);
            
            outputDevices.AddOptions(AudioManager.Instance.drivers.Select(a => a.name).ToList());
            outputDevices.value = AudioManager.Instance.currentDriver;
            bufferSizes.value = AudioManager.Instance.currentBufferSize;
            
            outputDevices.onValueChanged.AddListener(i => AudioManager.Instance.ChangeDevice(i));
            bufferSizes.onValueChanged.AddListener(i => AudioManager.Instance.ChangeSize(i));
        }

        public void SwitchActive()
        {
            panel.SetActive(!panel.activeSelf);
        }
        
    }
}
