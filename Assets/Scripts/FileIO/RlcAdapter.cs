using System;
using System.Collections.Generic;
using Core;

namespace CYAN4S
{
    [Serializable]
    public class RlcAdapter
    {
        public string title;
        public int button;
        public int level;
        public float bpm;
        public string audio;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;
        
        public Chart GetChart()
        {
            return new Chart
            {
                title = title,
                button = button,
                level = level,
                bpm = bpm,
                audio = audio,
                notes = notes,
                longNotes = longNotes
            };
        }
    }
}