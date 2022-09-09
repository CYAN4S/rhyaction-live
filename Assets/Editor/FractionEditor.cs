using Core;
using UnityEditor;

namespace CYAN4S.Editor
{
    [CustomEditor(typeof(Fraction))]
    public class FractionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}