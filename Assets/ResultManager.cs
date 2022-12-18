using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYAN4S
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;

        private void Awake()
        {
            scoreText.text = $"{Result.Instance.score}";
        }

        public void GoToSelect()
        {
            SceneManager.LoadScene("Select");
        }
    }
}
