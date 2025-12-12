using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [Header("Dialogue Source")]
    [Tooltip("TextAsset for this cutscene's JSON dialogue")]
    [SerializeField] private TextAsset dialogueJson;
    [Tooltip("Left profile sprite to pass to the DialogueBox")]
    [SerializeField] private Sprite leftProfileSprite;
    [Tooltip("Right profile sprite to pass to the DialogueBox")]
    [SerializeField] private Sprite rightProfileSprite;
    
    [Header("Dialogue Content")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1f;
    
    [Header("Player Reference")]
    [SerializeField] private MonoBehaviour playerController;
    
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool autoAdvance = false;
    
    [Header("Input")]
    [Tooltip("Assign an InputActionReference for advancing dialogue (e.g. a 'Submit' or 'Interact' action).")]
    [SerializeField] private InputActionReference advanceAction;

    private int currentLineIndex = 0;
    private bool isPlayingCutscene = false;
    private bool hasTriggered = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        // Make sure dialogue box is hidden at start
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (advanceAction != null && advanceAction.action != null)
        {
            advanceAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (advanceAction != null && advanceAction.action != null)
        {
            advanceAction.action.Disable();
        }
    }

    private void Update()
    {
        // Allow player to advance dialogue by pressing the mapped input
        if (isPlayingCutscene && !autoAdvance)
        {
            if (IsAdvancePressed())
            {
                if (isTyping)
                {
                    // Skip typing animation and show full text
                    CompleteCurrentLine();
                }
                else
                {
                    // Move to next line
                    ShowNextLine();
                }
            }
        }
    }

    private bool IsAdvancePressed()
    {
        if (advanceAction != null && advanceAction.action != null)
        {
            return advanceAction.action.triggered;
        }

        // Fallback to keyboard (useful in editor if no action is assigned)
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines assigned to CutsceneTrigger!");
            return;
        }

        isPlayingCutscene = true;
        hasTriggered = triggerOnce;
        currentLineIndex = 0;

        // Disable player control
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Show dialogue box and try to start the DialogueBox component if present.
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            var db = dialogueBox.GetComponent<DialogueBox>();
            if (db != null)
            {
                // Configure the DialogueBox with this trigger's JSON and sprites (if assigned), then play
                db.Setup(dialogueJson, leftProfileSprite, rightProfileSprite, advanceAction);
                db.PlayAssigned();
            }
            else
            {
                // Fallback to this trigger's inline dialogue array
                ShowNextLine();
            }
        }
        else
        {
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            string line = dialogueLines[currentLineIndex];
            currentLineIndex++;
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(line));
        }
        else
        {
            // All dialogue finished
            EndCutscene();
        }
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        if (dialogueText != null)
            dialogueText.text = "";
        
        foreach (char c in line)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;

        // If auto-advance is enabled, wait and show next line
        if (autoAdvance)
        {
            yield return new WaitForSeconds(delayBetweenLines);
            ShowNextLine();
        }
    }

    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        if (dialogueText != null && dialogueLines.Length > 0)
            dialogueText.text = dialogueLines[Mathf.Clamp(currentLineIndex - 1, 0, dialogueLines.Length - 1)];
        isTyping = false;
    }

    private void EndCutscene()
    {
        isPlayingCutscene = false;

        // Hide dialogue box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Re-enable player control
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Optionally destroy the trigger after use
        if (triggerOnce)
        {
            Destroy(gameObject);
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
