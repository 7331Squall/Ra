using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuLevelLoader : MonoBehaviour {

    [Header("PANELS")]
    [Tooltip("The UI Panel parenting all sub menus")]
    public GameObject mainCanvas;
    [Header("LOADING SCREEN")]
    [Tooltip("If this is true, the loaded scene won't load until receiving user input")]
    public bool waitForInput = true;
    public GameObject loadingMenu;
    [Tooltip("The loading bar Slider UI element in the Loading Screen")]
    public Slider loadingBar;
    public TMP_Text loadPromptText;
    public KeyCode userPromptKey;

    public void LoadScene(string scene) {
        if (scene != "") {
            StartCoroutine(LoadAsynchronously(scene));
        }
    }

    // Load Bar synching animation
    IEnumerator LoadAsynchronously(string sceneName) { // scene name is just the name of the current scene being loaded
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation != null) {
            operation.allowSceneActivation = false;
            mainCanvas.SetActive(false);
            loadingMenu.SetActive(true);

            while (!operation.isDone) {
                float progress = Mathf.Clamp01(operation.progress / .95f);
                loadingBar.value = progress;

                if (operation.progress >= 0.9f && waitForInput) {
                    loadPromptText.text = "Press " + userPromptKey.ToString().ToUpper() + " to continue";
                    loadingBar.value = 1;

                    if (Input.GetKeyDown(userPromptKey)) {
                        operation.allowSceneActivation = true;
                    }
                } else if (operation.progress >= 0.9f && !waitForInput) {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}