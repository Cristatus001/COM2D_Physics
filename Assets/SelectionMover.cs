using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMover : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle carToggle;
    public Toggle dotToggle;
    public Button continueButton;
    public Button resetButton;
    public Button nextButton;

    [Header("Objects to Move")]
    public Transform car;
    public Transform dot;

    [Header("Movement Settings")]
    public Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    public float moveDuration = 1.0f;

    private Vector3 carStartPos;
    private Vector3 dotStartPos;

    private int moveTargetsRemaining = 0;

    // Track move combinations
    private bool didCarOnly = false;
    private bool didDotOnly = false;
    private bool didBoth = false;

    void Start()
    {
        carStartPos = car.position;
        dotStartPos = dot.position;

        continueButton.onClick.AddListener(OnContinue);
        resetButton.onClick.AddListener(OnReset);
        nextButton.gameObject.SetActive(false);  // Hide at start
        resetButton.gameObject.SetActive(false); // Hide at start
    }

    void OnContinue()
    {
        moveTargetsRemaining = 0;
        resetButton.gameObject.SetActive(false);
        continueButton.interactable = false;

        bool carSelected = carToggle.isOn;
        bool dotSelected = dotToggle.isOn;

        // Track which combination was used
        if (carSelected && !dotSelected) didCarOnly = true;
        if (!carSelected && dotSelected) didDotOnly = true;
        if (carSelected && dotSelected) didBoth = true;

        // Start movements
        if (carSelected)
        {
            moveTargetsRemaining++;
            StartCoroutine(SmoothMove(car, car.position, car.position + moveOffset, isReset: false));
        }

        if (dotSelected)
        {
            moveTargetsRemaining++;
            StartCoroutine(SmoothMove(dot, dot.position, dot.position + moveOffset, isReset: false));
        }

        // If no objects selected, re-enable continue
        if (moveTargetsRemaining == 0)
        {
            continueButton.interactable = true;
        }

        CheckForLevelUnlock();
    }

    void OnReset()
    {
        // Immediately hide reset button
        resetButton.gameObject.SetActive(false);

        // Reset toggles
        carToggle.isOn = false;
        dotToggle.isOn = false;

        // Reset positions
        moveTargetsRemaining = 0;
        StartCoroutine(SmoothMove(car, car.position, carStartPos, isReset: true));
        StartCoroutine(SmoothMove(dot, dot.position, dotStartPos, isReset: true));
    }

    IEnumerator SmoothMove(Transform obj, Vector3 from, Vector3 to, bool isReset)
    {
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            obj.position = Vector3.Lerp(from, to, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = to;

        if (!isReset)
        {
            moveTargetsRemaining--;
            if (moveTargetsRemaining <= 0)
            {
                resetButton.gameObject.SetActive(true);
            }
        }
        else
        {
            moveTargetsRemaining++;
            if (moveTargetsRemaining == 2) // Only enable continue after both reset moves done
            {
                StartCoroutine(EnableContinueAfterDelay(0.1f));
                moveTargetsRemaining = 0;
            }
        }
    }

    IEnumerator EnableContinueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        continueButton.interactable = true;
    }

    void CheckForLevelUnlock()
    {
        if (didCarOnly && didDotOnly && didBoth)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    // Optional: Call this when transitioning to next level
    public void ResetTrialProgress()
    {
        didCarOnly = false;
        didDotOnly = false;
        didBoth = false;
        nextButton.gameObject.SetActive(false);
    }
}
