using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class NoteManager : MonoBehaviour
    {
        [Header("Note Prefab")]
        [SerializeField] private NoteSystem[] notePrefabs4B;
        [SerializeField] private LongNoteSystem[] longNotePrefabs4B;
        [SerializeField] private NoteSystem[] notePrefabs6B;
        [SerializeField] private LongNoteSystem[] longNotePrefabs6B;

        [Header("Divider Prefab")]
        [SerializeField] private Divider dividerPrefab;

        [Header("Parent Transform")] 
        [SerializeField] private RectTransform[] notes4B;
        [SerializeField] private RectTransform[] notes6B;
        [SerializeField] private RectTransform dividers;
        
        private readonly List<Queue<NoteSystem>> _noteQueue = new();

        private Chart _chart;

        public List<Queue<NoteSystem>> Initialize(Chart chart)
        {
            _chart = chart;

            var notePrefabs = chart.button switch
            {
                4 => notePrefabs4B,
                6 => notePrefabs6B,
                _ => throw new Exception($"{chart.button}B is not supported.")
            };
            
            var longNotePrefabs = chart.button switch
            {
                4 => longNotePrefabs4B,
                6 => longNotePrefabs6B,
                _ => throw new Exception($"{chart.button}B is not supported.")
            };
            
            var notes = chart.button switch
            {
                4 => notes4B,
                6 => notes6B,
                _ => throw new Exception($"{chart.button}B is not supported.")
            };

            // Set shared data
            NoteSystem.getBeat = Player.getBeat;
            NoteSystem.getScrollSpeed = Player.getScrollSpeed;
            Divider.getCurrentBeat = Player.getBeat;
            Divider.getScrollSpeed = Player.getScrollSpeed;
            
            // Get essential info from chart.
            var bpm = _chart.bpm;
            var buttonCount = _chart.button;
            var noteDataList = _chart.notes;
            var longNoteDataList = _chart.longNotes;

            var noteTemp = new List<List<NoteSystem>>();

            for (var i = 0; i < buttonCount; i++)
            {
                noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in noteDataList)
            {
                var targetPrefab =  notePrefabs[note.line];
                var system = Instantiate(targetPrefab, notes[note.line]);
                var time = note.beat * 60d / bpm;

                system.InstanceInitialize(note, time);
                noteTemp[note.line].Add(system);
            }

            foreach (var note in longNoteDataList)
            {
                var targetPrefab =  longNotePrefabs[note.line];
                var system = Instantiate(targetPrefab, notes[note.line]);
                var start = note.beat * 60d / bpm;
                var end = (note.beat + note.length) * 60d / bpm;

                system.InstanceInitialize(note, start, end);
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
                divider.InstanceInitialize(i);
            }

            return _noteQueue;
        }
    }
}