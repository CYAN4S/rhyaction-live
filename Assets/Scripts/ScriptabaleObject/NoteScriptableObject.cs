using UnityEngine;

namespace CYAN4S
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Skins/Note")]
    public class NoteScriptableObject : ScriptableObject
    {
        public string noteName;

        public NoteSystem[] prefab4B;
        public NoteSystem[] prefab5B;
        public NoteSystem[] prefab6B;
        public NoteSystem[] prefab8B;

        public LongNoteSystem[] prefabLong4B;
        public LongNoteSystem[] prefabLong5B;
        public LongNoteSystem[] prefabLong6B;
        public LongNoteSystem[] prefabLong8B;
    }
}
