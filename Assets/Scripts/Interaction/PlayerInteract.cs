using UnityEngine;
using System.Linq;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 2f;

    [Header("Hover (outline)")]
    public float hoverRange = 3f;
    public bool useScreenCenter = true; // дл€ FPS ставь true
    public LayerMask hoverMask = ~0;    // ограничь слоем NPC если надо

    private NPCInteractable currentHoveredNpc;
    private Outline currentOutline;

    void Update()
    {
        UpdateHover();

        if (Input.GetKeyDown(KeyCode.E))
        {
            // —начала попробуем взаимодействовать с ховернутым NPC, если он в пределах interactRange
            if (currentHoveredNpc != null)
            {
                float d = Vector3.Distance(transform.position, currentHoveredNpc.transform.position);
                if (d <= interactRange)
                {
                    currentHoveredNpc.Interact();
                    return;
                }
            }

            // »наче ищем ближайшего NPC в радиусе как запасной вариант
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            NPCInteractable nearest = null;
            float minDist = float.MaxValue;
            foreach (Collider c in colliderArray)
            {
                if (c.TryGetComponent<NPCInteractable>(out var npc))
                {
                    float dist = Vector3.Distance(transform.position, c.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = npc;
                    }
                }
            }

            if (nearest != null)
            {
                nearest.Interact();
            }
        }
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
                        // при желании можно настроить параметры:
                        // currentOutline.OutlineColor = Color.white;
                        // currentOutline.OutlineWidth = 4f;
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(Camera.main != null ? Camera.main.transform.position : transform.position,
                       Camera.main != null ? Camera.main.transform.forward * hoverRange : Vector3.forward * hoverRange);
    }
}
