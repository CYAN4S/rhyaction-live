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
            player = FindObjectOfType<Player>();
            player.scoreChanged.AddListener(ScoreChanged);
        }

        private void OnDestroy()
        {
            player.scoreChanged.RemoveListener(ScoreChanged);
        }

        public void ScoreChanged(int score)
        {
            _text.text = $"{score}";
        }
    }
}
