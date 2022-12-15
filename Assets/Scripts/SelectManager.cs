using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CYAN4S
{
    public class SelectManager : MonoBehaviour
    {
        [SerializeField] private Transform tracksPanel;
        [SerializeField] private TrackButton trackButtonPrefab;

        private void Awake()
        {
            var charts = ExploreCharts();

            foreach (var chart in charts)
            {
                MakeTrackElement(chart);
            }
        }

        private TrackButton MakeTrackElement(string path)
        {
            var result = Instantiate(trackButtonPrefab, tracksPanel);
            result.text.text = path;
            result.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnSelect(path);
            });
            return result;
        }

        private void OnSelect(string path)
        {
            Debug.Log(path);
            Selected.Instance.chart = JsonUtility.FromJson<Chart>(File.ReadAllText(path));
            SceneManager.LoadScene("Ingame");
        }
        
        private IEnumerable<string> ExploreCharts()
        {
            var path = Path.Combine(Application.dataPath, "Tracks");

            Debug.Log(path);

            if (!Directory.Exists(path))
            {
                Debug.Log("Make a directory named: \\Tracks");
                return null;
            }

            var files = Directory.EnumerateFiles(path, "*.rlc");
            
            return files;
        }
    }
}
