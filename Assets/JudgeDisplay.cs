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
        
        [SerializeField] private Sprite preciseSprite;
        [SerializeField] private Sprite greatEarlySprite;
        [SerializeField] private Sprite greatLateSprite;
        [SerializeField] private Sprite fairEarlySprite;
        [SerializeField] private Sprite fairLateSprite;
        [SerializeField] private Sprite poorSprite;
        [SerializeField] private Sprite breakSprite;

        private void Awake()
        {
            _image = GetComponent<Image>();
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
        }
    }
}
