using TMPro;
using UnityEngine;

namespace CYAN4S
{
    public class ComboDisplay : MonoBehaviour
    {
        [SerializeField] private Player player;
        
        private TextMeshProUGUI _text;
        private Animator _animator;
        
        private static readonly int ComboAnimaID = Animator.StringToHash("Combo");

        private void Awake()
        {
            player ??= GameObject.FindWithTag("Player").GetComponent<Player>();
            
            _text = GetComponent<TextMeshProUGUI>();
            _animator = GetComponent<Animator>();
        }

        public void ComboChanged(int combo)
        {
            _text.text = $"{combo}";
            _animator.SetTrigger(ComboAnimaID);
        }
    }
}
