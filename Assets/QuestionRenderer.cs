using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class QuestionRenderer : MonoBehaviour
{
    [Header("UI References")]
    public Canvas canvas;                  // assign your Canvas here
    public TextMeshProUGUI questionText;          // your TextMeshProUGUI question field
    public RectTransform overlayParent;    // where blank buttons will be added
    public Button blankButtonPrefab;       // prefab for blank button

    [TextArea]
    public string questionTemplate;        // the question string with underscores

    [Header("Blank Styling")]
    public float paddingHorizontal = 10f;  // extra width on each side
    public float blankHeight = 30f;        // height of each blank button

    [Header("Position Offset")]
    public Vector2 positionOffset = Vector2.zero;
    // e.g. (0, +5) will nudge all blanks up 5 units; (2, -3) nudges right 2, down 3

    private static readonly Regex placeholderRegex = new Regex(@"_+");

    void Start()
    {
        questionText.text = questionTemplate;
        questionText.ForceMeshUpdate();
        PlaceBlanks();
    }

    void PlaceBlanks()
    {
        // Clear old overlays
        foreach (Transform c in overlayParent)
            if (c.name.StartsWith("BlankOverlay_"))
                Destroy(c.gameObject);

        var chars = questionText.textInfo.characterInfo;
        bool inBlank = false;
        int blankStart = 0;

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i].character == '_')
            {
                if (!inBlank)
                {
                    inBlank = true;
                    blankStart = i;
                }
            }
            else if (inBlank)
            {
                CreateBlank(blankStart, i - 1);
                inBlank = false;
            }
        }
        if (inBlank)
            CreateBlank(blankStart, chars.Length - 1);
    }

    void CreateBlank(int startIdx, int endIdx)
    {
        var ciStart = questionText.textInfo.characterInfo[startIdx];
        var ciEnd = questionText.textInfo.characterInfo[endIdx];

        // Midpoint in TMP local space
        Vector3 localBL = (ciStart.bottomLeft + ciEnd.bottomLeft) / 2;
        Vector3 localTR = (ciStart.topRight + ciEnd.topRight) / 2;
        Vector3 localCenter = (localBL + localTR) / 2;

        // World & screen coordinates
        Vector3 worldCenter = questionText.transform.TransformPoint(localCenter);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldCenter);

        // Instantiate the blank button
        var btn = Instantiate(blankButtonPrefab, overlayParent);
        btn.name = $"BlankOverlay_{startIdx}";
        var rt = btn.GetComponent<RectTransform>();

        // Size to match underscore width + padding
        float underscoreWidth = ciEnd.topRight.x - ciStart.bottomLeft.x;
        rt.sizeDelta = new Vector2(underscoreWidth + paddingHorizontal * 2, blankHeight);

        // Center pivot & anchors
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Convert screen to overlay local point
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayParent, screenPoint, canvas.worldCamera, out localPoint);

        // Apply inspector-tweakable offset
        rt.anchoredPosition = localPoint + positionOffset;

        // Wire click to open option panel
        btn.onClick.AddListener(() =>
            OptionPanelManager.Instance.Open(btn.GetComponent<BlankButton>())
        );
    }

}
