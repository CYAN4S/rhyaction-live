using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ScrollSpeedDisplay : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Player player;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            player = FindObjectOfType<Player>();
            player.speedChanged.AddListener(OnSpeedChanged);
        }
        
        private void OnDestroy()
        {
            player.speedChanged.RemoveListener(OnSpeedChanged);
        }

        public void OnSpeedChanged(int speed)
        {
            _text.text = $"{speed / 10}.{speed % 10}";
        }
    }
}
