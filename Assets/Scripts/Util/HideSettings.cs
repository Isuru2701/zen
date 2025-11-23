using UnityEngine;

public class HideSettings : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Tick this box only if component should be hidden in normal mode.")]
    private bool hideNormally = false;
    private bool flag = false;

    // private void OnEnable()
    // {
 
    //     GameEvents.OnGameModeChanged += OnGameModeChanged;
    // }

    // private void OnDisable()
    // {
 
    //     GameEvents.OnGameModeChanged -= OnGameModeChanged;
    // }

    private void Start()
    {

        GameEvents.OnGameModeChanged += OnGameModeChanged;
        Debug.Log(hideNormally);
        if(hideNormally)
        ToggleVisibility(); 
    }

    private void OnGameModeChanged(GameManager.GameMode mode)
    {
        ToggleVisibility();
    }

    private void ToggleVisibility()
    {
        
        gameObject.SetActive(flag);
        flag = !flag;
        
    }
}
