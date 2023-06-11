using Core;
using UnityEngine;

namespace CYAN4S
{
    public enum Situation
    {
        Normal, Debug
    }
    
    public class Selected : Singleton<Selected>
    {
        public Situation situation;
        
        [Header("Debugging")]
        public Chart chart;

    }
}
