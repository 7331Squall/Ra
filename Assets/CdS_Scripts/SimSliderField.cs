using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimSliderField : MonoBehaviour {
    [SerializeField]
    // ReSharper disable once RedundantDefaultMemberInitializer
    bool isInterval = false;
    bool _enabled = true;
    bool _isUpdating;
    TMP_Text _label;
    Slider _slider;
    int _value;
    public UnityEvent<int> OnValueChanged { get; set; } = new();

    public bool Interactable {
        get => _enabled;
        set {
            _enabled = value;
            _slider.interactable = value;
        }
    }
    public int Value {
        get => _value;
        set {
            _value = value;
            _slider.value = _value;
            OnValueChanged.Invoke(_value);
            _slider.onValueChanged.Invoke(_value);
        }
    }

    void Awake() {
        _slider = GetComponentInChildren<Slider>();
        _label = GetComponentsInChildren<TMP_Text>().ToList().Find(x => x.name == "ValueLabel");
        _slider.maxValue = Enum.GetValues(isInterval ? typeof(IntervalSettings) : typeof(SpeedSettings)).Length - 1;
        _slider.onValueChanged.AddListener(ValueChanged);
        Value = isInterval ? (int) (IntervalSettings.Continuous) : (int) (SpeedSettings.OneHour);
        UpdateLabel();
    }

    void ValueChanged(float value) {
        if (_isUpdating || Mathf.Approximately(value, _value)) return;
        _isUpdating = true;
        Value = (int) value;
        UpdateLabel();
        _isUpdating = false;
    }

    void UpdateLabel() {
        _label.text = isInterval ? IntervalSetting.ToDisplayString((IntervalSettings) Value) : SpeedSetting.ToDisplayString((SpeedSettings) Value);
        //_label.faceColor = Value >= SpeedSettings.OneWeek ? new Color32(50, 0, 0, 255) : new Color32(10, 10, 10, 255);
    }
}