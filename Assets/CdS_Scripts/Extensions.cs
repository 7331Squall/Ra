using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class Extensions {
    public static void SetOptions(this TMP_Dropdown dropdown, List<string> options) {
        int value = dropdown.value;
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = Mathf.Clamp(value, 0, options.Count - 1);
    }
    
    public static void SetMaterialBoolParameter(this Material material, int parameterName, bool arg0) {
        material.SetInt(parameterName, arg0 ? 1 : 0);
    }

    public static bool IsFinite(this Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
}