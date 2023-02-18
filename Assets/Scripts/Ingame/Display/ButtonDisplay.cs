using UnityEngine;

namespace CYAN4S
{
    public class ButtonDisplay : MonoBehaviour
    {
        public InputHandler input;
        public Animator[] animators;
        
        private static readonly int IsPressed = Animator.StringToHash("IsPressed");

        private void Awake()
        {
            input.onButtonPressed.AddListener(ButtonPressed);
            input.onButtonReleased.AddListener(ButtonReleased);
        }

        private void OnDestroy()
        {
            input.onButtonPressed.RemoveListener(ButtonPressed);
            input.onButtonReleased.RemoveListener(ButtonReleased);
        }

        public void ButtonPressed(int btn, double _)
        {
            animators[btn].SetBool(IsPressed, true);
        }

        public void ButtonReleased(int btn, double _)
        {
            animators[btn].SetBool(IsPressed, false);
        }
    }
}
