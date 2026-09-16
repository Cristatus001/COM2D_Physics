using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ToggleGroup))]
public class RadioQuestion : MonoBehaviour
{
    [Header("UI References")]
    public Toggle correctToggle;
    public Toggle wrongToggle;

    [HideInInspector] public bool IsAnswered { get; private set; }
    Action onAnswered;

    private ToggleGroup toggleGroup;

    void Awake()
    {
        // Ensure there's a ToggleGroup on this GameObject
        toggleGroup = GetComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false; // You must pick one
    }

    /// <summary>
    /// Call from OptionPanelManager.SetupRadioStep, passing in UpdateRadioState.
    /// </summary>
    public void ResetQuestion(Action answeredCallback)
    {
        onAnswered = answeredCallback;
        IsAnswered = false;

        // Clear selection
        toggleGroup.SetAllTogglesOff(true);

        // Assign both toggles to this group
        correctToggle.group = toggleGroup;
        wrongToggle.group = toggleGroup;

        // Reset visuals
        correctToggle.transform.GetChild(0).GetComponent<Image>().color = Color.white;
        wrongToggle.transform.GetChild(0).GetComponent<Image>().color = Color.white;

        // Allow interaction
        correctToggle.interactable = true;
        wrongToggle.interactable = true;

        // Remove old listeners
        correctToggle.onValueChanged.RemoveAllListeners();
        wrongToggle.onValueChanged.RemoveAllListeners();

        // Only invoke when toggles go ON
        correctToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) MarkAnswered();
        });
        wrongToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) MarkAnswered();
        });
    }

    void MarkAnswered()
    {
        if (!IsAnswered) // only first time
        {
            IsAnswered = true;
            onAnswered?.Invoke();
        }
    }

    /// <summary>
    /// Called by OptionPanelManager.OnRadioCheck()
    /// </summary>
    public void Mark()
    {
        // Tint correct green
        correctToggle.transform.GetChild(0).GetComponent<Image>().color = Color.green;
        // Tint wrong red if it was selected
        if (wrongToggle.isOn)
            wrongToggle.transform.GetChild(0).GetComponent<Image>().color = Color.red;

        // Lock them
        correctToggle.interactable = false;
        wrongToggle.interactable = false;
    }
}
