using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class NoteManager : MonoBehaviour
    {
        [Header("Visual")]
        public NoteScriptableObject noteset;

        [Header("Note Prefab")]
        [SerializeField] private NoteSystem[] notePrefabs;
        [SerializeField] private LongNoteSystem[] longNotePrefabs;

        [Header("Divider Prefab")]
        [SerializeField] private Divider dividerPrefab;

        [Header("Parent Transform")] 
        [SerializeField] private RectTransform[] notes;
        [SerializeField] private RectTransform dividers;
        
        private readonly List<Queue<NoteSystem>> _noteQueue = new();

        private Chart _chart;

        public List<Queue<NoteSystem>> Initialize(Chart chart, Gear gear)
        {
            _chart = chart;
            notes = gear.noteTransforms;
            dividers = gear.dividersTransform;

            (notePrefabs, longNotePrefabs) = chart.button switch 
            {
                4 => (noteset.prefab4B, noteset.prefabLong4B),
                5 => (noteset.prefab5B, noteset.prefabLong5B),
                6 => (noteset.prefab6B, noteset.prefabLong6B),
                8 => (noteset.prefab8B, noteset.prefabLong8B),
                _ => throw new Exception("Error here!")
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