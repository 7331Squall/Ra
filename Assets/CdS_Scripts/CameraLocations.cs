using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CameraLocation {
    public string name;
    public Transform transform;

    public CameraLocation(string name, Transform transform) {
        this.name = name;
        this.transform = transform;
    }
}

public class CameraLocations : MonoBehaviour {
    [SerializeField]
    List<CameraLocation> locations = new();

    void Awake() {
        locations.Clear();
        foreach (Transform child in transform) {
            // Usa o nome do GameObject como nome do preset
            locations.Add(new CameraLocation(child.name, child));
        }
    }

    public int GetMaxCamPositions() {
        return locations.Count;
    }

    public Transform GetCamPosition(int index) {
        return locations[index].transform;
    }

    public string GetCamName(int index) {
        return locations[index].name;
    }

    public List<CameraLocation> GetAllLocations() {
        return locations;
    }
}