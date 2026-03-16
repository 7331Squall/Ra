using System;
using UnityEngine;

[Serializable]
public class CameraData {
    /// <summary>
    /// Horizontal Rotation Speed
    /// </summary>
    [SerializeField]
    public float xSpeed = 120f;
    /// <summary>
    /// Vertical Rotation Speed
    /// </summary>
    [SerializeField]
    public float ySpeed = 120f;
    /// <summary>
    /// Lower Vertical Rotation Limit
    /// </summary>
    [SerializeField]
    public float yMinLimit = 1f;
    /// <summary>
    /// Upper Vertical Rotation Limit
    /// </summary>
    [SerializeField]
    public float yMaxLimit = 90f;
}


[Serializable]
public class OrbitalCameraData : CameraData {
    /// <summary>
    /// if false, camera is in perspective mode.
    /// this does nothing, but is needed for zoom.
    /// </summary>
    [SerializeField]
    public bool isOrthographic;
    /// <summary>
    /// Camera distance to target
    /// </summary>
    [SerializeField]
    public float distance = 20f;
    /// <summary>
    /// Camera Zoom Speed
    /// </summary>
    [SerializeField]
    public float zoomSpeed = 5f; // Velocidade de zoom
    /// <summary>
    /// Minimum Zoom Distance
    /// </summary>
    [SerializeField]
    public float minDistance = 2f; // Zoom mínimo
    /// <summary>
    /// Maximum Zoom Distance
    /// </summary>
    [SerializeField]
    public float maxDistance = 30f; // Zoom máximo
    /// <summary>
    /// Sun Distance
    /// </summary>
    [SerializeField]
    public float sunDistance = 10f;
}