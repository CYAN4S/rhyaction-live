using Core;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
        [SerializeField] private ReadonlyFloatChannelSO scrollSpeedSO;
        [SerializeField] private ReadonlyDoubleChannelSO currentBeatChannelSO;

        private RectTransform _rectTransform;
        private NoteData _noteData;
        public bool IsLongNote => _noteData is LongNoteData;

        private NoteTranslator _nt;
        private NoteAppearance _na;

        public double Time { get; private set; }
        public bool IsProgress => _nt._state == LongNoteState.Progress;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void InstanceInitialize(NoteData noteData, double time, NoteType noteType)
        {
            _noteData = noteData;
            Time = time;

            _nt = new NoteTranslator(_noteData, _rectTransform, noteType);
        }

        public void OnProgress()
        {
            _nt.SetLongNoteState(LongNoteState.Progress);
        }

        private void Update()
        {
            _nt?.Update(currentBeatChannelSO.Value, scrollSpeedSO.Value);
        }
    }

    public enum NoteType
    {
        Normal,
        Long
    }

    public enum LongNoteState
    {
        Idle, // -> Progress, Missed
        Progress, // -> Cut, Missed, DEACTIVATE
        Missed, // -> DEACTIVATE
        Cut // -> Progress, Missed
    }

    public class NoteAppearance
    {
        private GameObject _o;
        private readonly NoteType _type;

        public UnityEvent OnDeactivate;

        public void Deactivate()
        {
        }
    }
}