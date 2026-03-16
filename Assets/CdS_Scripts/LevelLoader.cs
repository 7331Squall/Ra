using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    public void ChangeLevel(string levelName) => SceneManager.LoadScene(levelName);
}