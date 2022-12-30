using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class NoteManager : MonoBehaviour
    {
        [Header("Note Prefab")] 
        [SerializeField] private NoteSystem notePrefab;
        [SerializeField] private LongNoteSystem longNotePrefab;
        [SerializeField] private NoteSystem notePrefabVariant;
        [SerializeField] private LongNoteSystem longNotePrefabVariant;
        [SerializeField] private Divider dividerPrefab;

        [Header("Transforms")] 
        [SerializeField] private RectTransform[] notes4B;
        [SerializeField] private RectTransform dividers;
        
        private readonly List<Queue<NoteSystem>> _noteQueue = new();

        private Chart _chart;

        public List<Queue<NoteSystem>> Initialize(Chart chart)
        {
            _chart = chart;

            NoteSystem.getScrollSpeed = Player.getScrollSpeed;
            Divider.getScrollSpeed = Player.getScrollSpeed;
            
            // Get essential info from chart.
            var bpm = _chart.bpm;
            var buttonCount = _chart.button;
            var noteDataList = _chart.notes;
            var longNoteDataList = _chart.longNotes;

            var beat = Player.getBeat;

            var noteTemp = new List<List<NoteSystem>>();

            for (var i = 0; i < buttonCount; i++)
            {
                noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in noteDataList)
            {
                var targetPrefab = note.line is 1 or 2 ? notePrefabVariant : notePrefab;
                var system = Instantiate(targetPrefab, notes4B[note.line]);
                var time = note.beat * 60d / bpm;

                system.InstanceInitialize(note, time, beat);
                noteTemp[note.line].Add(system);
            }

            foreach (var note in longNoteDataList)
            {
                var targetPrefab = note.line is 1 or 2 ? longNotePrefabVariant : longNotePrefab;
                var system = Instantiate(targetPrefab, notes4B[note.line]);
                var start = note.beat * 60d / bpm;
                var end = (note.beat + note.length) * 60d / bpm;

                system.InstanceInitialize(note, start, end, beat);
                noteTemp[note.line].Add(system);
            }

            for (var i = 0; i < noteTemp.Count; i++)
            {
                var temp = noteTemp[i];
                temp.Sort((a, b) => a.Time.CompareTo(b.Time));

                _noteQueue[i] = new Queue<NoteSystem>(temp);
            }

            var endDivider = Math.Ceiling(chart.GetEndBeat());
            for (var i = 0; i <= endDivider; i++)
            {
                var divider = Instantiate(dividerPrefab, dividers);
                divider.InstanceInitialize(i, beat);
            }

            return _noteQueue;
        }
    }
}