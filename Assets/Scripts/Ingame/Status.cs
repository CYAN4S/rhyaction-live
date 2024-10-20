using System;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class Status
    {
        [SerializeField] private int score;
        [SerializeField] private int combo;

        public int Score => score;
        public int Combo => combo;
        public int[,] JudgeCount => _judgeCount;

        public event Action<int> ScoreChanged;
        public event Action<int> ComboIncreased;

        // TODO
        private int[,] _judgeCount = new int[Enum.GetNames(typeof(Judgement)).Length, 2];


        public void AddScore(int add)
        {
            score += add;
            ScoreChanged?.Invoke(score);
        }

        public void AddCombo(int add)
        {
            combo += add;
            ComboIncreased?.Invoke(combo);
        }

        public void ResetCombo()
        {
            combo = 0;
        }
        
        public void AddJudge(Judgement judgement, bool isEarly)
        {
            // TODO
            _judgeCount[(int)judgement, isEarly ? 0 : 1]++;
        }
    }
}