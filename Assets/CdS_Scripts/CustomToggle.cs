using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CustomToggle : MonoBehaviour {
    [SerializeField, HideInInspector]
    Toggle customToggle;
    [SerializeField, HideInInspector]
    public UnityEvent<bool> onValueChanged = new();
    public bool Value {
        get => customToggle.isOn;
        set => customToggle.isOn = value;
    }
    bool _interactable = true;

    public bool Interactable {
        get => _interactable;
        set {
            if (!customToggle) return;
            _interactable = value;
            customToggle.interactable = value;
        }
    }

    void Awake() {
        customToggle = GetComponent<Toggle>();
        onValueChanged = customToggle.onValueChanged;
    }
}