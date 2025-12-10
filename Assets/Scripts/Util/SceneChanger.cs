using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Tooltip("Optional list of scenes (by name) you can reference from buttons or other scripts.")]
    public string[] scenes;

    [Tooltip("Index into the 'scenes' array used by parameterless helper methods.")]
    public int defaultSceneIndex = 0;

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadSceneByName: sceneName is null or empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByBuildIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"LoadSceneByBuildIndex: buildIndex {buildIndex} is out of range.");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    public void LoadSceneFromList(int index)
    {
        if (scenes == null)
        {
            Debug.LogError("LoadSceneFromList: scenes array is null.");
            return;
        }

        if (index < 0 || index >= scenes.Length)
        {
            Debug.LogError($"LoadSceneFromList: index {index} is out of range (0..{(scenes.Length - 1)}).");
            return;
        }

        LoadSceneByName(scenes[index]);
    }

    public void LoadDefaultSceneFromList()
    {
        LoadSceneFromList(defaultSceneIndex);
    }

    public void LoadNextScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;
        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("LoadNextScene: already at last scene in build settings.");
            return;
        }

        SceneManager.LoadScene(next);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
