using UnityEngine;

public class UiFocusOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        CursorUIManager.Instance?.EnterUiFocus();
    }

    private void OnDisable()
    {
        CursorUIManager.Instance?.ExitUiFocus();
    }
}
