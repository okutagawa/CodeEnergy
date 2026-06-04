using System.Linq;
using MyGame.Data;
using UnityEngine;

public class FinalTestPortalInteractable : MonoBehaviour
{
    [SerializeField] private FinalTestController finalTestController;

    public void Interact()
    {
        if (GameState.Instance != null && GameState.Instance.IsWorldEventCompleted(WorldEventKey.ActivatePortal))
            return;

        if (finalTestController == null)
            finalTestController = FindObjectOfType<FinalTestController>(true);
        if (finalTestController == null)
            finalTestController = gameObject.AddComponent<FinalTestController>();

        if (finalTestController == null)
        {
            Debug.LogError("[FinalTestPortal] FinalTestController not found in scene.");
            return;
        }

        int courseId = GameState.Instance != null ? GameState.Instance.GetData().selectedCourseId : -1;
        var course = DataManager.LoadCourses()?.courses?.FirstOrDefault(c => c != null && c.id == courseId);
        if (!FinalTestController.AreAllCourseTasksCompleted(course))
        {
            Debug.LogWarning("[FinalTestPortal] Final test is locked until all selected course tasks are completed.");
            return;
        }

        finalTestController.StartFinalTestForCourse(courseId);
    }
}