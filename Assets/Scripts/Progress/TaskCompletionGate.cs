using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class TaskCompletionGate : MonoBehaviour
{
    [Header("Gate mode")]
    [SerializeField] private bool useSpecificTaskIds = true;
    [SerializeField] private int requiredCompletedCount = 0;

    [Header("Specific IDs (used when 'useSpecificTaskIds' is enabled)")]
    [SerializeField] private List<int> requiredTaskIds = new List<int>();

    [Header("World events")]
    [SerializeField] private List<string> requiredWorldEvents = new List<string>();
    [SerializeField] private bool requireAllWorldEvents = false;

    public bool IsUnlocked()
    {
        if (GameState.Instance == null)
            return false;

        var completed = GameState.Instance.GetData().completedTaskIds;
        if (completed == null)
            return false;

        bool taskRequirementMet;
        if (useSpecificTaskIds)
        {
            taskRequirementMet = requiredTaskIds != null
                && requiredTaskIds.Count > 0
                && requiredTaskIds.All(id => completed.Contains(id));
        }
        else
        {
            taskRequirementMet = completed.Count >= Mathf.Max(0, requiredCompletedCount);
        }

        return taskRequirementMet || AreWorldEventsCompleted();
    }

    private bool AreWorldEventsCompleted()
    {
        if (requiredWorldEvents == null || requiredWorldEvents.Count == 0)
            return false;

        var validEvents = requiredWorldEvents
            .Select(WorldEventKey.Normalize)
            .Where(worldEvent => worldEvent != WorldEventKey.None)
            .Distinct()
            .ToList();

        if (validEvents.Count == 0)
            return false;

        return requireAllWorldEvents
            ? validEvents.All(GameState.Instance.IsWorldEventCompleted)
            : validEvents.Any(GameState.Instance.IsWorldEventCompleted);
    }
}