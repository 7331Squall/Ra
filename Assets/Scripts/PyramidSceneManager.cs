using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PyramidSceneManager : MonoBehaviour {
    public List<Renderer> bricks;
    public GameObject sunObject;
    public GameObject holeObject;
    public float magnitude = 10.0f;

    LineRenderer _sunRay;

    [Header("Sun")]
    public float latitude = 29.9773036766044f;
    public SerializableDateTime sunDateTime = new(new DateTime(2026, 06, 21, 11, 24, 00));

    [InspectorButton("UpdateSun", ButtonWidth = 250), SerializeField]
    private bool updateSun;
    bool _shallUpdate;

    void Awake() {
        sunObject = transform.Find("Sun").gameObject;
        _sunRay = GetComponent<LineRenderer>();
        PopulateBricks();
        UpdateSun();
        if (EditorApplication.isPlaying) _shallUpdate = true;
    }

    void PopulateBricks() {
        bricks.Clear();

        int layer = LayerMask.NameToLayer("Interactable");

        Renderer[] all = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var r in all) {
            if (r.gameObject.layer == layer)
                bricks.Add(r);
        }
    }

    void CastSunRay() {
        if (!holeObject) return;
        sunObject.transform.LookAt(holeObject.transform, Vector3.up);
        Ray ray = new(sunObject.transform.position, sunObject.transform.forward * (magnitude * 2));
        Physics.Raycast(ray, out RaycastHit sunHit);
        List<Vector3> RayVertexes = new();
        RayVertexes.Add(sunObject.transform.position);
        do {
            RayVertexes.Add(sunHit.point);
            if (!sunHit.collider || !sunHit.collider.gameObject) {
                break;
            }
            if (sunHit.collider.gameObject.CompareTag("Pharaoh")) {
                break;
            }
            if (sunHit.collider.gameObject.CompareTag("SunMirror")) {
                var origin = sunHit.point + ray.direction * 0.001f;
                var destination = Vector3.Reflect(ray.direction, sunHit.normal);
                ray = new Ray(origin, destination);
                Physics.Raycast(ray, out sunHit);
            } else {
                break;
            }
        } while (true);

        _sunRay.positionCount = RayVertexes.Count;
        _sunRay.SetPositions(RayVertexes.ToArray());
        if (EditorApplication.isPlaying) _shallUpdate = true;
    }

    void UpdateSun() {

        (Vector3 position, Quaternion rotation) sunCoords = GPTSolarCalc.GetPositionNOAA(latitude, sunDateTime.Value);
        sunCoords.position.z = 0;
        sunObject.transform.position = sunCoords.position * magnitude;
        sunObject.transform.rotation = sunCoords.rotation;
        CastSunRay();

    }

    public void SelectBrick(Renderer brick) {
        foreach (Renderer i in bricks) {
            i.gameObject.SetActive(i != brick);
        }
        holeObject = brick.gameObject;
        UpdateSun();
    }

    // Update is called once per frame
    void Update() {
        if (_shallUpdate) UpdateSun();
    }
}