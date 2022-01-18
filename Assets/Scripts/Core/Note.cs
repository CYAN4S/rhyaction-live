using System;

namespace Core
{
    [Serializable]
    public class NoteData
    {
        public Fraction beat;
        public int line;
        public string audioPath;

        public NoteData(Fraction beat, int line, string audioPath)
        {
            this.beat = beat;
            this.line = line;
            this.audioPath = audioPath;
        }
    }

    [Serializable]
    public class LongNoteData : NoteData
    {
        public Fraction length;

        public LongNoteData(Fraction beat, int line, string audioPath, Fraction length) : base(beat, line, audioPath)
        {
            this.length = length;
        }
    }
}