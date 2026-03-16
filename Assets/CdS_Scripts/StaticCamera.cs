using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StaticCamera : MonoBehaviour {
    [SerializeField]
    CameraLocations cams;
    [SerializeField]
    int max, cur;
    [SerializeField]
    CameraData camData;

    [SerializeField]
    InputActionReference deltaAction;
    [SerializeField]
    InputActionReference clickAction;

    Transform _desiredTransform;
    Button _prevButton, _nextButton;
    TMP_Text _camNameLbl;

    [SerializeField]
    CameraState camState;
    [SerializeField, Range(0, 1)]
    float lerpSpeed = 0.25f;

    enum CameraState {
        Idle,
        Automatic,
        Manual,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        GameObject panel = FindObjectsByType<GameObject>(FindObjectsSortMode.None).First(x => x.name == "CameraPanel");
        _prevButton = panel.GetComponentsInChildren<Button>().First(x => x.name == "PrevButton");
        _prevButton.onClick.AddListener(PrevCam);
        _nextButton = panel.GetComponentsInChildren<Button>().First(x => x.name == "NextButton");
        _nextButton.onClick.AddListener(NextCam);
        _camNameLbl = panel.GetComponentsInChildren<TMP_Text>().First(x => x.name == "CamNameLbl");
        cams ??= GetComponent<CameraLocations>();
        max = cams.GetMaxCamPositions() - 1;
        ChangeCam(0);
        transform.SetPositionAndRotation(_desiredTransform.position, _desiredTransform.rotation);
    }


    void ChangeCam(int newCam = -1) {
        if (newCam == -1) return;
        cur = Math.Clamp(newCam, 0, max);
        _desiredTransform = cams.GetCamPosition(cur);
        camState = CameraState.Automatic;
        _prevButton.interactable = cur > 0;
        _nextButton.interactable = cur < max;
        _camNameLbl.text = cams.GetCamName(cur);
    }

    public void NextCam() {
        ChangeCam(cur + 1);
    }

    public void PrevCam() {
        ChangeCam(cur - 1);
    }

    public void Update() {
        bool hasMoved = clickAction.action.inProgress && Utilities.GetObjectsUnderPointer().Count <= 0 && !Utilities.AnyDropdownOpen();
        if (camState == CameraState.Idle && hasMoved)
            camState = CameraState.Manual;
        if (camState == CameraState.Manual && !hasMoved)
            camState = CameraState.Idle;
        switch (camState) {
            case CameraState.Automatic:
                HandleAutomaticMovement();
            break;
            case CameraState.Manual:
                HandleManualMovement();
            break;
        }
    }

    void HandleAutomaticMovement() {
        transform.SetPositionAndRotation(
            Vector3.Lerp(transform.position, _desiredTransform.position, lerpSpeed),
            Quaternion.Lerp(transform.rotation, _desiredTransform.rotation, lerpSpeed)
        );
        if (Quaternion.Angle(transform.rotation, _desiredTransform.rotation) < 0.01f
         && Vector3.Distance(transform.position, _desiredTransform.position) < 0.01f) {
            camState = CameraState.Manual;
        }
    }

    void HandleManualMovement() {
        transform.localRotation = Utilities.CalcCamLocalRotation(transform, deltaAction, camData);
    }
}