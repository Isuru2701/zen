// ...existing code...
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TypewriterTMP : MonoBehaviour
{
    public TMP_Text tmp;
    public float charactersPerSecond = 30f;
    public bool playOnStart = true;

    public UnityEvent onFinished;
    public GameObject nextButton;
    private bool finished = false;

    // New Input System fields:
    // - Optional: assign an InputActionReference created for this scene (recommended if you want to edit bindings visually).
    public InputActionReference skipAction;

    // If skipAction is null, a runtime action will be created using this binding string.
    // Examples: "<Keyboard>/enter", "<Keyboard>/e", "<Mouse>/leftButton", "<Gamepad>/buttonSouth"
    public string runtimeBinding = "<Keyboard>/enter";

    private InputAction runtimeAction; // created when skipAction == null
    private bool skipRequested = false;

    void Awake()
    {
        if (tmp == null) tmp = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // Prefer assigned action
        if (skipAction != null && skipAction.action != null)
        {
            skipAction.action.performed += OnSkipPerformed;
            skipAction.action.Enable();
        }
        else
        {
            // create a temporary action just for this component/scene
            runtimeAction = new InputAction("TypewriterSkip", InputActionType.Button, runtimeBinding);
            runtimeAction.performed += OnSkipPerformed;
            runtimeAction.Enable();
        }
    }

    void OnDisable()
    {
        if (skipAction != null && skipAction.action != null)
        {
            skipAction.action.performed -= OnSkipPerformed;
            skipAction.action.Disable();
        }

        if (runtimeAction != null)
        {
            runtimeAction.performed -= OnSkipPerformed;
            runtimeAction.Disable();
            runtimeAction.Dispose();
            runtimeAction = null;
        }
    }

    void Start()
    {
        if (playOnStart && tmp != null) StartCoroutine(Play());
    }

    public void PlayOnce()
    {
        if (tmp != null) StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        tmp.ForceMeshUpdate();
        int total = tmp.textInfo.characterCount;
        tmp.maxVisibleCharacters = 0;

        float carry = 0f;
        int shown = 0;
        skipRequested = false;

        while (shown < total)
        {
            if (skipRequested)
            {
                tmp.maxVisibleCharacters = total;
                InvokeFinished();
                yield break;
            }

            carry += Time.deltaTime * charactersPerSecond;
            int add = Mathf.FloorToInt(carry);
            if (add > 0)
            {
                shown = Mathf.Min(total, shown + add);
                tmp.maxVisibleCharacters = shown;
                carry -= add;
            }
            yield return null;
        }

        tmp.maxVisibleCharacters = total;
        InvokeFinished();
    }

    void InvokeFinished()
    {
        if (finished) return;
        finished = true;
        if (nextButton != null) nextButton.SetActive(true);
        if (onFinished != null) onFinished.Invoke();
    }

    void OnSkipPerformed(InputAction.CallbackContext ctx)
    {
        skipRequested = true;
    }
}
// ...existing code...