using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

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
        private static float[] _xPos;
        private static Func<Fraction, float> _yPos;
        private static Func<Fraction, float> _height;
        private static Func<Fraction, float> _heightIn;

        private RectTransform _rectTransform;

        private bool isLongInProgress;

        private NoteData _noteData;
        public bool IsLongNote => _noteData is LongNoteData;

        public void AlertInProgress() => isLongInProgress = true;

        public float Time { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            isLongInProgress = false;
        }

        public static void StaticInitialize(float[] xPos, Func<Fraction, float> yPos, Func<Fraction, float> height,
            Func<Fraction, float> heightIn)
        {
            _xPos = xPos;
            _yPos = yPos;
            _height = height;
            _heightIn = heightIn;
        }

        public void InstanceInitialize(NoteData noteData, float time)
        {
            _noteData = noteData;
            Time = time;
            _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], 0f);
        }

        private void Update()
        {
            if (!IsLongNote)
            {
                _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));
            }
            else if (!isLongInProgress)
            {
                _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));
                _rectTransform.sizeDelta =
                    new Vector2(_rectTransform.sizeDelta.x, _height(((LongNoteData) _noteData).length));
            }
            else
            {
                _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], 0);
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x,
                    _heightIn(((LongNoteData) _noteData).length + _noteData.beat));
            }
        }
    }
}