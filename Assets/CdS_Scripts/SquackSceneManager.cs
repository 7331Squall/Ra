using System;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpeedSettings;
using static SpeedSetting;
using static IntervalSettings;
using static IntervalSetting;
using static UnityEngine.ParticleSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SquackSceneManager : MonoBehaviour {

#region HUD
    // ReSharper disable once InconsistentNaming
    public Canvas HUD;
    NewDateTimeField _datetimeField;
    NewLatitudeField _latitudeField;
    SimSliderField _simSpeedField;
    SimSliderField _simIntervalField;
    Button _simButton;
    Button _optButton;
#endregion

#region ExternalVariables
    DateTime CurrentTime {
        get => _datetimeField.Value;
        set => _datetimeField.Value = value;
    }
    float Latitude {
        get => _latitudeField.Value;
        set => _latitudeField.Value = value;
    }
#endregion

#region SimulationVariables
    bool _isSimulating;
    bool _doSunTrail;
    public bool DoSunTrail {
        get => _doSunTrail;
        set {
            _doSunTrail = value;
            TryAndResetParticle();
        }
    }
    DateTime _simulationDateTime;
    DateTime _simStartTime;
#endregion

#region Others
    public GameObject lightsGameObject;
    public GameObject constellationGameObject;
    [CanBeNull]
    public GameObject earthGameObject;
    public float sphereRadius = 10f;
    [Tooltip("Visual Multiplier where 1 Unity unit = 1e-9 meters")]
    public float orbitScaleEMinus9 = 1f;
    OrbitalCamera _camera;
    ParticleSystem _lightParticle;
    public Quaternion earthTilt = new(0, 0, -0.203641683f, 0.97904551f);
#endregion

    void Awake() {
        _datetimeField = HUD.GetComponentInChildren<NewDateTimeField>();
        _latitudeField = HUD.GetComponentInChildren<NewLatitudeField>();
        _simButton = HUD.GetComponentsInChildren<Button>().ToList().Find(x => x.name.Contains("SimButton"));
        _optButton = HUD.GetComponentsInChildren<Button>().ToList().Find(x => x.name.Contains("OptButton"));
        _simSpeedField = HUD.GetComponentsInChildren<SimSliderField>().ToList().Find(x => x.name.Contains("SimSpeedField"));
        _simIntervalField = HUD.GetComponentsInChildren<SimSliderField>().ToList().Find(x => x.name.Contains("SimIntervalField"));
        _lightParticle = lightsGameObject.GetComponent<ParticleSystem>();
        _camera = GetComponentInChildren<OrbitalCamera>();
    }

    void Start() {
        CurrentTime = new DateTime(2000, 12, 23, 12, 00, 0);
        Latitude = -23f;
        _simButton.onClick.AddListener(ToggleSimulation);
        _datetimeField.OnValueChanged.AddListener(_ => DataUpdated());
        _simSpeedField.OnValueChanged.AddListener(_ => TryAndResetParticle());
        _simIntervalField.OnValueChanged.AddListener(_ => TryAndResetParticle());
        _latitudeField.OnValueChanged.AddListener(_ => DataUpdated());
        DataUpdated();
    }

    void Update() {
        if (_isSimulating) {
            if (_simulationDateTime.Year == 1999)
                _simulationDateTime = CurrentTime;
            int simSecondsPerSecond = SpeedInSeconds(_simSpeedField.Value);
            double simValue = simSecondsPerSecond * Time.deltaTime;
            _simulationDateTime = _simulationDateTime.AddSeconds(simValue);
            if (_simIntervalField.Value > (int) Continuous) {
                if (TimeBetween(_simStartTime, CurrentTime, _simulationDateTime)) {
                    if (DoSunTrail) _lightParticle.Pause(true);
                    _simulationDateTime = _simulationDateTime.AddDays(IntervalInDays(_simIntervalField.Value));
                    if (DoSunTrail) PlayParticle();
                }
            }
            CurrentTime = _simulationDateTime;
            DataUpdated();
        } else if (_simulationDateTime.Year != 1999) {
            _simulationDateTime = new DateTime(1999, 1, 1, 12, 0, 0);
        }
    }

    static bool TimeBetween(DateTime evalTime, DateTime startTime, DateTime endTime) {
        TimeSpan eval = evalTime.TimeOfDay;
        TimeSpan start = startTime.TimeOfDay;
        TimeSpan end = endTime.TimeOfDay;
        bool differentDays = startTime.Date != endTime.Date;
        return (!differentDays && start < eval && end >= eval) || (differentDays && (start < eval || end >= eval));
    }

    void DataUpdated() {
        TryAndResetParticle();
        (Vector3 position, Quaternion rotation) calc = GPTSolarCalc.GetPositionNOAA(Latitude, CurrentTime);
        lightsGameObject.transform.position = calc.position * sphereRadius;
        lightsGameObject.transform.rotation = calc.rotation;
        if (earthGameObject) {
            // calc = GPTSolarCalc.GetEarthTransform(CurrentTime);
            // earthGameObject.transform.position = new Vector3(calc.position.x, calc.position.y, -calc.position.z) * sphereRadius;
            // earthGameObject.transform.rotation = earthTilt * calc.rotation; // earthPivot?
            earthGameObject.transform.position = GPTEarthCalc.CalculateEarthPosition(CurrentTime, orbitScaleEMinus9);
            earthGameObject.transform.rotation = GPTEarthCalc.CalculateEarthRotation(CurrentTime);
        }
        constellationGameObject.transform.rotation = GPTSolarCalc.OrientationForCelestialPole(Latitude, CurrentTime);
    }

    void TryAndResetParticle() {
        if (!_isSimulating)
            _lightParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void UpdateProps(OrbitalCameraData props) {
        _camera.camData = props ?? new OrbitalCameraData();
        sphereRadius = _camera.camData.sunDistance;
        lightsGameObject.transform.localScale = Vector3.one * sphereRadius / 10f;
        DataUpdated();
    }

    void ToggleSimulation() {
        if (!_isSimulating) {
        }
        AdjustHudForSim();
        MainModule main = _lightParticle.main;
        _simStartTime = CurrentTime;
        if (_simIntervalField.Value == (int) Continuous) {
            main.startLifetime = SpeedInSeconds(OneDay) / SpeedInSeconds(_simSpeedField.Value);
        } else {
            main.startLifetime = float.MaxValue;
        }
        if (_isSimulating && DoSunTrail)
            PlayParticle();
    }

    void PlayParticle() {
        if (!DoSunTrail) return;
        _lightParticle.Play(true);
    }

    void AdjustHudForSim() {
        _isSimulating = !_isSimulating;
        _latitudeField.Interactable = !_isSimulating;
        _datetimeField.Interactable = !_isSimulating;
        _simSpeedField.Interactable = !_isSimulating;
        _simIntervalField.Interactable = !_isSimulating;
        _optButton.interactable = !_isSimulating;
        _simButton.GetComponentInChildren<TMP_Text>().text = _isSimulating ? "Simulando..." : "Simular";
    }
}