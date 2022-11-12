using System;
using System.Collections.Generic;
using Core;
using CYAN4S;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ChartEditor : EditorWindow
{
    private VisualElement _root;
    
    private TextField _titleText;
    private DropdownField _buttonDropdown;
    private SliderInt _levelSlider;
    private TextField _bpmText;
    
    private Button _logResult;
    private Button _logExample;
    
    [MenuItem("Window/UI Toolkit/Chart Editor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<ChartEditor>();
        wnd.titleContent = new GUIContent("Chart Editor");
    }

    public void CreateGUI()
    {
        Debug.Log("Hi!");
        _root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ChartEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        _root.Add(labelFromUXML);
        
        // Get Elements
        _titleText = _root.Q<TextField>("Title");
        _buttonDropdown = _root.Q<DropdownField>("Button");
        _levelSlider = _root.Q<SliderInt>("Level");
        _bpmText = _root.Q<TextField>("BPM");
        
        _logExample = _root.Q<Button>("LogExample");
        _logResult = _root.Q<Button>("LogResult");
        
        // Event bindings
        _logExample.clickable.clicked += LogExample;
        _logResult.clickable.clicked += LogResult;
    }

    private void OnDestroy()
    {
        _logExample.clickable.clicked -= LogExample;
        _logResult.clickable.clicked -= LogResult;
    }

    private void LogResult()
    {
        var trackTitle = _titleText.value;
        var button = _buttonDropdown.index switch
        {
            0 => 4, 1 => 5, 2 => 6, 3 => 8, _ => throw new Exception("Button Dropdown Error!")
        };

        var bpm = Convert.ToDecimal(_bpmText.value);
        
        var result = new Chart
        {
            title = trackTitle,
            notes = new List<NoteData>(),
            longNotes = new List<LongNoteData>(),
            button = button,
            bpm = bpm
        };
        
        Debug.Log(JsonUtility.ToJson(result));
    }

    private void LogExample()
    {
        Debug.Log(JsonUtility.ToJson(Chart.GetTestChart()));
    }
}