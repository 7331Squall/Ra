using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour {
    List<Entry> _brickEntries;
    List<Entry> _mirrorEntries = new();

    public UIDocument uiDocument;
    public VisualTreeAsset brickSelectTemplate;
    public VisualTreeAsset mirrorSelectTemplate;
    public VisualTreeAsset mirrorAdjustTemplate;
    public VisualTreeAsset statementTemplate;
    public VisualTreeAsset compassTemplate;
    public PyramidSceneManager manager;

    Camera _cam;

    VisualElement _mirrorElement;
    VisualElement _compassElement;
    VisualElement _statementElement;
    Slider _mirrorAdjSlider;
    TextField _mirrorAdjText;
    Button _mirrorCloseBtn;

    Transform _selectedMirrorTransform;
    public float offset = 0f;

    class Entry {
        public Renderer Renderer;
        public VisualElement Element;
    }

    void Start() {
        _cam = Camera.main;
        CreateElements();
    }

    // ---------------------------------
    // 2️⃣ Cria UI para cada objeto
    // ---------------------------------
    void CreateElements() {
        VisualElement root = uiDocument.rootVisualElement;
        CreateSelectors(root, brickSelectTemplate, manager.bricks, out _brickEntries);
        CreateSelectors(root, mirrorSelectTemplate, manager.mirrors, out _mirrorEntries);
        CreateStatement(root);
        CreateCompass(root);
        CreateMirrorPanel(root);
        AssignCallbacks();
    }

    void CreateCompass(VisualElement root) {
        _compassElement = compassTemplate.CloneTree().Q<VisualElement>("compass");
        _compassElement.pickingMode = PickingMode.Ignore;
        root.Add(_compassElement);
    }

    void CreateMirrorPanel(VisualElement root) {
        _mirrorElement = mirrorAdjustTemplate.CloneTree();
        _mirrorAdjSlider = _mirrorElement.Q<Slider>("Slider");
        _mirrorAdjText = _mirrorElement.Q<TextField>("SliderField");
        _mirrorCloseBtn = _mirrorElement.Q<Button>("CloseBtn");
        _mirrorElement.style.display = DisplayStyle.None;
        _mirrorCloseBtn.RegisterCallback<GeometryChangedEvent>(evt => {
                float size = evt.newRect.height;
                _mirrorCloseBtn.style.width = size;
            }
        );
        root.Add(_mirrorElement);
    }

    void CreateStatement(VisualElement root) {
        _statementElement = statementTemplate.CloneTree().Q<VisualElement>("StatementWindow");
        _statementElement.pickingMode = PickingMode.Ignore;
        SetupStatement();
        root.Add(_statementElement);
    }

    void SetupStatement() {
        Label statementLabel = _statementElement.Q<Label>("Statement");
        CultureInfo culture = new("pt-BR");
        string latitude = Mathf.Abs(manager.latitude).ToString("F1", culture);
        string latitudeDirection = manager.latitude > 0 ? "N" : "S";
        string time = manager.SunDateTime.ToString("hh'h'mm", culture);
        string date = manager.SunDateTime.ToString("dd' de 'MMMM", culture);
        string pharaoh = manager.pharaoh;
        statementLabel.text =
            $"O faraó <b>{pharaoh}</b> tem uma pirâmide posicionada à <b>Latitude {latitude}° {latitudeDirection}</b> deseja receber sol em seu "
          + $"sarcófago todo <b>dia {date} às {time}</b>. Faça a luz chegar até sua câmara levando em conta a declinação solar.";
    }

    void ViewSelectors(bool visible) {
        ViewBrickSelectors(visible);
        ViewMirrorSelectors(visible);
    }

    void ViewBrickSelectors(bool visible) {
        Renderer brickSelected = manager.holeObject ? manager.holeObject.GetComponent<Renderer>() : null;
        Func<Renderer, bool> visibility = brickSelected == null ? (_ => visible) : (rend => visible && rend != brickSelected);
        foreach (Entry e in _brickEntries)
            e.Element.style.display = visibility(e.Renderer) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void ViewMirrorSelectors(bool visible) {
        foreach (Entry e in _mirrorEntries)
            e.Element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void AssignCallbacks() {
        foreach (Entry entry in _brickEntries) {
            entry.Element.RegisterCallback<ClickEvent>(_ => {
                    manager.SelectBrick(entry.Renderer);
                    ViewBrickSelectors(true);
                }
            );
        }
        foreach (Entry entry in _mirrorEntries) {
            entry.Element.RegisterCallback<ClickEvent>(_ => {
                    if (!manager.IsMirrorAdjusting) {
                        manager.IsMirrorAdjusting = true;
                        ViewSelectors(false);

                        DisplayMirrorAdjustmentHUD(entry);
                    }
                }
            );
        }
        _mirrorAdjSlider.RegisterValueChangedCallback(evt => {
                SetMirrorAdjTextValue(evt.newValue);
                _selectedMirrorTransform.rotation = Quaternion.Euler(0, 0, evt.newValue);
            }
        );

        _mirrorAdjText.RegisterValueChangedCallback(evt => {
                float newValue = ValidateValue(evt.newValue);
                _mirrorAdjSlider.value = newValue;
                _selectedMirrorTransform.rotation = Quaternion.Euler(0, 0, newValue);
                return;

                float ValidateValue(string valueAsString) {
                    if (!float.TryParse(valueAsString, out float value))
                        return 0f;
                    return Mathf.Repeat(value + 180f, 361f) - 180f;
                }
            }
        );
        _mirrorCloseBtn.RegisterCallback<ClickEvent>(_ => {
                _mirrorElement.style.display = DisplayStyle.None;
                ViewSelectors(true);
                _selectedMirrorTransform = null;
                manager.IsMirrorAdjusting = false;
            }
        );
    }

    void DisplayMirrorAdjustmentHUD(Entry entry) {
        _selectedMirrorTransform = entry.Renderer.transform.parent.Find("MirrorMesh").transform;
        float rotAngle = _selectedMirrorTransform.rotation.eulerAngles.z;
        _mirrorAdjSlider.value = rotAngle;
        SetMirrorAdjTextValue(rotAngle);
        _mirrorElement.style.display = DisplayStyle.Flex;
    }

    void SetMirrorAdjTextValue(float rotAngle) {

        _mirrorAdjText.value = Mathf.RoundToInt(rotAngle).ToString();
    }

    void CreateSelectors(VisualElement root, VisualTreeAsset template, List<Renderer> list, out List<Entry> entryList) {
        entryList = new List<Entry>();
        foreach (Renderer r in list) {
            VisualElement element = template.CloneTree().Q<VisualElement>("marker");

            element.style.position = Position.Absolute;
            element.pickingMode = PickingMode.Position;
            root.Add(element);
            entryList.Add(
                new Entry {
                    Renderer = r,
                    Element = element,
                }
            );
        }
    }

    void Update() {
        AdjustElementsPosition();
    }

    void AdjustElementsPosition() {
        foreach (var e in _brickEntries) {
            FitElementToRenderer(e.Element, e.Renderer, _cam);
        }
        foreach (var e in _mirrorEntries) {
            FitElementToRenderer(e.Element, e.Renderer, _cam);
        }
        Vector3 north = Quaternion.Euler(0, -90, 0) * manager.transform.forward;
        north = manager.transform.rotation * north;

        north.y = 0;
        north.Normalize();

        float angle = Mathf.Atan2(north.x, north.z) * Mathf.Rad2Deg;

        _compassElement.style.rotate = new Rotate(new Angle(-angle + offset, AngleUnit.Degree));
    }

    public static void FitElementToRenderer(VisualElement element, Renderer renderer, Camera cam) {
        Bounds b = renderer.localBounds;

        Vector3 c = b.center;
        Vector3 e = b.extents;

        Transform t = renderer.transform;

        Vector3[] corners = new Vector3[] {
            t.TransformPoint(c + new Vector3(-e.x, -e.y, -e.z)),
            t.TransformPoint(c + new Vector3(-e.x, -e.y, e.z)),
            t.TransformPoint(c + new Vector3(-e.x, e.y, -e.z)),
            t.TransformPoint(c + new Vector3(-e.x, e.y, e.z)),
            t.TransformPoint(c + new Vector3(e.x, -e.y, -e.z)),
            t.TransformPoint(c + new Vector3(e.x, -e.y, e.z)),
            t.TransformPoint(c + new Vector3(e.x, e.y, -e.z)),
            t.TransformPoint(c + new Vector3(e.x, e.y, e.z)),
        };

        var panel = element.panel;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var corner in corners) {
            Vector2 p = RuntimePanelUtils.CameraTransformWorldToPanel(panel, corner, cam);

            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        element.style.position = Position.Absolute;
        element.style.left = minX;
        element.style.top = minY;
        element.style.width = maxX - minX;
        element.style.height = maxY - minY;
    }
}