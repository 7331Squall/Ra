using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour {
    [SerializeField, HideInInspector]
    Slider customSlider;
    [SerializeField, HideInInspector]
    TMP_Text customSliderLabel;

    [SerializeField]
    public List<string> labels;
    [SerializeField, HideInInspector]
    public UnityEvent<float> onValueChanged = new();
    public float Value {
        get => customSlider.value;
        set => UpdateConfig(value);
    }
    bool _interactable = true;

    public bool Interactable {
        get => _interactable;
        set {
            _interactable = value;
            customSlider.interactable = value;
        }
    }

    void Awake() {
        if (labels == null || labels.Count == 0) {
            Debug.LogError("Labels are empty");
            // ReSharper disable once NotResolvedInText
            throw new ArgumentNullException("labels", "Labels are empty");
        }
        customSlider = GetComponentInChildren<Slider>();
        customSliderLabel = GetComponentInChildren<TMP_Text>();

        customSlider.maxValue = labels.Count - 1;
        customSlider.wholeNumbers = true;
        customSlider.onValueChanged.AddListener(UpdateConfig);
        UpdateConfig(labels.Count - 1);
    }

    void UpdateConfig(float value) {
        customSlider.value = value;
        customSliderLabel.text = labels[(int) value];
        onValueChanged?.Invoke(value);
    }
}