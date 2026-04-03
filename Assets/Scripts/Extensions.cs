using UnityEngine.UIElements;

public static partial class Extensions {

    public static void Show(this VisualElement element) {
        element.style.display = DisplayStyle.Flex;
    }

    public static void Hide(this VisualElement element) {
        element.style.display = DisplayStyle.None;
    }

}