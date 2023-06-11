using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ComboDisplay : MonoBehaviour
    {
        [SerializeField] private Player player;
        
        public TextMeshProUGUI _text;
        public Animator[] _animator;
        
        private static readonly int ComboAnimaID = Animator.StringToHash("Combo");

        private void Awake()
        {
            player ??= GameObject.FindWithTag("Player").GetComponent<Player>();
            player.comboIncreased.AddListener(ComboChanged);
        }

        public void ComboChanged(int combo)
        {
            _text.text = $"{combo}";

            foreach (var item in _animator)
            {
                item.SetTrigger(ComboAnimaID);
            }
        }

        private void OnDestroy()
        {
            player.comboIncreased.RemoveListener(ComboChanged);
        }
    }
}
