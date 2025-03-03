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
            if (input is null)
            {
                input = FindAnyObjectByType<InputHandler>();
            }
            input.onButtonPressedEx.AddListener(ButtonPressed);
            input.onButtonReleasedEx.AddListener(ButtonReleased);
        }

        private void OnDestroy()
        {
            input.onButtonPressedEx.RemoveListener(ButtonPressed);
            input.onButtonReleasedEx.RemoveListener(ButtonReleased);
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
