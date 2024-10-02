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
        [SerializeField] private TextMeshProUGUI noTracksWarningText;
        [SerializeField] private TextMeshProUGUI selectedTrackTitleText;
        [SerializeField] private TextMeshProUGUI selectedTrackButtonText;
        [SerializeField] private TextMeshProUGUI selectedTrackLevelText;
        [SerializeField] private TextMeshProUGUI selectedTrackBPMText;
        [SerializeField] private TextMeshProUGUI selectedTrackNoteCountText;
        [SerializeField] private TextMeshProUGUI selectedTrackLongNoteCountText;


        private void Awake()
        {
            ExploreCharts();
        }

        private TrackButton MakeTrackElement(string path)
        {
            var text = File.ReadAllText(path);
            var target = new ChartFactoryRLC().ToChart(text);
            target.rootPath = path;
            
            var result = Instantiate(trackButtonPrefab, tracksPanel);
            var info = $"{target.title} / {target.button}B / LV {target.level}";
            result.text.text = info;
            result.GetComponent<Button>().onClick.AddListener(() => { OnSelect(path); });
            return result;
        }

        private void OnSelect(string path)
        {
            var text = File.ReadAllText(path);
            Selected.Instance.chart = new ChartFactoryRLC().ToChart(text);
            Selected.Instance.chart.rootPath = Path.GetDirectoryName(path);
            selectedTrackTitleText.text = Selected.Instance.chart.title;
            selectedTrackButtonText.text = $"{Selected.Instance.chart.button}B";
            selectedTrackLevelText.text = $"LEVEL {Selected.Instance.chart.level}";
            selectedTrackBPMText.text = $"{Selected.Instance.chart.bpm} BPM";
            selectedTrackNoteCountText.text = $"{Selected.Instance.chart.notes.Count}";
            selectedTrackLongNoteCountText.text = $"{Selected.Instance.chart.longNotes.Count}";
        }

        private void ExploreCharts()
        {
            var path = Path.Combine(Application.dataPath, "Tracks");
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                noTracksWarningText.text = $"채보가 발견되지 않았습니다.\n하단 경로로 폴더가 생성되었습니다.\n해당 위치에 채보 파일을 넣으세요.\n{path}";
                noTracksWarningText.gameObject.SetActive(true);
                Selected.Instance.chart = null;
                return;
            }
            
            var files = Directory.EnumerateFiles(path, "*.rlc").ToList();
            
            files.AddRange(
                Directory
                .EnumerateDirectories(path)
                .SelectMany(subpath => Directory.EnumerateFiles(subpath, "*.rlc"))
            );

            if (files.Count == 0)
            {
                noTracksWarningText.text = $"채보가 발견되지 않았습니다.\n하단 경로에 채보 파일을 넣으세요.\n{path}";
                noTracksWarningText.gameObject.SetActive(true);
                Selected.Instance.chart = null;
                return;
            }
            
            noTracksWarningText.gameObject.SetActive(false);

            for (var i = 0; i < files.Count; i++)
            {
                var target = MakeTrackElement(files[i]);
                // target.GetComponent<RectTransform>().localPosition = new Vector3(0, -84 * i);
            }

            Selected.Instance.chart = null;
        }

        public void Play()
        {
            if (Selected.Instance.chart == null) return;
            SceneManager.LoadScene("Ingame");
        }

        public void GoToTitle()
        {
            SceneManager.LoadScene("Title");
        }
    }
}