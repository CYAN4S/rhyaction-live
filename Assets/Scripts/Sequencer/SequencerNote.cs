using System;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYAN4S
{
    public class SequencerNote : MonoBehaviour, IPointerClickHandler
    {
        public float beatFloat;
        public Fraction beatFraction;

        private RectTransform rt;

        public Action onThisClick;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            Sequencer.Instance.OnCurrentBeatChange += OnChange;
            Sequencer.Instance.OnScaleChange += OnChange;
        }

        private void OnDestroy()
        {
            Sequencer.Instance.OnCurrentBeatChange -= OnChange;
            Sequencer.Instance.OnScaleChange -= OnChange;
        }

        private void OnChange(float _)
        {
            rt.localPosition = new Vector3(0, Sequencer.Instance.BeatToYPos(beatFloat));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onThisClick();
            Destroy(gameObject);
        }
    }
}
