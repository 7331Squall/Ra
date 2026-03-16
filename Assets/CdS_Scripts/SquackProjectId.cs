using TMPro;
using UnityEngine;

public class SquackProjectId : MonoBehaviour {

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        TMP_Text label = GetComponent<TMP_Text>();
        label.text = label.text.Replace("{ProjectName}", $"{Application.productName}").Replace("{ProjectVersion}", $"v{Application.version}");
    }
}