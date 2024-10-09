using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using FMOD;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace CYAN4S
{
    public class Sequencer : MonoBehaviour, IScrollHandler, IPointerMoveHandler
    {
        [Header("Debug")] 
        public Chart chart;
        public bool isPlaying = false;        
        public float scale = 2;
        public float currentBeat = 0;
        public float currentTime = 0;
        public List<SequencerLine> lines = new();
        public List<SequencerDivider> dividers;
        
        [Header("Preferences Debug")] 
        public int snap;

        [Header("UIs")] 
        public RectTransform canvas;
        public TextMeshProUGUI currentBeatText;
        public TextMeshProUGUI currentTimeText;
        public TextMeshProUGUI scaleText;
        public TextMeshProUGUI cursorBeatText;
        public TMP_InputField audioPathInputField;
        public TMP_InputField bpmInputField;
        public TMP_Dropdown buttonModeDropdown;
        public TMP_InputField chartPathInputField;
        public TMP_InputField exportChartPathInputField;
        
        [Header("Preferences UIs")] 
        public TMP_InputField snapInputField;

        [Header("Parent Transforms")]
        public Transform dividersTransform;
        public Transform linesTransform;

        [Header("Prefabs")]
        public SequencerLine linePrefab;
        public SequencerDivider dividerPrefab;

        // Actions
        public event Action<float> OnCurrentBeatChange;
        public event Action<float> OnCurrentTimeChange;
        public event Action<float> OnScaleChange;

        public static Sequencer Instance { get; private set; }
        
        // FMOD
        public Sound sound;
        public Channel channel;

        private void Awake()
        {
            chart = new Chart { button = 4, bpm = 120f };
            snap = 4;
            Instance = this;
            
            dividers = Enumerable.Range(0, 200).Select(i =>
            {
                var target = Instantiate(dividerPrefab, dividersTransform);
                target.beat = i;
                return target;
            }).ToList();

            OnCurrentBeatChange += ChangeCurrentBeatText;
            OnCurrentTimeChange += ChangeCurrentTimeText;

            OnCurrentBeatChange?.Invoke(currentBeat);
            OnScaleChange?.Invoke(scale);
            OnButtonModeChange(0);
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                OnPlayOrPause();
            }
            
            if (isPlaying)
            {
                currentTime += Time.deltaTime;
                currentBeat = currentTime / 60f * chart.bpm;
                OnCurrentBeatChange?.Invoke(currentBeat);
                OnCurrentTimeChange?.Invoke(currentTime);
            }
        }

        public void PlaceLines(int button)
        {
            foreach (var line in lines)
                Destroy(line.gameObject);

            lines = Enumerable.Range(0, button).Select(i =>
            {
                var target = Instantiate(linePrefab, linesTransform);
                target.transform.GetComponent<RectTransform>().localPosition = new Vector3(100 * i, 0);
                target.lineNumber = i;
                return target;
            })
                .ToList();
        }

        public void OnButtonModeChange(int i)
        {
            var b = i switch { 0 => 4, 1 => 5, 2 => 6, 3 => 8, _ => throw new Exception("huh?")};
            chart.button = b;
            PlaceLines(b);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Keyboard.current.leftCtrlKey.isPressed)
            {
                scale += eventData.scrollDelta.y / 60f;
                if (scale < 0.5f) scale = 0.5f;
                scaleText.text = $"{scale:F2}";
                OnScaleChange?.Invoke(scale);
                return;
            }

            if (isPlaying)
                OnPlayOrPause();

            currentBeat += eventData.scrollDelta.y / (60f * scale);
            if (currentBeat < 0) currentBeat = 0;
            currentTime = currentBeat * 60f / chart.bpm;
            
            OnCurrentBeatChange?.Invoke(currentBeat);
            OnCurrentTimeChange?.Invoke(currentTime);
        }

        public void ChangeCurrentBeatText(float beat)
        {
            currentBeatText.text = $"{beat:F2}";
        }
        
        public void ChangeCurrentTimeText(float time)
        {
            var t = TimeSpan.FromSeconds(time);
            currentTimeText.text = $"{t:mm\\:ss\\.fff}";
        }

        public float BeatToYPos(float beat)
        {
            return (beat - currentBeat) * 200f * scale;
        }

        public float YPosToBeat(float y)
        {
            return y / (200f * scale) + currentBeat;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position,
                    eventData.pressEventCamera, out var localCursor)) return;

            cursorBeatText.text = $"{YPosToBeat(localCursor.y):F2}";
        }

        public void OnExportLog()
        {
            Debug.Log(Export());
        }

        public void OnExport()
        {
            var path = Path.Combine(Application.dataPath, "Tracks");
            path = Path.Combine(path, exportChartPathInputField.text);
            
            File.WriteAllText(path, Export());
        }

        public string Export()
        {
            chart.notes = lines.Select((l, i) => l.notes.Keys
                    .ToList()
                    .Select(b =>
                        new NoteData(new Fraction((int)(b * 100), 100), i, "")
                    )
                )
                .SelectMany(x => x)
                .ToList();

            return new ChartFactoryRLC().FromChart(chart);
        }

        public void ImportChart()
        {
            var path = Path.Combine(Application.dataPath, "Tracks");
            path = Path.Combine(path, chartPathInputField.text);
            var text = File.ReadAllText(path);

            var c = new ChartFactoryRLC().ToChart(text);
            ImportChart(c);
        }

        public void ImportChart(Chart imported)
        {
            chart = imported;
            bpmInputField.text = $"{chart.bpm}";
            audioPathInputField.text = $"{chart.audio}";
            buttonModeDropdown.value = chart.button switch
            {
                4 => 0, 5 => 1, 6 => 2, 8 => 3, _ => throw new Exception("uh...")
            };
            
            PlaceLines(chart.button);
            chart.notes.ForEach(note =>
            {
                lines[note.line].CreateNote(note.beat);
            });
            
            OnAudioImport(chart.audio);
        }

        public void OnNumEdit(string input)
        {
            if (int.TryParse(input, out var result))
                snap = result;
            else
                snapInputField.text = $"{snap}";
        }

        public void OnBpmEdit(string input)
        {
            if (float.TryParse(input, out var result))
                chart.bpm = result;
            else
                bpmInputField.text = $"{chart.bpm}";
        }

        public void OnAudioImport()
        {
            chart.audio = audioPathInputField.text;
            OnAudioImport(audioPathInputField.text);
        }
        
        public void OnAudioImport(string path)
        {
            var x = AudioManager.PrepareSound(path, Path.Combine(Application.dataPath, "Tracks"));
            if (x == null) return;
            
            sound = (Sound)x;
            channel = AudioManager.PlaySound(sound, true);
        }

        public void OnPlayOrPause()
        {
            if (isPlaying)
            {
                channel.setPaused(true);
            }
            else
            {
                channel.setPosition((uint)(currentTime * 1000), TIMEUNIT.MS);
                channel.setPaused(false);
            }

            isPlaying = !isPlaying;
        }
    }
}