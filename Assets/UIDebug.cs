using UnityEngine;
using UnityEngine.UI;

namespace CYAN4S
{
    /// <summary>
    /// DEVELOPMENT ONLY
    /// </summary>
    public class UIDebug : MonoBehaviour
    {
        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        public void ChangeText(int value)
        {
            _text.text = $"{value}";
        }
    }
}