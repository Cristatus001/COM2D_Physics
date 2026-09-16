using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameUIPanel;

    [Header("DropDown Settings")]
    public RectTransform dropDownPanel;
    public Vector2 hiddenPosition = new Vector2(0, 600);
    public Vector2 shownPosition = new Vector2(0, 0);
    public float slideDuration = 0.5f;

    public Button startButton;
    public Button hideButton;

    void Start()
    {
        // Initialize UI states
        mainMenuPanel.SetActive(true);
        gameUIPanel.SetActive(false);
        dropDownPanel.anchoredPosition = hiddenPosition;

        startButton.onClick.AddListener(OnStartButtonPressed);
        hideButton.onClick.AddListener(OnHideButtonPressed);
    }

    void OnStartButtonPressed()
    {
        mainMenuPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        StartCoroutine(SlidePanel(dropDownPanel, hiddenPosition, shownPosition));
    }

    void OnHideButtonPressed()
    {
        StartCoroutine(SlidePanel(dropDownPanel, shownPosition, hiddenPosition));
    }

    IEnumerator SlidePanel(RectTransform panel, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            panel.anchoredPosition = Vector2.Lerp(from, to, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = to;
    }
}
