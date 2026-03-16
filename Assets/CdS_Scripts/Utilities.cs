using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public static class Utilities {
    public static List<string> PopulateList(int amount, int offset = 0) {
        List<string> options = new();
        for (int i = 0; i < amount; i++) { options.Add((i + offset).ToString("D2")); }
        return options;
    }

    // ✅ Verifica se qualquer TMP_Dropdown está expandido
    public static bool AnyDropdownOpen() {
        TMP_Dropdown[] dropdowns = Object.FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None); // FindObjectsOfType<TMP_Dropdown>();
        return dropdowns.Any(dd => dd.IsExpanded);
    }

    public static List<GameObject> GetObjectsUnderPointer() {
        if (EventSystem.current == null) {
            return new List<GameObject>();
        }
        int pointerId = PointerId.mousePointerId;
        PointerEventData pointerData = new(EventSystem.current) {
            pointerId = pointerId,
            position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero
        };
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Select(r => r.gameObject).ToList();
    }

    public static Vector2 GetCameraMovementValues(InputActionReference pointAction, Vector2 camSpeed) {
        Vector2 coords = pointAction.action.ReadValue<Vector2>();
        Vector2 mousePos = new();
        mousePos.x += coords.x * camSpeed.x * Time.deltaTime;
        mousePos.y += coords.y * camSpeed.y * Time.deltaTime;
        return mousePos;
    }

    public static Quaternion CalcCamLocalRotation(Transform transform, InputActionReference deltaAction, CameraData camData) {
        Vector2 mv = GetCameraMovementValues(deltaAction, new(camData.xSpeed, -camData.ySpeed));
        // Pega a rotação atual
        Vector3 euler = transform.localEulerAngles;
        // Converte X de 0–360 para -180–180
        if (euler.x > 180f) euler.x -= 360f;
        // Aplica delta vertical do input e clamp
        float newRotX = Mathf.Clamp(euler.x + mv.y, camData.yMinLimit, camData.yMaxLimit);
        // Mantém horizontal (Y) atual
        float newRotY = euler.y + mv.x;
        // Aplica a rotação final
        return Quaternion.Euler(newRotX, newRotY, 0f);
    }
}

/// <summary>
/// This attribute can only be applied to fields because its
/// associated PropertyDrawer only operates on fields (either
/// public or tagged with the [SerializeField] attribute) in
/// the target MonoBehaviour.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute {
    public static float kDefaultButtonWidth = 180;

    public readonly string MethodName;

    private float _buttonWidth = kDefaultButtonWidth;
    public float ButtonWidth {
        get { return _buttonWidth; }
        set { _buttonWidth = value; }
    }

    public InspectorButtonAttribute(string MethodName) {
        this.MethodName = MethodName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer {
    private MethodInfo _eventMethodInfo = null;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {
        InspectorButtonAttribute inspectorButtonAttribute = (InspectorButtonAttribute) attribute;
        Rect buttonRect = new Rect(
            position.x + (position.width - inspectorButtonAttribute.ButtonWidth) * 0.5f, position.y, inspectorButtonAttribute.ButtonWidth,
            position.height
        );
        if (GUI.Button(buttonRect, label.text)) {
            System.Type eventOwnerType = prop.serializedObject.targetObject.GetType();
            string eventName = inspectorButtonAttribute.MethodName;

            if (_eventMethodInfo == null) {
                _eventMethodInfo = eventOwnerType.GetMethod(
                    eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                );
            }

            if (_eventMethodInfo != null) {
                _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
            } else {
                Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", eventName, eventOwnerType));
            }
        }
    }
}
#endif