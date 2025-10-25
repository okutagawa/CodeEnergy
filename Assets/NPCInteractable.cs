using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    public Vector3 cameraOffsetLocal = new Vector3(-1.5f, 0.6f, 0f);
    public float lookAtHeight = 0.6f;
    public string npcTitle = "NPC Drone";
    public string npcBody = "Привет. Это тестовый диалог.";

    private bool isInteracting = false;

    public void Interact()
    {
        if (isInteracting) return;
        isInteracting = true;
        CameraAndDialogManager.Instance.FocusOnNPC(transform, cameraOffsetLocal, lookAtHeight, OnCameraFocused);
    }

    private void OnCameraFocused()
    {
        DialogManager.Instance.OpenDialog(npcTitle, npcBody, OnDialogClosed);
    }

    private void OnDialogClosed()
    {
        CameraAndDialogManager.Instance.ReturnToOriginal(OnReturned);
    }

    private void OnReturned()
    {
        isInteracting = false;
    }
}
