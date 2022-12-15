using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using CYAN4S;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ChartEditor : EditorWindow
{
    private VisualElement _root;
    private VisualTreeAsset _noteTree;

    private TextField _titleText;
    private DropdownField _buttonDropdown;
    private SliderInt _levelSlider;
    private TextField _bpmText;

    private GroupBox _notes;
    private Button _addNote;

    private TextField _fileName;
    private Button _logResult;
    private Button _logExample;
    private Button _save;

    private Chart _chart;

    [MenuItem("Window/UI Toolkit/Chart Editor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<ChartEditor>();
        wnd.titleContent = new GUIContent("Chart Editor");
    }

    public void CreateGUI()
    {
        _root = rootVisualElement;
        _chart = new Chart
        {
            notes = new List<NoteData>(),
            longNotes = new List<LongNoteData>()
        };

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ChartEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        _root.Add(labelFromUXML);

        _noteTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Note.uxml");

        // Get Elements
        _titleText = _root.Q<TextField>("Title");
        _buttonDropdown = _root.Q<DropdownField>("Button");
        _levelSlider = _root.Q<SliderInt>("Level");
        _bpmText = _root.Q<TextField>("BPM");

        _notes = _root.Q<GroupBox>("Notes");
        _addNote = _root.Q<Button>("AddNote");

        _fileName = _root.Q<TextField>("FileName");
        _logExample = _root.Q<Button>("LogExample");
        _logResult = _root.Q<Button>("LogResult");
        _save = _root.Q<Button>("Save");

        // Event bindings
        _addNote.clickable.clicked += AddNote;

        _save.clickable.clicked += Save;
        _logExample.clickable.clicked += LogExample;
        _logResult.clickable.clicked += LogResult;
    }

    public void AddNote()
    {
        VisualElement noteElement = _noteTree.Instantiate();

        var remove = noteElement.Q<Button>("Remove");
        remove.clickable.clicked += () => { _notes.Remove(noteElement); };

        _notes.Add(noteElement);
    }

    private void CalcChart()
    {
        var trackTitle = _titleText.value;
        var button = _buttonDropdown.index switch
        {
            0 => 4, 1 => 5, 2 => 6, 3 => 8, _ => throw new Exception("Button Dropdown Error!")
        };

        var bpm = Convert.ToSingle(_bpmText.value);

        _chart.title = trackTitle;
        _chart.button = button;
        _chart.bpm = bpm;

        _chart.notes = _notes.Children().Select(tree =>
        {
            var line = tree.Q<DropdownField>("Line").index;
            var num = tree.Q<TextField>("Num").value;
            var den = tree.Q<TextField>("Den").value;

            var numV = Convert.ToInt32(num);
            var denV = Convert.ToInt32(den);

            return new NoteData(new Fraction(numV, denV), line, "");
        }).ToList();
    }

    public void Save()
    {
        CalcChart();
    }

    private void LogResult()
    {
        CalcChart();
        Debug.Log(JsonUtility.ToJson(_chart));
    }

    private void LogExample()
    {
        Debug.Log(JsonUtility.ToJson(Chart.GetTestChart()));
    }
}