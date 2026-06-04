using UnityEngine;

[RequireComponent(typeof(CharacterController))] // опционально, убрать если не нужно
public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;

    [Header("Hover (outline)")]
    public float hoverRange = 3f;
    public bool useScreenCenter = true; // дл€ FPS ставь true
    public LayerMask hoverMask = ~0;    // ограничь слоем NPC если надо

    // runtime
    private NPCInteractable currentHoveredNpc;
    private Outline currentOutline;
    private Collider currentHoveredCollider;
    private FinalTestPortalInteractable currentHoveredPortal;

    // pause flag
    private bool isPaused = false;

    void Update()
    {
        if (isPaused) return; // не обновл€ем hover и не реагируем на E в паузе

        UpdateHover();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if ((currentHoveredNpc == null && currentHoveredPortal == null) || currentHoveredCollider == null)
            return;

        var cameraTransform = Camera.main != null ? Camera.main.transform : transform;
        Vector3 distanceFrom = cameraTransform.position;
        Vector3 closestPoint = currentHoveredCollider.ClosestPoint(distanceFrom);

        // Distance to collider surface is more stable than transform-to-transform distance.
        float distance = Vector3.Distance(distanceFrom, closestPoint);
        if (distance > interactRange)
            return;

        if (currentHoveredNpc != null) currentHoveredNpc.Interact();
        else currentHoveredPortal?.Interact();
    }

    private void UpdateHover()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = useScreenCenter
            ? cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f))
            : cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, hoverRange, hoverMask))
        {
            var npc = ResolveNpcInteractable(hit.collider);
            var portal = ResolveFinalTestPortalInteractable(hit.collider);
            if (npc == null && portal == null)
            {
                ClearCurrentOutline();
                return;
            }
            if (npc != currentHoveredNpc || portal != currentHoveredPortal)
            {
                ClearCurrentOutline();

                currentHoveredNpc = npc;
                currentHoveredPortal = portal;
                currentHoveredCollider = hit.collider;
                var outlineSource = currentHoveredNpc != null ? currentHoveredNpc.gameObject : currentHoveredPortal.gameObject;
                currentOutline = outlineSource.GetComponent<Outline>()
                                    ?? outlineSource.GetComponentInChildren<Outline>()
                                    ?? outlineSource.GetComponentInParent<Outline>();
                if (currentOutline != null)
                {
                    currentOutline.enabled = true;
                }
                else
                {
                    currentHoveredCollider = hit.collider;
                }
                

                return;
            }
        }

        // если ничего не попало Ч сн€ть текущую подсветку
        ClearCurrentOutline();
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }

        currentHoveredCollider = null;
        currentHoveredNpc = null;
        currentHoveredPortal = null;
    }

    private static NPCInteractable ResolveNpcInteractable(Collider hitCollider)
    {
        if (hitCollider == null)
            return null;

        return hitCollider.GetComponent<NPCInteractable>()
               ?? hitCollider.GetComponentInParent<NPCInteractable>()
               ?? hitCollider.GetComponentInChildren<NPCInteractable>();
    }

    private static FinalTestPortalInteractable ResolveFinalTestPortalInteractable(Collider hitCollider)
    {
        if (hitCollider == null)
            return null;

        var interactable = hitCollider.GetComponent<FinalTestPortalInteractable>()
               ?? hitCollider.GetComponentInParent<FinalTestPortalInteractable>()
               ?? hitCollider.GetComponentInChildren<FinalTestPortalInteractable>();
        if (interactable != null) return interactable;

        var portalState = hitCollider.GetComponent<PortalStateByTasks>()
               ?? hitCollider.GetComponentInParent<PortalStateByTasks>()
               ?? hitCollider.GetComponentInChildren<PortalStateByTasks>();
        return portalState != null ? portalState.gameObject.AddComponent<FinalTestPortalInteractable>() : null;
    }

    // Pause handler
    public void OnGamePaused(bool paused)
    {
        // при паузе снимаем подсветку, чтобы не оставалось включЄнных outline
        if (paused) ClearCurrentOutline();
        isPaused = paused;
    }

    // ќпционально: дл€ визуальной отладки
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        if (Camera.main != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * hoverRange);
        }
    }
}
