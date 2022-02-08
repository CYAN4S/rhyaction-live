using System;
using Core;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    /// 노트의 역할
    /// - 실시간으로 움직인다
    ///     - 초기 고정: 노트의 라인 위치, 박자
    ///     - 외부 동적: 현재 경과 시간, 스크롤 속도
    /// - 실시간으로 크기가 변화한다
    ///     - 일반 노트의 경우 크기가 일정
    ///     - 롱 노트의 경우 상황에 따라 달라진다
    ///         - 초기 고정: 노트의 박자 길이
    ///         - 외부 동적 공통: 스크롤 속도
    ///             - 입력 전
    ///             - 입력 중: 현재 경과 시간
    /// - 입력에 따라 사라진다
    ///     - 일반 노트: 입력을 받아 처리되면 즉시
    ///     - 롱 노트: 떼는 판정까지 처리되면 즉시
    ///         - 처음부터 무시 -> 끝 시간 + 일정 텀 후에
    ///         - 너무 일찍 뗌 -> 끝 시간 + 일정 텀 후에
    ///         - 너무 늦게 뗌 -> 강제 종료 판정이 된 즉시
    /// 
    public class NoteSystem : MonoBehaviour
    {
        [SerializeField] private FloatSO scrollSpeedSO;
        [SerializeField] private DoubleSO currentBeatSO;

        private RectTransform _rectTransform;
        private NoteData _noteData;
        public bool IsLongNote => _noteData is LongNoteData;

        private NoteTranslator _nt;
        private NoteAppearance _na;

        public float Time { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void InstanceInitialize(NoteData noteData, float time)
        {
            _noteData = noteData;
            Time = time;

            _nt = new NoteTranslator(_noteData, _rectTransform,
                noteData is LongNoteData ? NoteType.Long : NoteType.Normal);
        }
        
        public void OnProgress()
        {
            _nt.SetLongNoteState(LongNoteState.Progress);
        }

        private void Update()
        {
            _nt?.Update(currentBeatSO.Value, scrollSpeedSO.Value);
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

    public class NoteTranslator
    {
        private readonly RectTransform _rt;
        private readonly NoteType _type;
        private readonly NoteData _d;

        private Action<double, float> _onUpdate;

        private float[] _xPos = {-150f, -50f, 50f, 150f};

        private LongNoteState _state;

        public NoteTranslator(NoteData noteData, RectTransform rt, NoteType noteType)
        {
            if (noteType == NoteType.Long && noteData is not LongNoteData)
            {
                throw new Exception("noteData can convert into LongNoteType");
            }

            _d = noteData;
            _rt = rt;
            _type = noteType;
            _state = LongNoteState.Idle;

            _onUpdate += noteType switch
            {
                NoteType.Normal => UpdateNormal,
                NoteType.Long => UpdateLong,
                _ => throw new ArgumentOutOfRangeException(nameof(noteType), noteType, null)
            };
        }

        public void SetLongNoteState(LongNoteState state)
        {
            if (_type != NoteType.Long)
                throw new Exception("State translation is for long note.");

            switch (state)
            {
                case LongNoteState.Idle:
                    throw new Exception("No defined state translation to Idle.");
                case LongNoteState.Progress:
                    if (_state == LongNoteState.Idle)
                    {
                        _onUpdate -= UpdateLong;
                        _onUpdate += UpdateActiveLong;
                    }

                    break;
                case LongNoteState.Missed:
                    break;
                case LongNoteState.Cut:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            _state = state;
        }

        private void UpdateNormal(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], (float) (_d.beat - currentBeat) * 100f * scrollSpeed);
        }

        private void UpdateLong(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], (float) (_d.beat - currentBeat) * 100f * scrollSpeed);
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, (float) ((LongNoteData) _d).length * 100f * scrollSpeed);
        }

        private void UpdateActiveLong(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], 0);
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x,
                Mathf.Max((float) (((LongNoteData) _d).length + _d.beat - currentBeat) * 100f * scrollSpeed, 0f));
        }

        public void Update(double currentBeat, float scrollSpeed)
        {
            _onUpdate?.Invoke(currentBeat, scrollSpeed);
        }
    }
}