using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StepOptions
{
    [Tooltip("Optional name for this step/question")]
    public string stepName;

    [Tooltip("All possible answers for this step")]
    public List<string> options;
}
