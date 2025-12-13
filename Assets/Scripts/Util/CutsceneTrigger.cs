using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private Canvas dialogueBox;
    [Header("Dialogue Source")]
    [Tooltip("TextAsset for this cutscene's JSON dialogue")]
    [SerializeField] private TextAsset dialogueJson;
    [Tooltip("Left profile sprite to pass to the DialogueBox")]
    [SerializeField] private Sprite leftProfileSprite;
    [Tooltip("Right profile sprite to pass to the DialogueBox")]
    [SerializeField] private Sprite rightProfileSprite;
    
    [Header("Dialogue Content")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1f;
        
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool autoAdvance = true;
    
    [Header("Input")]
    [Tooltip("Assign an InputActionReference for advancing dialogue (e.g. a 'Submit' or 'Interact' action).")]
    [SerializeField] private InputActionReference advanceAction;

    private bool isPlayingCutscene = false;
    private bool hasTriggered = false;

    private void Start()
    {
        // Make sure dialogue box is hidden at start
        if (dialogueBox != null)
        {
            dialogueBox.gameObject.SetActive(false);
        }
    }

    // private void OnEnable()
    // {
    //     if (advanceAction != null && advanceAction.action != null)
    //     {
    //         advanceAction.action.Enable();
    //     }
    // }

    // private void OnDisable()
    // {
    //     if (advanceAction != null && advanceAction.action != null)
    //     {
    //         advanceAction.action.Disable();
    //     }
    // }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Debug.Log("CutsceneTrigger: Player entered trigger, starting cutscene.");
            StartCutscene();
        }
    }

    private void StartCutscene()
    {

        isPlayingCutscene = true;
        hasTriggered = triggerOnce;

        // Show dialogue box and try to start the DialogueBox component if present.
        if (dialogueBox != null)
        {
            Debug.Log("CutsceneTrigger: Activating DialogueBox for cutscene.");
            dialogueBox.gameObject.SetActive(true);
            var db = dialogueBox.GetComponent<DialogueBox>();
            if (db != null)
            {
                // Configure the DialogueBox with this trigger's JSON and sprites (if assigned), then play
                Debug.Log("CutsceneTrigger: Found DialogueBox component, setting up dialogue.");
                db.Setup(dialogueJson, leftProfileSprite, rightProfileSprite, advanceAction, autoAdvance, textSpeed, delayBetweenLines);

                // Pause the game while dialogue is playing
                if (PauseManager.Instance != null)
                {
                    PauseManager.Instance.PauseGame(false);
                }

                // Subscribe to dialogue end so we can resume the game
                db.OnDialogueEnd += OnDialogueFinished;

                db.PlayAssigned();
            }
        }

    }

    private void OnDialogueFinished()
    {
        // Resume the game
        if (PauseManager.Instance != null)
            PauseManager.Instance.ResumeGame();

        // Hide dialogue UI
        if (dialogueBox != null)
            dialogueBox.gameObject.SetActive(false);

        // Clear playing flag
        isPlayingCutscene = false;

        // Unsubscribe from any DialogueBox (defensive)
        if (dialogueBox != null)
        {
            var db = dialogueBox.GetComponent<DialogueBox>();
            if (db != null)
                db.OnDialogueEnd -= OnDialogueFinished;
        }
    }


    // Public method to manually start cutscene if needed
    public void TriggerCutscene()
    {
        if (!hasTriggered)
        {
            StartCutscene();
        }
    }
}
