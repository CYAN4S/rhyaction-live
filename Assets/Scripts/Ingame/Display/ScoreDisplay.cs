using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ScoreDisplay : MonoBehaviour
    {
        private Player player;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void ScoreChanged(int score)
        {
            _text.text = $"{score}";
        }
    }
}
