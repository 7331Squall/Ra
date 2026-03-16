using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class OrbitalCamera : MonoBehaviour {
    public Transform target; // Alvo (pode ser vazio no (0,0,0))
    [SerializeField]
    public OrbitalCameraData camData;

    [SerializeField]
    InputActionReference clickAction;
    [SerializeField]
    InputActionReference deltaAction;
    [SerializeField]
    InputActionReference scrollAction;

    [SerializeField]
    public Camera[] cameras;

    [SerializeField]
    public Camera GalacticOverlayCamera;

    enum States {
        Idle,
        MovingMainCam,
        MovingOverlay
    }

    States state = States.Idle;

    void Awake() {
        camData ??= new OrbitalCameraData();
        cameras = GetComponentsInChildren<Camera>();
    }

    void Start() {
        InitTarget();
        InitMouse();
        MoveCamera();
    }

    void InitMouse() {
        // habilita todas as actions ligadas a esta câmera
        clickAction.action.Enable();
        deltaAction.action.Enable();
        scrollAction.action.Enable();
        clickAction.action.started += _ => DefineState(true);
        clickAction.action.canceled += _ => DefineState(false);
        scrollAction.action.performed += _ => ZoomCamera(scrollAction.action.ReadValue<Vector2>().y);
    }

    void InitTarget() {
        if (target) return;
        GameObject go = new("Camera Target") { transform = { position = Vector3.zero } };
        target = go.transform;
    }

    void LateUpdate() {
        switch (state) {
            case States.Idle: return;
            case States.MovingMainCam:
                MoveCamera();
                return;
            case States.MovingOverlay:
                MoveOverlay();
                return;
        }
    }

    void MoveOverlay() {
        if (GalacticOverlayCamera == null) return;
        GalacticOverlayCamera.transform.localRotation = Utilities.CalcCamLocalRotation(GalacticOverlayCamera.transform, deltaAction, camData);
        // cam.transform.position = cam.transform.localRotation + target.position;
    }

    void MoveCamera() {
        transform.localRotation = Utilities.CalcCamLocalRotation(transform, deltaAction, camData);

    }

    void ZoomCamera(float zoom) {
        float scroll = zoom * (Utilities.AnyDropdownOpen() ? 0 : 1);
        TouchControl[] touches = Touchscreen.current.touches.Where(t => t.press.isPressed).ToArray();
        if (touches.Length >= 2) {
            TouchState t1 = touches[0].ReadValue();
            TouchState t2 = touches[1].ReadValue();
            Vector2 prevT1 = t1.startPosition + (t1.position - t1.delta);
            Vector2 prevT2 = t2.startPosition + (t2.position - t2.delta);

            float prevMag = (prevT1 - prevT2).magnitude;
            float currMag = (t1.position - t2.position).magnitude;

            scroll = (prevMag - currMag) * 0.01f;
            // usar scroll aqui
        }
        camData.distance = Mathf.Clamp(camData.distance - scroll * camData.zoomSpeed, camData.minDistance, camData.maxDistance);
        Vector3 negDistance = new(0, 0, -camData.distance);
        if (camData.isOrthographic) {
            foreach (Camera cam in cameras) {
                if (cam.name != "GalacticOverlayCamera") {
                    cam.orthographicSize = camData.distance; // zoom = tamanho ortográfico
                }
            }
            transform.position = Vector3.zero;
        } else {
            transform.position = transform.localRotation * negDistance + target.position;
        }
    }

    void DefineState(bool mouseDown) {
        state = NewState(mouseDown);
        return;

        States NewState(bool mouseDown) {
            if (!mouseDown || Utilities.AnyDropdownOpen())
                return States.Idle;
            List<GameObject> overUI = Utilities.GetObjectsUnderPointer();
            if (overUI.Any(x => x.name == "SolarSystemDiagram"))
                return States.MovingOverlay;
            if (overUI.Count <= 0)
                return States.MovingMainCam;
            return States.Idle;
        }
    }

}