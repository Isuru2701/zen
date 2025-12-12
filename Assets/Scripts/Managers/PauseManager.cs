using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Tooltip("Name of the main menu scene to load when quitting")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Drop a Canvas here (with buttons named ResumeButton, RestartButton, QuitButton) to use a custom pause menu. If empty, a simple runtime menu is created.")]
    public Canvas pauseCanvas;

    [Tooltip("Optional Input Action for pause (performed will toggle pause). If not set, Escape key is used.)")]
    public InputActionReference pauseActionReference;

    private bool createdRuntimeCanvas = false;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        pauseCanvas?.gameObject.SetActive(false);
    }


    void TriggerPause(InputAction.CallbackContext context)
    {

        if (context.performed)
            TogglePause();
    }

    void OnEnable()
    {
        if (pauseActionReference != null && pauseActionReference.action != null)
            pauseActionReference.action.performed += TriggerPause;
    }

    void OnDisable()
    {
        if (pauseActionReference != null && pauseActionReference.action != null)
            pauseActionReference.action.performed -= TriggerPause;
    }

    public bool IsPaused() => isPaused;

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        AudioManager.Instance?.pauseSFX();

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        AudioManager.Instance?.resumeSFX();

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        // unpause first
        ResumeGame();
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.name);
    }

    public void QuitToMainMenu()
    {
        ResumeGame();
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

}
