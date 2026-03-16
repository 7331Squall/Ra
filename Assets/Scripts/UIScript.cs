using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour {
    List<Entry> entries = new();
    public UIDocument uiDocument;
    public VisualTreeAsset template;
    
    public PyramidSceneManager manager;


    Camera cam;

    class Entry {
        public Renderer renderer;
        public VisualElement element;
    }

    void Start() {
        cam = Camera.main;
        CreateElements();
    }


    // ---------------------------------
    // 2️⃣ Cria UI para cada objeto
    // ---------------------------------
    void CreateElements() {
        var root = uiDocument.rootVisualElement;

        foreach (var r in manager.bricks) {
            VisualElement element = template.CloneTree();

            element.style.position = Position.Absolute;
            root.Add(element);
            // evento de clique
            element.RegisterCallback<ClickEvent>(_ =>
            {
                foreach (var e in entries) {
                    e.element.style.display = (e.element == element ? DisplayStyle.None : DisplayStyle.Flex);
                }
                manager.SelectBrick(r);
            });
            entries.Add(
                new Entry {
                    renderer = r,
                    element = element
                }
            );
        }
    }


    void Update() {
        AdjustElementsPosition();
    }

    void AdjustElementsPosition() {
        foreach (var e in entries) {
            FitElementToRenderer(e.element, e.renderer, cam);
        }
    }

    public static void FitElementToRenderer(VisualElement element, Renderer renderer, Camera cam) {
        Bounds b = renderer.bounds;

        Vector3 c = b.center;
        Vector3 e = b.extents;

        Vector3[] corners = new Vector3[] {
            c + new Vector3(-e.x, -e.y, -e.z),
            c + new Vector3(-e.x, -e.y, e.z),
            c + new Vector3(-e.x, e.y, -e.z),
            c + new Vector3(-e.x, e.y, e.z),
            c + new Vector3(e.x, -e.y, -e.z),
            c + new Vector3(e.x, -e.y, e.z),
            c + new Vector3(e.x, e.y, -e.z),
            c + new Vector3(e.x, e.y, e.z),
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