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

    [SerializeField] private PlayerController playerController;

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

    public void PauseGame(bool showUI = true)
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        AudioManager.Instance?.pauseSFX();

        SetInputMapsForPause(true);

        if (showUI && pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        AudioManager.Instance?.resumeSFX();
        SetInputMapsForPause(false);

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(false);
    }

        private void SetInputMapsForPause(bool paused)
    {
        // Find all PlayerInput instances and toggle their action maps.
        // Keeps maps named "UI" (or containing "ui") enabled so UI navigation works.
        var allPlayerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.InstanceID);
        foreach (var pi in allPlayerInputs)
        {
            if (pi == null || pi.actions == null) continue;

            foreach (var map in pi.actions.actionMaps)
            {
                if (map == null) continue;
                bool isUiMap = map.name.Equals("UI", System.StringComparison.OrdinalIgnoreCase)
                               || map.name.ToLowerInvariant().Contains("ui");
                if (isUiMap)
                {
                    if (!map.enabled) map.Enable();
                }
                else
                {
                    if (paused)
                    {
                        if (map.enabled) map.Disable();
                    }
                    else
                    {
                        if (!map.enabled) map.Enable();
                    }
                }
            }
        }
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
