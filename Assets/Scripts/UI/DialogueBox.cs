using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textHeader;
    [SerializeField] private TextMeshProUGUI textParagraph;
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;

    [Header("Portrait Sprites")]
    [Tooltip("Sprite shown for the left speaker (e.g. player)")]
    [SerializeField] private Sprite leftProfileSprite;
    [Tooltip("Sprite shown for the right speaker (e.g. NPC)")]
    [SerializeField] private Sprite rightProfileSprite;

    [Header("Dialogue Source")]
    [Tooltip("TextAsset containing JSON (like Assets/Dialogues/BossFight.json)")]
    [SerializeField] private TextAsset dialogueJson;

    private InputActionReference advanceAction;

    [Header("Typing")]
    [SerializeField] private float textSpeed = 0.03f;

    public bool IsPlaying { get; private set; }

    private List<Line> lines = new List<Line>();
    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (textHeader != null) textHeader.text = "";
        if (textParagraph != null) textParagraph.text = "";
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (advanceAction != null && advanceAction.action != null)
            advanceAction.action.Enable();
    }

    private void OnDisable()
    {
        if (advanceAction != null && advanceAction.action != null)
            advanceAction.action.Disable();
    }

    private void PrintAllDialogue()
    {
        foreach (var line in lines)
        {
            Debug.Log($"{line.speaker}: {line.text}");
        }
    }

    private void Update()
    {
        if (!IsPlaying) return;

        if (IsAdvancePressed())
        {
            if (isTyping)
            {
                CompleteLine();
            }
            else
            {
                NextLine();
            }
        }
    }

    private bool IsAdvancePressed()
    {
        if (advanceAction != null && advanceAction.action != null)
            return advanceAction.action.triggered;

        if (Keyboard.current != null)
            return Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame;

        return false;
    }

    // Public: start playing the assigned TextAsset. If not assigned, does nothing.
    public void PlayAssigned()
    {
        if (dialogueJson == null)
        {
            Debug.LogWarning("No dialogue JSON assigned to DialogueBox.");
            return;
        }

        Debug.Log("DialogueBox: Playing assigned dialogue JSON." + dialogueJson.text);
        ParseJson(dialogueJson.text);
        StartDialogue();
    }

    // Public: allow external wiring of the JSON and portrait sprites before playing
    public void Setup(TextAsset json, Sprite leftSprite, Sprite rightSprite, InputActionReference inputAction = null)
    {
        if (json != null) dialogueJson = json;
        if (leftSprite != null) leftProfileSprite = leftSprite;
        if (rightSprite != null) rightProfileSprite = rightSprite;
        if (inputAction != null) advanceAction = inputAction;
    }

    // Public: start playing from raw JSON string
    public void PlayFromJson(string json)
    {
        ParseJson(json);
        StartDialogue();
    }

    private void ParseJson(string json)
    {
        lines.Clear();

        if (string.IsNullOrEmpty(json)) return;

        // Preferred: expect JSON shaped for JsonUtility:
        // { "conversation": "boss_cutscene", "lines": [ { "speaker":"Player","text":"..." }, ... ] }
        DialogueData data = null;
        try
        {
            data = JsonUtility.FromJson<DialogueData>(json);
        }
        catch
        {
            data = null;
        }

        if (data != null && data.lines != null && data.lines.Length > 0)
        {
            foreach (var l in data.lines)
            {
                lines.Add(new Line { speaker = l.speaker, text = l.text });
            }
            return;
        }

        PrintAllDialogue();

        Debug.LogWarning("DialogueBox: Failed to parse JSON. Update your JSON to the expected shape for JsonUtility. Example:\n{\n  \"conversation\":\"boss_cutscene\",\n  \"lines\":[{\"speaker\":\"Player\",\"text\":\"Hello\"}]\n}");
    }

    private void StartDialogue()
    {
        if (lines.Count == 0)
        {
            Debug.LogWarning("DialogueBox: No lines parsed.");
            return;
        }

        index = 0;
        IsPlaying = true;
        gameObject.SetActive(true);
        ShowLine(lines[index]);
    }

    private void ShowLine(Line line)
    {
        if (textHeader != null) textHeader.text = line.speaker;

        // Portrait deciding: if speaker matches leftProfile name (player), show left sprite, otherwise right
        bool isLeft = IsLeftSpeaker(line.speaker);
        if (leftPortraitImage != null) leftPortraitImage.enabled = isLeft;
        if (rightPortraitImage != null) rightPortraitImage.enabled = !isLeft;

        if (isLeft && leftPortraitImage != null && leftProfileSprite != null)
            leftPortraitImage.sprite = leftProfileSprite;
        if (!isLeft && rightPortraitImage != null && rightProfileSprite != null)
            rightPortraitImage.sprite = rightProfileSprite;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    // Simple rule: if speaker equals "Player" (case-insensitive) it's left. You can change this check if needed.
    private bool IsLeftSpeaker(string speaker)
    {
        if (string.IsNullOrEmpty(speaker)) return false;
        return speaker.Trim().ToLower() == "player";
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (textParagraph == null)
        {
            Debug.LogWarning("DialogueBox: textParagraph is not assigned. Dialogue text will be logged to Console.");
            Debug.Log(text);
            isTyping = false;
            yield break;
        }

        textParagraph.text = "";

        foreach (char c in text)
        {
            textParagraph.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    private void CompleteLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (index >= 0 && index < lines.Count)
        {
            if (textParagraph != null)
                textParagraph.text = lines[index].text;
            else
                Debug.Log(lines[index].text);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }

        ShowLine(lines[index]);
    }

    private void EndDialogue()
    {
        IsPlaying = false;
        gameObject.SetActive(false);
        OnDialogueEnd?.Invoke();
    }

    // Invoked when the dialogue finishes
    public System.Action OnDialogueEnd;

    [System.Serializable]
    private class Line
    {
        public string speaker;
        public string text;
    }

    [System.Serializable]
    private class DialogueData
    {
        public string conversation;
        public SerializableLine[] lines;
    }

    [System.Serializable]
    private class DialogueArrayWrapper
    {
        public SerializableLine[] lines;
    }

    [System.Serializable]
    private class SerializableLine
    {
        public string speaker;
        public string text;
    }
}
