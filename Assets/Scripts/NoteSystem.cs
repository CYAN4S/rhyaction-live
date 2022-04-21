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
        [SerializeField] private ReadonlyFloatChannelSO scrollSpeedSO;
        [SerializeField] private ReadonlyDoubleChannelSO currentBeatChannelSO;

        private RectTransform _rectTransform;
        private NoteData _noteData;
        public bool IsLongNote => _noteData is LongNoteData;

        private NoteTranslator _nt;
        private NoteAppearance _na;

        public float Time { get; private set; }
        public bool IsProgress => _nt._state == LongNoteState.Progress;

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