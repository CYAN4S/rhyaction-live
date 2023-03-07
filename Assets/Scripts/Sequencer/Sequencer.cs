using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace CYAN4S
{
    public class Sequencer : MonoBehaviour, IScrollHandler, IPointerMoveHandler
    {
        [Header("Debug")] 
        public Chart chart = new() { button = 4, bpm = 120f };
        public float scale = 2;
        public float currentBeat;
        public List<SequencerLine> lines = new();
        public List<SequencerDivider> dividers;

        [Header("Preferences Debug")] 
        public int snapNumerator;

        [Header("UIs")] 
        public RectTransform canvas;
        public TextMeshProUGUI currentBeatText;
        public TextMeshProUGUI scaleText;
        public TextMeshProUGUI cursorBeatText;
        
        [Header("Preferences UIs")] 
        public TMP_InputField snapNumeratorInputField;

        [Header("Parent Transforms")]
        public Transform dividersTransform;
        public Transform linesTransform;

        [Header("Prefabs")]
        public SequencerLine linePrefab;
        public SequencerDivider dividerPrefab;

        public event Action<float> OnCurrentBeatChange;
        public event Action<float> OnScaleChange;

        public static Sequencer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            dividers = Enumerable.Range(0, 20).Select(i =>
            {
                var target = Instantiate(dividerPrefab, dividersTransform);
                target.beat = i;
                return target;
            }).ToList();

            OnCurrentBeatChange?.Invoke(currentBeat);
            OnScaleChange?.Invoke(scale);
            OnButtonModeChange(chart.button);
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

            currentBeat += eventData.scrollDelta.y / (60f * scale);
            if (currentBeat < 0) currentBeat = 0;
            currentBeatText.text = $"{currentBeat:F2}";
            OnCurrentBeatChange?.Invoke(currentBeat);
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

        public void OnExport()
        {
            chart.notes = lines.Select((l, i) => l.notes.Keys
                    .ToList()
                    .Select(b =>
                        new NoteData(new Fraction((int)(b * 100), 100), i, "")
                    )
                )
                .SelectMany(x => x)
                .ToList();

            Debug.Log(new ChartFactoryRLC().FromChart(chart));
        }

        public void OnNumEdit(string input)
        {
            if (int.TryParse(input, out var result))
            {
                snapNumerator = result;
            }
            else
            {
                snapNumeratorInputField.text = $"{snapNumerator}";
            }
        }
    }
}