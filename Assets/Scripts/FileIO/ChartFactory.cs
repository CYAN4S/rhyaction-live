using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public interface IChartFactory
    {
        public Chart GetChart(string text);
    }
    
    [Serializable]
    public class ChartFactoryRLC : IChartFactory
    {
        [Serializable]
        public class RLCData
        {
            public string title;
            public int button;
            public int level;
            public float bpm;
            public string audio;

            public List<NoteData> notes;
            public List<LongNoteData> longNotes;
        }
        
        public Chart GetChart(string text)
        {
            var result = JsonUtility.FromJson<RLCData>(text);
            
            return new Chart
            {
                title = result.title,
                button = result.button,
                level = result.level,
                bpm = result.bpm,
                audio = result.audio,
                notes = result.notes,
                longNotes = result.longNotes
            };
        }
    }
}