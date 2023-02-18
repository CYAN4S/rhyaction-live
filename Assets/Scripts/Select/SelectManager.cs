using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ExploreCharts();
        }

        private TrackButton MakeTrackElement(string path)
        {
            var text = File.ReadAllText(path);
            var target = new ChartFactoryRLC().GetChart(text);
            
            var result = Instantiate(trackButtonPrefab, tracksPanel);
            result.text.text = target.title;
            result.GetComponent<Button>().onClick.AddListener(() => { OnSelect(path); });
            return result;
        }

        private void OnSelect(string path)
        {
            var text = File.ReadAllText(path);
            Selected.Instance.chart = new ChartFactoryRLC().GetChart(text);
            SceneManager.LoadScene("Ingame");
        }

        private void ExploreCharts()
        {
            var path = Path.Combine(Application.dataPath, "Tracks");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log("Make a directory named: \\Tracks");
                return;
            }
            
            var files = Directory.EnumerateFiles(path, "*.rlc").ToList();

            for (var i = 0; i < files.Count; i++)
            {
                var target = MakeTrackElement(files[i]);
                target.GetComponent<RectTransform>().localPosition = new Vector3(0, -100 * i);
            }
        }
    }
}