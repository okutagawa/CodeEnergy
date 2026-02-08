using UnityEngine;

/// <summary>
/// Простой компонент для восстановления и сохранения позиции игрока.
/// Повесьте на корневой GameObject игрока (тот, у которого CharacterController).
/// </summary>
[DisallowMultipleComponent]
public class PlayerSaveHandler : MonoBehaviour
{
    [Tooltip("Автосохранение позиции каждые N секунд. 0 — отключено.")]
    [SerializeField] private float autoSaveInterval = 5f;

    private float _timer;

    void Start()
    {
        // Применяем сохранённую позицию, если она не нулевая
        if (GameState.Instance != null)
        {
            // Убедимся, что состояние загружено
            GameState.Instance.LoadState();
            var pos = GameState.Instance.GetData().playerPosition;
            var saved = new Vector3(pos.x, pos.y, pos.z);

            // Если позиция не (0,0,0) — применяем. Если (0,0,0) — считаем, что сохранения нет.
            if (saved != Vector3.zero)
            {
                transform.position = saved;
                Debug.Log($"[PlayerSaveHandler] Applied saved player position {saved}");
            }
            else
            {
                Debug.Log("[PlayerSaveHandler] No saved player position (zero), keeping current transform");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSaveHandler] GameState.Instance is null at Start()");
        }

        _timer = 0f;
    }

    void Update()
    {
        if (autoSaveInterval > 0f)
        {
            _timer += Time.deltaTime;
            if (_timer >= autoSaveInterval)
            {
                _timer = 0f;
                SavePlayerPosition(false);
            }
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SavePlayerPosition(true);
    }

    void OnApplicationQuit()
    {
        SavePlayerPosition(true);
    }

    /// <summary>
    /// Сохраняет текущую позицию в GameState. Если immediate==true — вызывает SaveState() немедленно.
    /// </summary>
    public void SavePlayerPosition(bool immediate)
    {
        if (GameState.Instance == null)
        {
            Debug.LogWarning("[PlayerSaveHandler] GameState.Instance is null, cannot save player position");
            return;
        }

        GameState.Instance.SetPlayerPosition(transform.position);
        if (immediate)
        {
            GameState.Instance.SaveState();
            Debug.Log($"[PlayerSaveHandler] Saved player position immediately {transform.position}");
        }
        else
        {
            // Если не immediate — можно отложить запись; но для простоты вызываем SaveState() всё равно,
            // чтобы избежать потери при краше. Если это слишком часто — установи autoSaveInterval=0.
            GameState.Instance.SaveState();
            Debug.Log($"[PlayerSaveHandler] Auto-saved player position {transform.position}");
        }
    }
}
