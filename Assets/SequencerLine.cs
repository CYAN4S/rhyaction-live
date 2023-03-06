using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYAN4S
{
    public class SequencerLine : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        public int lineNumber;
        public GameObject notePreviewPrefab;
        public GameObject notePreview;

        public SequencerNote notePrefab;
        public List<SequencerNote> notes;

        private RectTransform _canvas;
        private RectTransform _notePreview;

        private void Awake()
        {
            notePreview = Instantiate(notePreviewPrefab, transform);
            notePreview.SetActive(false);
            _notePreview = notePreview.GetComponent<RectTransform>();

            _canvas = FindObjectOfType<Canvas>().gameObject.GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            notePreview.SetActive(true);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, eventData.position,
                    eventData.pressEventCamera, out var localCursor))
            {
                _notePreview.localPosition = new Vector3(0, localCursor.y);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, eventData.position,
                    eventData.pressEventCamera, out var localCursor)) return;
            
            var target = Instantiate(notePrefab, transform);
            target.GetComponent<RectTransform>().localPosition = new Vector3(0, localCursor.y);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            notePreview.SetActive(false);
        }
    }
}