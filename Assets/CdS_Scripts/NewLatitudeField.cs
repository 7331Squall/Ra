using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewLatitudeField : MonoBehaviour
{
    public TMP_InputField latitudeNumberField;
    public Button latitudeButton;
    public TMP_Text latitudeButtonText;
    float _actualValue;
    bool _isUpdating, _interactable = true;
    public UnityEvent<float> OnValueChanged { get; set; } = new();
    public bool Interactable {
        get => _interactable;
        set {
            _interactable = value;
            latitudeNumberField.interactable = value;
            latitudeButton.interactable = value;
        }
    }
    public float Value {
        get => _actualValue;
        set {
            if (_isUpdating || Mathf.Approximately(_actualValue, value)) return;
            _isUpdating = true;
            _actualValue = Mathf.Clamp(Mathf.Abs(value) * Mathf.Sign(value), -90, 90);
            latitudeNumberField.text = Mathf.Abs(_actualValue).ToString("F1");
            latitudeButtonText.text = _actualValue < 0 ? "S" : "N";
            OnValueChanged.Invoke(value);
            _isUpdating = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        latitudeNumberField.onEndEdit.AddListener(value => Value = Mathf.Abs(float.Parse(value)) * Mathf.Sign(Value));
        latitudeButton.onClick.AddListener(() => Value *= -1);
    }
}