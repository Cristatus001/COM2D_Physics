using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BlankButton : MonoBehaviour
{
    public TMP_Text label;
    public Image background;

    private RectTransform rectTransform;
    private Button button;
    public int Index { get; private set; }

    public TMP_Text Label => label;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    public void Initialize(int idx)
    {
        Index = idx;
        label.text = "";
        background.color = Color.white;
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OptionPanelManager.Instance.Open(this);
        });
    }

    public void SetText(string text)
    {
        label.text = text;
        Resize();
    }

    public void Mark(bool correct)
    {
        background.color = correct ? Color.green : Color.red;
        button.interactable = !correct;
        if (!correct) { label.text = ""; Resize(); }
    }

    void Resize()
    {
        float p = OptionPanelManager.Instance.paddingHorizontal;
        float h = OptionPanelManager.Instance.blankHeight;
        float w = label.preferredWidth + p * 2;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }
}
