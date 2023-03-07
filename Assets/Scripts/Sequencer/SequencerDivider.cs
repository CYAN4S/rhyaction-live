using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class SequencerDivider : MonoBehaviour
    {
        public float beat;

        private RectTransform rt;

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
            rt.localPosition = new Vector3(0, Sequencer.Instance.BeatToYPos(beat));
        }
    }
}
