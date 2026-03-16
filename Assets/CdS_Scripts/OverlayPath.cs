using UnityEngine;

public class OverlayPath : MonoBehaviour {
    
    [Header("SceneManager")]
    public SquackSceneManager sSceneManager;
    public int segments = 512;
    public int radialSegments = 12;
    public float tubeRadius = 0.07f;
    
    void Start() {
        MeshFilter mf = GetComponent<MeshFilter>();
        // MeshRenderer mr = GetComponent<MeshRenderer>();

        mf.mesh = GPTEarthCalc.GenerateOrbitMesh(segments: segments, radialSegments: radialSegments, tubeRadius: tubeRadius, scaleTimesENeg9: sSceneManager.orbitScaleEMinus9);
    }
}