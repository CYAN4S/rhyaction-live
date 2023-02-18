using UnityEngine;

namespace CYAN4S
{
    [RequireComponent(typeof(Animator))]
    public class AnimateOnBeat : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        
    }
}
