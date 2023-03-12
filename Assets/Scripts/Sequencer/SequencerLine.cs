using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
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
            var beatPos = Sequencer.Instance.YPosToBeat(y);
            var numerator = Keyboard.current.altKey.isPressed ? 100 : Sequencer.Instance.snapNumerator;
            
            var a = beatPos * numerator;
            var denominator = (int)math.round(a);
            var fraction = new Fraction(numerator, denominator);

            CreateNote(fraction);
        }

        public SequencerNote CreateNote(Fraction beat)
        {
            var beatFloat = (float)beat;
            
            if (beatFloat < 0) return null;
            if (notes.ContainsKey(beatFloat)) return null;
            
            var target = Instantiate(notePrefab, transform);

            target.beatFraction = beat;
            target.beatFloat = beatFloat;
            target.GetComponent<RectTransform>().localPosition = new Vector3(0, Sequencer.Instance.BeatToYPos(beatFloat));

            notes.Add(beatFloat, target);
            target.onThisClick = () => notes.Remove(beatFloat);

            return target;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            notePreview.SetActive(false);
        }
    }
}