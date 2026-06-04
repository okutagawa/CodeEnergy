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

        finalTestController.StartFinalTestForSelectedCourse();
    }
}