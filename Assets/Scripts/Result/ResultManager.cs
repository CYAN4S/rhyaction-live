using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYAN4S
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI judgesText;
        

        private void Awake()
        {
            scoreText.text = $"{Result.Instance.score}";
            judgesText.text = $"{DebugOnly()}";
        }

        public void GoToSelect()
        {
            SceneManager.LoadScene("Select");
        }

        private string DebugOnly()
        {
            var x = Result.Instance.judgeCount;
            return $"{x[0, 0] + x[0, 1]}\n" +
                   $"{x[1, 0] + x[1, 1]}\n" +
                   $"{x[2, 0] + x[2, 1]}\n" +
                   $"{x[3, 0] + x[3, 1]}\n" +
                   $"{x[4, 0] + x[4, 1]}";
        }
    }
}
