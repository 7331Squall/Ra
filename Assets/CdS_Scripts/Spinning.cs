using UnityEngine;

public class Spinning : MonoBehaviour {
    [Tooltip("Velocidade de rotação em graus por segundo")]
    public float rotationSpeed = 90f;
    public Vector3 rotationAxis = new Vector3(0f, 0f, 1f);
    Vector3 vectorToRotate;

    void Start() {
        vectorToRotate = rotationAxis.normalized * rotationSpeed;
    }

    // Update is called once per frame
    void Update() {
        // Rotaciona o objeto no eixo Y
        transform.Rotate(vectorToRotate * Time.deltaTime);
    }
}