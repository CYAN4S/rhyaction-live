using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYAN4S
{
    public class JudgeDisplay : MonoBehaviour
    {
        private Image _image;
        private Animator _animator;

        [SerializeField] private Sprite preciseSprite;
        [SerializeField] private Sprite greatEarlySprite;
        [SerializeField] private Sprite greatLateSprite;
        [SerializeField] private Sprite fairEarlySprite;
        [SerializeField] private Sprite fairLateSprite;
        [SerializeField] private Sprite poorSprite;
        [SerializeField] private Sprite breakSprite;

        private static readonly int PreciseAnim = Animator.StringToHash("Precise");
        private static readonly int GreatAnim = Animator.StringToHash("Great");
        private static readonly int FairAnim = Animator.StringToHash("Fair");

        private void Awake()
        {
            // Get Components
            _image = GetComponent<Image>();
            _animator = GetComponent<Animator>();
        }

        public void OnJudge(Judgement judge, bool isEarly)
        {
            _image.sprite = (judge, isEarly) switch
            {
                (Judgement.Precise, _) => preciseSprite,
                (Judgement.Great, true) => greatEarlySprite,
                (Judgement.Great, false) => greatLateSprite,
                (Judgement.Fair, true) => fairEarlySprite,
                (Judgement.Fair, false) => fairLateSprite,
                (Judgement.Poor, _) => poorSprite,
                (Judgement.Break, _) => breakSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _animator.SetTrigger(judge switch
            {
                Judgement.Precise => PreciseAnim,
                Judgement.Great => GreatAnim,
                _ => FairAnim
            });
        }
    }
}