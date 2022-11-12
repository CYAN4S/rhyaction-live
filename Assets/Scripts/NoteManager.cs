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

        [Header("Transform")] 
        [SerializeField] private RectTransform notesParent;

        [Header("Transforms")] 
        [SerializeField] private RectTransform[] notes4B;
        
        private readonly List<Queue<NoteSystem>> _noteQueue = new();

        private Chart _chart;

        public List<Queue<NoteSystem>> Initialize(Chart chart)
        {
            _chart = chart;
            
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
                var system = Instantiate(notePrefab, notesParent);
                var time = note.beat * 60d / (double) bpm;

                system.InstanceInitialize(note, time, beat);
                noteTemp[note.line].Add(system);
            }

            foreach (var note in longNoteDataList)
            {
                var system = Instantiate(longNotePrefab, notesParent);
                var start = note.beat * 60d / (double) bpm;
                var end = (note.beat + note.length) * 60d / (double) bpm;

                system.InstanceInitialize(note, start, end, beat);
                noteTemp[note.line].Add(system);
            }

            for (var i = 0; i < noteTemp.Count; i++)
            {
                var temp = noteTemp[i];
                temp.Sort((a, b) => a.Time.CompareTo(b.Time));

                _noteQueue[i] = new Queue<NoteSystem>(temp);
            }

            return _noteQueue;
        }
    }
}