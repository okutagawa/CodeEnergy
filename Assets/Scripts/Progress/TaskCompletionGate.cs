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

    public bool IsUnlocked()
    {
        if (GameState.Instance == null)
            return false;

        var completed = GameState.Instance.GetData().completedTaskIds;
        if (completed == null)
            return false;

        if (useSpecificTaskIds)
        {
            if (requiredTaskIds == null || requiredTaskIds.Count == 0)
                return false;

            return requiredTaskIds.All(id => completed.Contains(id));
        }

        return completed.Count >= Mathf.Max(0, requiredCompletedCount);
    }
}