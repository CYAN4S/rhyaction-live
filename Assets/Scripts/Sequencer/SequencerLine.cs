using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CYAN4S
{
    public class SequencerLine : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        public int lineNumber;
        public GameObject notePreviewPrefab;
        public GameObject notePreview;

        public SequencerNote notePrefab;
        public Dictionary<float, SequencerNote> notes = new();

        public RectTransform canvas;
        public RectTransform _notePreview;

        private void Awake()
        {
            notePreview = Instantiate(notePreviewPrefab, transform);
            notePreview.SetActive(false);
            _notePreview = notePreview.GetComponent<RectTransform>();

            canvas = Sequencer.Instance.canvas;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            notePreview.SetActive(true);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position,
                    eventData.pressEventCamera, out var localCursor)) return;

            var beat = Sequencer.Instance.YPosToBeat(localCursor.y);

            if (Keyboard.current.altKey.isPressed)
            {
                _notePreview.localPosition = new Vector3(0, beat);
                return;
            }
            
            var a = beat * Sequencer.Instance.snapNumerator;
            var b = math.round(a) / Sequencer.Instance.snapNumerator;
            var snappedPos = Sequencer.Instance.BeatToYPos(b);
            
            _notePreview.localPosition = new Vector3(0, snappedPos);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position,
                    eventData.pressEventCamera, out var localCursor)) return;

            var y = localCursor.y;
            var beat = Sequencer.Instance.YPosToBeat(y);
            
            if (!Keyboard.current.altKey.isPressed)
            {
                var a = beat * Sequencer.Instance.snapNumerator;
                beat = math.round(a) / Sequencer.Instance.snapNumerator;
                y = Sequencer.Instance.BeatToYPos(beat);
            }
            
            if (beat < 0) return;
            if (notes.ContainsKey(beat)) return;
            
            var target = Instantiate(notePrefab, transform);
            
            target.beat = beat;
            target.GetComponent<RectTransform>().localPosition = new Vector3(0, y);

            notes.Add(beat, target);
            target.onThisClick = () => notes.Remove(beat);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            notePreview.SetActive(false);
        }
    }
}