using UnityEngine;
using UnityEngine.UI;

namespace CYAN4S
{
    /// <summary>
    /// DEVELOPMENT ONLY
    /// </summary>
    public class UIDebug : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;

        public void OnScoreChange(int value)
        {
            scoreText.text = $"{value}";
        }

        public void OnComboIncreased(int value)
        {
            comboText.text = $"{value}";
        }
    }
}