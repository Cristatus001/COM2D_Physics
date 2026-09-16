using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StepData
{
    [Tooltip("Use underscores for blanks; ignored in Step 3.")]
    public string questionTemplate;
    [Tooltip("Options for blanks; ignored in Step 3.")]
    public List<string> options;
    [Tooltip("Correct answers for blanks; ignored in Step 3.")]
    public List<string> correctAnswers;
}

public class OptionPanelManager : MonoBehaviour
{
    public static OptionPanelManager Instance { get; private set; }

    [Header("Common UI")]
    public Canvas canvas;
    public TextMeshProUGUI questionText;
    public RectTransform overlayParent;
    public Button blankButtonPrefab;
    [Range(0, 100)] public float paddingHorizontal = 10f;
    [Range(10, 200)] public float blankHeight = 30f;
    public Vector2 positionOffset;

    [Header("Fill-Blank Controls")]
    public Button fillCheckButton;
    public Button nextButton;

    [Header("Fill-Blank Popup")]
    public GameObject optionPanel;
    public Transform optionContainer;
    public GameObject optionButtonPrefab;

    [Header("Radio (Step 3) Controls")]
    public GameObject radioQuestionsPanel;
    public RadioQuestion[] radioQuestions;  // 3 elements
    public Button radioCheckButton;         // single check/continue

    [Header("Steps Data")]
    public List<StepData> steps;  // size = 6

    public int currentStep;
    List<BlankButton> blankButtons = new List<BlankButton>();
    static readonly Regex placeholderRegex = new Regex(@"_+");

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Fill-blank wiring
        optionPanel.SetActive(false);
        fillCheckButton.interactable = false;
        nextButton.interactable = false;
        fillCheckButton.onClick.AddListener(OnFillCheck);
        nextButton.onClick.AddListener(OnNext);

        // Radio wiring
        radioQuestionsPanel.SetActive(false);
        radioCheckButton.interactable = false;
        radioCheckButton.onClick.AddListener(OnRadioCheck);
    }

    void Start() => LoadStep(0);

    public void LoadStep(int stepIndex)
    {
        currentStep = Mathf.Clamp(stepIndex, 0, steps.Count -1);

        // Hide all UI areas
        optionPanel.SetActive(false);
        overlayParent.gameObject.SetActive(true);
        radioQuestionsPanel.SetActive(false);
        fillCheckButton.interactable = false;
        nextButton.interactable = false;
        radioCheckButton.interactable = false;

        if (currentStep == 2) SetupRadioStep();
         SetupFillStep();
    }

    // ─── STEP 3: RADIO SECTION ───────────────────────────-

    void SetupRadioStep()
    {
        radioQuestionsPanel.SetActive(true);
        foreach (var rq in radioQuestions)
            rq.ResetQuestion(UpdateRadioState);
        UpdateRadioState();
    }

    void UpdateRadioState()
    {
        bool allAnswered = true;
        foreach (var rq in radioQuestions)
            if (!rq.IsAnswered) { allAnswered = false; break; }
        radioCheckButton.interactable = allAnswered;
    }

    void OnRadioCheck()
    {
        // 1) Mark each question
        foreach (var rq in radioQuestions) rq.Mark();

        // 2) Disable the check button to avoid re-click
        radioCheckButton.interactable = false;

        // 3) Wait 5 seconds, then hide panel & load next question
        StartCoroutine(RadioAdvanceAfterDelay(1f));
    }

    IEnumerator RadioAdvanceAfterDelay(float secs)
    {
        yield return new WaitForSeconds(secs);

        // Hide the radio panel
        radioQuestionsPanel.SetActive(false);
        
        // Advance to the next step (fill-in-the-blank)
        
    }

    // ─── FILL-IN-THE-BLANK SECTION ───────────────────────

    void SetupFillStep()
    {
        var data = steps[currentStep];
        overlayParent.gameObject.SetActive(true);
        questionText.text = data.questionTemplate;
        questionText.ForceMeshUpdate();

        // Clear old blanks
        foreach (Transform c in overlayParent)
            if (c.name.StartsWith("BlankOverlay_"))
                Destroy(c.gameObject);
        blankButtons.Clear();
        optionPanel.SetActive(false);

        // Render new blanks
        var chars = questionText.textInfo.characterInfo;
        bool inBlank = false; int start = 0, idx = 0;
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i].character == '_')
            {
                if (!inBlank) { inBlank = true; start = i; }
            }
            else if (inBlank)
            {
                CreateBlank(start, i - 1, idx++);
                inBlank = false;
            }
        }
        if (inBlank) CreateBlank(start, chars.Length - 1, idx);
    }

    void CreateBlank(int s, int e, int idx)
    {
        var ciStart = questionText.textInfo.characterInfo[s];
        var ciEnd = questionText.textInfo.characterInfo[e];
        var localBL = (ciStart.bottomLeft + ciEnd.bottomLeft) / 2f;
        var localTR = (ciStart.topRight + ciEnd.topRight) / 2f;
        var localC = (localBL + localTR) / 2f;

        var worldPt = questionText.transform.TransformPoint(localC);
        var screenPt = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPt);

        var go = Instantiate(blankButtonPrefab.gameObject, overlayParent);
        go.name = $"BlankOverlay_{idx}";
        var bb = go.GetComponent<BlankButton>();
        bb.Initialize(idx);
        blankButtons.Add(bb);

        float width = (ciEnd.topRight.x - ciStart.bottomLeft.x)
                    + paddingHorizontal * 2f;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, blankHeight);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayParent, screenPt, canvas.worldCamera, out Vector2 localPt);
        rt.anchoredPosition = localPt + positionOffset;
    }

    public void Open(BlankButton blank)
    {
        optionPanel.SetActive(true);
        foreach (Transform c in optionContainer) Destroy(c.gameObject);

        foreach (var opt in steps[currentStep].options)
        {
            var btn = Instantiate(optionButtonPrefab, optionContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = opt;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                blank.SetText(opt);
                optionPanel.SetActive(false);
                UpdateFillState();
            });
        }
    }

    void UpdateFillState()
    {
        bool allFilled = true;
        foreach (var b in blankButtons)
            if (string.IsNullOrEmpty(b.Label.text))
                allFilled = false;
        fillCheckButton.interactable = allFilled;
    }

    void OnFillCheck()
    {
        bool allCorrect = true;
        var data = steps[currentStep];
        for (int i = 0; i < blankButtons.Count; i++)
        {
            bool ok = blankButtons[i].Label.text == data.correctAnswers[i];
            blankButtons[i].Mark(ok);
            if (!ok) allCorrect = false;
        }
        nextButton.interactable = allCorrect;
    }

    void OnNext()
    {
        // Hide the overlay if leaving fill-blank
        overlayParent.gameObject.SetActive(false);
        // Load next
        LoadStep(currentStep + 1);
    }
}
