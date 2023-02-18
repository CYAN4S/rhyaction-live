using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CYAN4S
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI judgesText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private Image rankImage;
        [SerializeField] private Sprite[] rankImageSprites;

        private void Awake()
        {
            scoreText.text = $"{Result.Instance.score}";
            judgesText.text = $"{DebugOnly()}";
            accuracyText.text = $"{Result.Instance.accuracy:0.00}%";

            rankImage.sprite =
                Result.Instance.accuracy >= 95 ? rankImageSprites[0] :
                Result.Instance.accuracy >= 90 ? rankImageSprites[1] :
                Result.Instance.accuracy >= 80 ? rankImageSprites[2] :
                Result.Instance.accuracy >= 70 ? rankImageSprites[3] :
                Result.Instance.accuracy >= 50 ? rankImageSprites[4] :
                rankImageSprites[5];
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
