using UnityEngine;

[RequireComponent(typeof(CharacterController))] // опционально, убрать если не нужно
public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 2f;

    [Header("Hover (outline)")]
    public float hoverRange = 3f;
    public bool useScreenCenter = true; // дл€ FPS ставь true
    public LayerMask hoverMask = ~0;    // ограничь слоем NPC если надо

    // runtime
    private NPCInteractable currentHoveredNpc;
    private Outline currentOutline;

    void Update()
    {
        UpdateHover();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // если есть ховернутый NPC и в пределах interactRange Ч интерактим его
        if (currentHoveredNpc == null) return;

        // »наче ищем ближайшего NPC в радиусе как запасной вариант
        float d = Vector3.Distance(transform.position, currentHoveredNpc.transform.position);
        if (d > interactRange) return;

        currentHoveredNpc.Interact();
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
            var npc = hit.collider.GetComponentInParent<NPCInteractable>();
            if (npc != null)
            {
                if (npc != currentHoveredNpc)
                {
                    ClearCurrentOutline();

                    currentHoveredNpc = npc;
                    currentOutline = currentHoveredNpc.GetComponent<Outline>();
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = true;
                    }
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
        currentHoveredNpc = null;
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
