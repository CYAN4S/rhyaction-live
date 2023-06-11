using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Skins/Gear")]
    public class GearScriptableObject : ScriptableObject
    {
        public string gearName;

        public Gear prefab4B;
        public Gear prefab5B;
        public Gear prefab6B;
        public Gear prefab8B;
    }
}
