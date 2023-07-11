using UnityEngine;

namespace CYAN4S
{
    public class BombDisplay : MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        
        private static readonly int Triggered = Animator.StringToHash("Triggered");
        private Player player;

        private void Awake()
        {
            player = FindObjectOfType<Player>();
            player.judged.AddListener(OnJudged);
        }

        private void OnDestroy()
        {
            player.judged.RemoveListener(OnJudged);
        }

        public void OnJudged(Judgement j, bool _, int line)
        {
            if (j is Judgement.Precise or Judgement.Great or Judgement.Fair)
            {
                animators[line].SetTrigger(Triggered);
            }
        }
    }
}
