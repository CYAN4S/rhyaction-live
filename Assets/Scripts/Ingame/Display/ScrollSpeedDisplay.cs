using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ScrollSpeedDisplay : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void OnSpeedChanged(int speed)
        {
            _text.text = $"{speed / 10}.{speed % 10}";
        }
    }
}
